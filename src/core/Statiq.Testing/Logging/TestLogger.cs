using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestLogger : ILogger
    {
        private readonly TestLoggerProvider _provider;

        public ConcurrentQueue<TestMessage> Messages { get; }

        public string CategoryName { get; }

        public TestLogger(TestLoggerProvider provider, string categoryName, ConcurrentQueue<TestMessage> messages = null)
        {
            _provider = provider;
            Messages = messages ?? new ConcurrentQueue<TestMessage>();
            CategoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string formatted = formatter(state, exception);
            Messages.Enqueue(new TestMessage(CategoryName, logLevel, eventId, state, exception, formatted));
            TestContext.Out.WriteLine(formatted);
            if ((int)logLevel >= (int)_provider.ThrowLogLevel)
            {
                throw new Exception($"Log level of {logLevel} exceeds {nameof(TestLoggerProvider)} {nameof(TestLoggerProvider.ThrowLogLevel)} of {_provider.ThrowLogLevel}, log message: {formatted}");
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;
    }
}
