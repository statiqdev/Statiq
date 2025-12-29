using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerResult
    {
        public AnalyzerResult(string analyzerName, LogLevel logLevel, IDocument document, string message)
        {
            AnalyzerName = analyzerName;
            LogLevel = logLevel;
            Document = document;
            Message = message;
        }

        public string AnalyzerName { get; }

        public LogLevel LogLevel { get; }

        public IDocument Document { get; }

        public string Message { get; }
    }
}
