using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
partial class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.BuildGenerator);


    [Solution] readonly Solution Solution;

    const string KariGeneratorName = "Kari.Generator";
    const string KariAnnotatorName = "Kari.Annotator";
    /*
    AbsolutePath GetProjectPath(string name) => SourceDirectory / name / (name + ".csproj");
    AbsolutePath KariGeneratorProject => GetProjectPath(KariGeneratorName);
    AbsolutePath KariAnnotatorProject => GetProjectPath(KariAnnotatorName);
    */
    AbsolutePath SourceDirectory => RootDirectory / "source";
    


    // public struct PathInfo
    // {
    //     public AbsolutePath BaseOutputPath;
    //     public AbsolutePath OutputPath;
    //     public AbsolutePath BaseIntermediateOutputPath;
    //     public AbsolutePath IntermediateOutputPath;
    // }

    AbsolutePath GetProjectBaseOutputPath(string projectName) => _ops.BuildOutputDirectory / projectName;
    AbsolutePath GetProjectOutputPath(string projectName, string configuration) => GetProjectBaseOutputPath(projectName) / configuration;
    AbsolutePath GetBaseIntermediateOutputPath(string projectName) => _ops.BuildOutputDirectory / projectName;
    AbsolutePath GetIntermediateOutputPath(string projectName, string configuration) => GetBaseIntermediateOutputPath(projectName) / configuration;
    AbsolutePath GetPackageOutputPath(string projectName, string configuration) => _ops.BuildOutputDirectory / projectName / configuration;
    AbsolutePath GetExecutablePath(string projectName, string configuration, string framework = "net6.0") =>
        GetProjectOutputPath(projectName, configuration) / framework / (projectName + (_ops.IsRunningOnWindows ? ".exe" : ""));
    Tool GetAnnotatorTool(string configuration) => 
        ToolResolver.GetLocalTool(GetExecutablePath(KariAnnotatorName, configuration));
    Tool GetGeneratorTool(string configuration) => 
        ToolResolver.GetLocalTool(GetExecutablePath(KariGeneratorName, configuration));

    AbsolutePath InternalPluginsDirectory => SourceDirectory / "Kari.Plugins";

    BuildParameters _ops;
    protected override void OnBuildInitialized()
    {
        _ops = new BuildParameters(this);
        Log.Information("Building version {0} of Kari ({1}) using version {2} of Nuke.",
            _ops.Version,
            Configuration,
            typeof(NukeBuild).Assembly.GetName().Version.ToString());
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            // Files generated by default by Kari.
            SourceDirectory.GlobDirectories("Generated").ForEach(DeleteDirectory);
            // Files generated by default by the annotator.
            SourceDirectory.GlobFiles("*.[gG]enerated.cs").ForEach(DeleteFile);

            EnsureCleanDirectory(_ops.BuildOutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore();
        });

    void CompileProject(string name)
    {
        Project project = Solution.GetProject(name);
        Assert.NotNull(project);

        DotNetBuild(settings => settings
            .SetConfiguration(Configuration)
            .SetProjectFile(project.Path)
            .SetNoRestore(true));
    }

    Target BuildGenerator => _ => _
        .DependsOn(Restore)
        .Executes(() => CompileProject(KariGeneratorName));

    Target BuildAnnotator => _ => _
        .DependsOn(Restore)
        .Executes(() => CompileProject(KariAnnotatorName));

    void CompilePlugins(IEnumerable<string> pluginsToBuild)
    {
        Tool annotatorTool = GetAnnotatorTool(Configuration);

        foreach (var pluginName in pluginsToBuild)
        {
            Project project = Solution.GetProject(pluginName);

            // TODO: find a way to generate the paths to additional generated source files in a props, for each plugin individually??
            // var annotatorArgumentsString = project.GetProperty("KariAnnotatorArguments");
            // string[] arguments = new[]
            // {
            //     "-generatedFilesOutputFolder",
            //     GetProjectBaseOutputPath(pluginName) / "annotations",
            // };
            
            // if (annotatorArgumentsString is null)
            // {
            //     annotatorArguments = 
            annotatorTool.Invoke(workingDirectory: project.Directory);

            CompileProject(pluginName);
        }
    }
    
    // TODO: Use independent builds (unless plugins depend on each other)
    Target BuildPlugins => _ => _
        .DependsOn(BuildAnnotator)
        // .Requires(() => PluginsToBuild)
        .Executes(() =>
        {
            CompilePlugins(PluginsToBuild);
        });

    Target BuildAllPlugins => _ => _
        .DependsOn(BuildAnnotator)
        .Executes(() =>
        {
            var allPluginPaths = Helper.GetAllPluginDirectoryNames(InternalPluginsDirectory);
            CompilePlugins(allPluginPaths);
        });


    // TODO: this should only run tests for the plugins that are passed in??
    // Target RunTests => _ => _
    //     .DependsOn(BuildAnnotator, BuildGenerator, BuildPlugins)
    //     .Executes(() =>
    //     {
    //     });

    
    /*
        Bootstrap current version by building an older version separately.
        Building the tools (the generator and the annotator).
        Building these separately.
        To do that, restore exactly their dependencies (I guess I need separate restores for these).
        Publishing packages to nuget.
        Sharing configuration with external plugins.
        Internal plugins:
            - Running the annotator, taking the configuration from somewhere;
            - Including the file it generates in clean;
            - Make them depend on both the generator and the annotator being compiled.
        External plugins:
            - Sharing configuration somehow (it's possible);
            - Reusing the configuration for internal plugins.
        Tests:
            - Make them depend on the generator having been built;
            - Make them depend on the corresponding plugin having been built (by name? or by property in config file);
            - Make kari run before it's getting compiled;
        Tools:
            - Install Kari as tool globally;
            - Install the annotator as tool;
            - Kari should output help in nuke compatible html;
            - Use nuke for running the code generator and doing Unity builds???
        Using git for versioning.
        Overriding the currently installed global nuget package and the tools with the temp debug version.
        Output files to the intermediate obj folder, and not together with other files.
        
        That's all??
    */
}
