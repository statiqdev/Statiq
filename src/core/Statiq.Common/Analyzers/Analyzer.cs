using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public abstract class Analyzer : IAnalyzer
    {
        /// <inheritdoc/>
        public virtual LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// The pipelines and phases this analyzer will be run after.
        /// </summary>
        /// <remarks>
        /// Set phases in the derived constructor(s).
        /// </remarks>
        public MultiValueDictionary<string, Phase> PipelinePhases { get; } = new MultiValueDictionary<string, Phase>(false, StringComparer.OrdinalIgnoreCase);

        IEnumerable<KeyValuePair<string, Phase>> IAnalyzer.PipelinePhases => PipelinePhases;

        /// <inheritdoc/>
        public virtual Task BeforeEngineExecutionAsync(IEngine engine, Guid executionId) => Task.CompletedTask;

        /// <inheritdoc/>
        public virtual async Task AnalyzeAsync(IAnalyzerContext context) =>
            await context.Inputs.ParallelForEachAsync(
                async input =>
                {
                    // Only analyze the document if it doesn't have a log level of None
                    if (context.GetLogLevel(input) != LogLevel.None)
                    {
                        await AnalyzeDocumentAsync(input, context);
                    }
                },
                context.CancellationToken);

        /// <summary>
        /// Analyzes an individual document.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="AnalyzeAsync(IAnalyzerContext)"/> is overridden.
        /// </remarks>
        /// <param name="document">The document to analyze.</param>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        protected virtual Task AnalyzeDocumentAsync(IDocument document, IAnalyzerContext context) => Task.CompletedTask;
    }
}
