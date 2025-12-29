using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Reads all the source files from a specified MSBuild solution.
    /// This module will be executed once and input documents will be ignored if a search path is
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input
    /// document and the resulting output documents will be aggregated.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// </summary>
    /// <remarks>
    /// The output of this module is similar to executing the ReadFiles module on all source files in the solution.
    /// </remarks>
    /// <metadata cref="CodeAnalysisKeys.AssemblyName" usage="Output" />
    /// <metadata cref="CodeAnalysisKeys.OutputBuildLog" usage="Setting"/>
    /// <category name="Input/Output" />
    public class ReadSolution : ReadWorkspace
    {
        /// <summary>
        /// Reads the solution file at the specified path. This allows you to specify a different solution file depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a <see cref="NormalizedPath"/> with the solution file path.</param>
        public ReadSolution(Config<NormalizedPath> path)
            : base(path)
        {
        }

        /// <summary>
        /// Reads the solution file at the specified path. This allows you to specify a different solution file depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a path with the solution file path.</param>
        public ReadSolution(Config<string> path)
            : this(path?.Transform(x => (NormalizedPath)x))
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<Project> GetProjects(IExecutionContext context, IFile file)
        {
            StringWriter log = new StringWriter();
            AnalyzerManager manager = new AnalyzerManager(file.Path.Parent.FullPath, new AnalyzerManagerOptions
            {
                LogWriter = log
            });

            IAnalyzerResult[] results = manager.Projects.Values
                .Select(analyzer =>
                {
                    if (context.Settings.GetBool(CodeAnalysisKeys.OutputBuildLog))
                    {
                        analyzer.AddBinaryLogger();
                    }
                    return CompileProject(context, analyzer, log);
                })
                .Where(x => x is object)
                .ToArray();

            AdhocWorkspace workspace = new AdhocWorkspace();
            foreach (IAnalyzerResult result in results)
            {
                result.AddToWorkspace(workspace);
            }
            return workspace.CurrentSolution.Projects;
        }
    }
}