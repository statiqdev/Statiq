using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public interface IAnalyzer
    {
        /// <summary>
        /// The level at which this analyzer should log.
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// The pipelines and phases this analyzer will be run after.
        /// </summary>
        IEnumerable<KeyValuePair<string, Phase>> PipelinePhases { get; }

        /// <summary>
        /// Called for each analyzer instance before each execution.
        /// </summary>
        /// <remarks>
        /// This is useful because an analyzer that applies to more than one pipeline and phase
        /// will have <see cref="AnalyzeAsync(IAnalyzerContext)"/> called for each of them so
        /// this method is a way to do more general initialization for each execution.
        /// </remarks>
        /// <param name="engine">The engine instance.</param>
        /// <param name="executionId">The ID of the execution that's about to run.</param>
        Task BeforeEngineExecutionAsync(IEngine engine, Guid executionId);

        /// <summary>
        /// Performs analysis.
        /// </summary>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        Task AnalyzeAsync(IAnalyzerContext context);
    }
}
