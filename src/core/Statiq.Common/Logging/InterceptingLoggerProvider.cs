using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public abstract class InterceptingLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _provider;

        public InterceptingLoggerProvider(ILoggerProvider provider)
        {
            _provider = provider.ThrowIfNull(nameof(provider));
        }

        public ILogger CreateLogger(string categoryName) =>
            new InterceptingLogger(this, _provider.CreateLogger(categoryName));

        public void Dispose() => _provider.Dispose();

        protected virtual void Log<TState>(
            ILogger logger,
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter) =>
            logger.Log(logLevel, eventId, state, exception, formatter);

        private class InterceptingLogger : ILogger
        {
            private readonly InterceptingLoggerProvider _provider;
            private readonly ILogger _logger;

            public InterceptingLogger(InterceptingLoggerProvider provider, ILogger logger)
            {
                _provider = provider.ThrowIfNull(nameof(provider));
                _logger = logger.ThrowIfNull(nameof(logger));
            }

            public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter) =>
                _provider.Log(_logger, logLevel, eventId, state, exception, formatter);
        }
    }
}
