﻿using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Kari.Utils;
using Microsoft.CodeAnalysis;

namespace Kari.GeneratorCore.Workflow
{
    /// <summary>
    /// aka a pseudoproject. 
    /// Holds metadata about a project.
    /// </summary>
    public class ProjectEnvironmentData
    {
        public NamedLogger Logger { get; init; }

        /// <summary>
        /// Directory with the source files, including the source code and the project files.
        /// </summary>
        public string Directory { get; init; }
        
        /// <summary>
        /// The name of the namespace that the generated code will end up in.
        /// </summary>
        public string GeneratedNamespaceName { get; init; }
        public readonly List<CodeFragment> CodeFragments = new();

        
        /// <summary>
        /// Writes the text to a file with the given file name, 
        /// placed in the directory of this project, with the current /Generated suffix appended to it.
        /// </summary>
        public void AddCodeFragment(CodeFragment fragment)
        {
            lock (CodeFragments)
            {
                CodeFragments.Add(fragment);
            }
        }

        /// <summary>
        /// Is not thread safe.
        /// </summary>
        public void DisposeOfCodeFragments()
        {
            foreach (ref var f in CollectionsMarshal.AsSpan(CodeFragments))
            {
                if (f.AreBytesRentedFromArrayPool)
                    ArrayPool<byte>.Shared.Return(f.Bytes.Array);
            }
            CodeFragments.Clear();
        }
    }

    /// <summary>
    /// Caches symbols for a project.
    /// </summary>
    public class ProjectEnvironment : ProjectEnvironmentData
    {
        public INamespaceSymbol RootNamespace { get; init; }


        // Cached symbols
        public readonly List<INamedTypeSymbol> Types = new List<INamedTypeSymbol>();
        public readonly List<INamedTypeSymbol> TypesWithAttributes = new List<INamedTypeSymbol>();
        public readonly List<IMethodSymbol> MethodsWithAttributes = new List<IMethodSymbol>();

        /// <summary>
        /// Asynchronously collects and caches relevant symbols.
        /// </summary>
        internal async Task Collect()
        {
            // THOUGHT: For monolithic projects, this effectively runs on 1 core.
            return Task.Run(() => {
                foreach (var symbol in RootNamespace.GetMembers())
                {
                    void AddType(INamedTypeSymbol type)
                    {
                        Types.Add(type);
                        
                        if (type.GetAttributes().Length > 0)
                            TypesWithAttributes.Add(type);
                        
                        foreach (var method in type.GetMethods())
                        {
                            if (method.GetAttributes().Length > 0)
                                MethodsWithAttributes.Add(method);
                        }
                    }

                    if (symbol is INamedTypeSymbol type)
                    {
                        AddType(type);
                    }
                    else if (symbol is INamespaceSymbol nspace)
                    {
                        if (independentNamespaceNames.Contains(nspace.Name))
                        {
                            continue;
                        }
                        foreach (var topType in nspace.GetNotNestedTypes())
                        {
                            AddType(topType);
                        }
                    }
                }

                Logger.Log($"Collected {Types.Count} types, {TypesWithAttributes.Count} annotated types, {MethodsWithAttributes.Count} annotated methods.");
            });
        }
    }
}
