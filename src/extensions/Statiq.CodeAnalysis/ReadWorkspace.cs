using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Reads an MSBuild solution or project file and returns all referenced source files as documents.
    /// This module will be executed once and input documents will be ignored if a search path is
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input
    /// document and the resulting output documents will be aggregated.
    /// </summary>
    public abstract class ReadWorkspace : ParallelSyncConfigModule<NormalizedPath>
    {
        private Func<string, bool> _whereProject;
        private Func<IFile, bool> _whereFile;
        private string[] _extensions;

        protected ReadWorkspace(Config<NormalizedPath> path)
            : base(path, false)
        {
        }

        /// <summary>
        /// Filters the project based on name.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the project should be included.</param>
        /// <returns>The current module instance.</returns>
        public ReadWorkspace WhereProject(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _whereProject;
            _whereProject = currentPredicate is null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Filters the source code file based on path.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the source code file should be included.</param>
        /// <returns>The current module instance.</returns>
        public ReadWorkspace WhereFile(Func<IFile, bool> predicate)
        {
            Func<IFile, bool> currentPredicate = _whereFile;
            _whereFile = currentPredicate is null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Filters the source code files based on extension.
        /// </summary>
        /// <param name="extensions">The extensions to include (if defined, any extensions not listed will be excluded).</param>
        /// <returns>The current module instance.</returns>
        public ReadWorkspace WithExtensions(params string[] extensions)
        {
            _extensions = _extensions?.Concat(extensions.Select(x => x.StartsWith(".") ? x : "." + x)).ToArray()
                ?? extensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        /// <summary>
        /// Gets the projects in the workspace (solution or project).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="file">The project file.</param>
        /// <returns>A sequence of Roslyn <see cref="Project"/> instances in the workspace.</returns>
        protected abstract IEnumerable<Project> GetProjects(IExecutionContext context, IFile file);

        protected internal static IAnalyzerResult CompileProject(IExecutionContext context, IProjectAnalyzer analyzer, StringWriter log)
        {
            log.GetStringBuilder().Clear();
            context.LogDebug($"Building project {analyzer.ProjectFile.Path}");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IAnalyzerResult result = analyzer.Build().FirstOrDefault();
            sw.Stop();
            context.LogDebug($"Project {analyzer.ProjectFile.Path} built in {sw.ElapsedMilliseconds} ms");
            if (result?.Succeeded != true)
            {
                context.LogError($"Could not compile project at {analyzer.ProjectFile.Path}");
                context.LogWarning(log.ToString());
                return null;
            }
            return result;
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, NormalizedPath value)
        {
            if (!value.IsNull)
            {
                IFile projectFile = context.FileSystem.GetInputFile(value);
                return GetProjects(context, projectFile)
                    .Where(project => project is object && (_whereProject is null || _whereProject(project.Name)))
                    .AsParallel()
                    .SelectMany(GetProjectDocuments);

                IEnumerable<IDocument> GetProjectDocuments(Project project)
                {
                    context.LogDebug("Read project {0}", project.Name);
                    string assemblyName = project.AssemblyName;
                    return project.Documents
                        .Where(x => !string.IsNullOrWhiteSpace(x.FilePath))
                        .Select(x => context.FileSystem.GetInputFile(x.FilePath))
                        .Where(x => x.Exists && (_whereFile is null || _whereFile(x)) && (_extensions?.Contains(x.Path.Extension) != false))
                        .Select(GetProjectDocument);

                    IDocument GetProjectDocument(IFile file)
                    {
                        context.LogDebug($"Read file {file.Path.FullPath}");
                        return context.CreateDocument(
                            file.Path,
                            null,
                            new MetadataItems
                            {
                                { CodeAnalysisKeys.AssemblyName, assemblyName }
                            },
                            file.GetContentProvider());
                    }
                }
            }

            return Array.Empty<IDocument>();
        }
    }
}
