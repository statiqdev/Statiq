using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class FailureLoggerProvider : ILoggerProvider, ILogger
    {
        private readonly LogLevel _failureLogLevel;

        private bool _failed;

        public FailureLoggerProvider(LogLevel failureLogLevel)
        {
            _failureLogLevel = failureLogLevel;
        }

        public void Reset() => _failed = false;

        public void ThrowIfFailed()
        {
            if (_failed)
            {
                throw new LogLevelFailureException(_failureLogLevel);
            }
        }

        public ILogger CreateLogger(string categoryName) => this;

        public void Dispose()
        {
        }

        public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel != LogLevel.None && logLevel >= _failureLogLevel)
            {
                _failed = true;
            }
        }
    }
}
