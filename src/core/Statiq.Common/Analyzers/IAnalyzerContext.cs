using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Tracks analyzer results and passes information about the current execution state to analyzers.
    /// </summary>
    public interface IAnalyzerContext : IExecutionContext
    {
        /// <summary>
        /// Adds an analyzer result or <c>null</c> if the result does not apply to a single document..
        /// </summary>
        /// <param name="document">The document this result applies to.</param>
        /// <param name="message">The analyzer result message to add.</param>
        void AddAnalyzerResult(IDocument document, string message);

        /// <summary>
        /// Gets the log level for this analyzer and given document.
        /// </summary>
        /// <param name="document">The document to get an effective log level for.</param>
        /// <returns>The effective log level for the current analyzer and given document.</returns>
        LogLevel GetLogLevel(IDocument document);
    }
}
