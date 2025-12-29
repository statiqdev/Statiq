using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestAnalyzerContext : TestExecutionContext, IAnalyzerContext
    {
        public TestAnalyzerContext()
        {
        }

        public TestAnalyzerContext(params IDocument[] inputs)
            : base(inputs)
        {
        }

        public TestAnalyzerContext(IEnumerable<IDocument> inputs)
            : base(inputs)
        {
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Warning;

        public ConcurrentBag<TestAnalyzerResult> AnalyzerResults { get; } = new ConcurrentBag<TestAnalyzerResult>();

        public void AddAnalyzerResult(IDocument document, string message) => AnalyzerResults.Add(new TestAnalyzerResult(document, message));

        public LogLevel GetLogLevel(IDocument document) => LogLevel;
    }
}
