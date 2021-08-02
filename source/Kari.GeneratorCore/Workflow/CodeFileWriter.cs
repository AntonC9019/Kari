using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kari.GeneratorCore.Workflow
{
    /// <summary>
    /// Provides an abstraction for writing code files to a given output file.
    /// The file may be written in a different folder or file than requested.
    /// See the implementing classes for the different available options.
    /// </summary>
    public interface IFileWriter : IDisposable
    {
        /// <summary>
        /// Returns a new writer, scoped to the generated output file/directory 
        /// of the given project base directory.
        /// </summary>
        IFileWriter GetWriter(string projectDirectory);

        /// <summary>
        /// Writes the given text to a file.
        /// The `fileNameHint` parameter indicates the desired file name, but the function
        /// gives no guarantees of the actual file the text will be written to.
        /// </summary>
        void WriteCodeFile(string fileNameHint, string text);

        /// <summary>
        /// Destroys all files that were or would have been generated by the given file writer.
        /// </summary>
        void DeleteOutput();
    }

    /// <summary>
    /// Data common to all writers, such as the header and the footer of the generated files.
    /// </summary>
    public static class FileWriterData
    {
        public static readonly Encoding NoBomUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public const string Header = @"
// <auto-generated>
// This file has been autogenerated by Kari.
// </auto-generated>

#pragma warning disable";

        public const string Footer = "#pragma warning restore";
    }

    /// <summary>
    /// Dumps all output in a single file, indicated by `MasterEnvironment.Instance.GeneratedDirectorySuffix`.
    /// Must be initialized after you have set the global MasterEnvironment instance.
    /// </summary>
    public class SingleCodeFileWriter : OneFilePerProjectFileWriter
    {
        public SingleCodeFileWriter(string generatedFilePath) : base(generatedFilePath)
        {
        }

        // Works the same as OneFilePerProjectFileWriter, but returns the first instance every time.
        // That instance will be set to the output file of the RootProject.
        public override IFileWriter GetWriter(string generatedFilePath)
        {
            return this;
        }
    }

    /// <summary>
    /// Dumps the output in a single file, unique per identified project.
    /// The output file is determined by `MasterEnvironment.Instance.GeneratedDirectorySuffix`.
    /// </summary>
    public class OneFilePerProjectFileWriter : IFileWriter, IDisposable
    {
        private readonly string _filePath;
        private StreamWriter? _file;

        public OneFilePerProjectFileWriter(string generatedFilePath)
        {
            _filePath = generatedFilePath;
            var dirName = Path.GetDirectoryName(_filePath);
            Directory.CreateDirectory(dirName);
        }

        private void OpenFile()
        {
            _file = new StreamWriter(_filePath, append: false, FileWriterData.NoBomUtf8);
            _file.Write(FileWriterData.Header);
        }

        public void Dispose()
        {
            if (_file is null) return;
            _file.Write(FileWriterData.Footer);
            _file.Flush();
            _file.Close();
            _file = null;
        }

        public void WriteCodeFile(string fileNameHint, string text)
        {
            if (_file is null) OpenFile();
            _file.WriteLine("// " + fileNameHint);
            _file.Write(text);
        }

        public virtual IFileWriter GetWriter(string filePath)
        {
            return new OneFilePerProjectFileWriter(filePath);
        }

        public void DeleteOutput()
        {
            if (_file != null) _file.Close();
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }

    /// <summary>
    /// Dumps all output in the respective separate files, conforming to the requested file name.
    /// The output is written to the output folder of the specified project.
    /// The output folder is determined by `MasterEnvironment.Instance.GeneratedDirectorySuffix`.
    /// </summary>
    public class SeparateCodeFileWriter : IFileWriter
    {
        private readonly string _baseFolder;

        public SeparateCodeFileWriter(string generatedDirectoryPath)
        {
            _baseFolder = Path.Combine(generatedDirectoryPath);
            Directory.CreateDirectory(_baseFolder);
        }

        public void DeleteOutput()
        {
            if (Directory.Exists(_baseFolder))
            {
                foreach (var file in Directory.EnumerateFiles(_baseFolder, "*.cs", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(file);
                }
            }
        }

        public void Dispose(){}

        public IFileWriter GetWriter(string projectDirectory)
        {
            return new SeparateCodeFileWriter(projectDirectory);
        }

        public void WriteCodeFile(string fileName, string text)
        {
            var path = Path.Combine(_baseFolder, fileName);
            Debug.Assert(!File.Exists(path));

            var file = new StreamWriter(path, append: true, FileWriterData.NoBomUtf8);
            file.Write(FileWriterData.Header);
            file.Write(text);
            file.Write(FileWriterData.Footer);
            file.Flush();
            file.Close();
        }
    }
}