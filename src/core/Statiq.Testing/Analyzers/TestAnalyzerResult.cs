using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestAnalyzerResult
    {
        public TestAnalyzerResult(IDocument document, string message)
        {
            Document = document;
            Message = message;
        }

        public IDocument Document { get; }

        public string Message { get; }
    }
}
