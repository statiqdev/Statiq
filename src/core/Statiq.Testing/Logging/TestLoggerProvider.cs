using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<TestMessage> Messages { get; }

        public TestLoggerProvider(ConcurrentQueue<TestMessage> messages = null)
            : this(LogLevel.Warning, messages)
        {
        }

        public TestLoggerProvider(LogLevel throwLogLevel, ConcurrentQueue<TestMessage> messages = null)
        {
            ThrowLogLevel = throwLogLevel;
            Messages = messages ?? new ConcurrentQueue<TestMessage>();
        }

        public LogLevel ThrowLogLevel { get; set; }

        public ILogger CreateLogger(string categoryName) => new TestLogger(this, categoryName, Messages);

        public ILogger CreateLogger() => new TestLogger(this, null, Messages);

        public ILoggerFactory CreateLoggerFactory() => new LoggerFactory(new ILoggerProvider[] { this });

        public void Dispose()
        {
        }
    }
}
