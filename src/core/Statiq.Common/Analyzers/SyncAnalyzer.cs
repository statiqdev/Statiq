using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncAnalyzer : Analyzer
    {
        public sealed override Task AnalyzeAsync(IAnalyzerContext context)
        {
            Analyze(context);
            return Task.CompletedTask;
        }

        // Unused, prevent overriding in derived classes
        protected sealed override Task AnalyzeDocumentAsync(IDocument document, IAnalyzerContext context) =>
            throw new NotSupportedException();

        protected virtual void Analyze(IAnalyzerContext context) =>
            context.Inputs.AsParallel().ForAll(input => AnalyzeDocument(input, context));

        /// <summary>
        /// Analyzes an individual document.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="Analyze(IAnalyzerContext)"/> is overridden.
        /// </remarks>
        /// <param name="document">The document to analyze.</param>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        protected virtual void AnalyzeDocument(IDocument document, IAnalyzerContext context)
        {
        }
    }
}
