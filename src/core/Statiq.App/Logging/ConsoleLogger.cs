using System;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConsoleLogger : ILogger
    {
        private readonly ConsoleLoggerProvider _provider;
        private readonly string _categoryName;
        private readonly Func<LogLevel, bool> _filter;

        public ConsoleLogger(ConsoleLoggerProvider provider, string categoryName, Func<LogLevel, bool> filter)
        {
            _provider = provider;
            _categoryName = categoryName;
            _filter = filter;
        }

        public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => _filter(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                _provider.AddMessage(new ConsoleLogMessage(_categoryName, DateTimeOffset.Now, logLevel, eventId, formatter(state, exception), exception, state));
            }
        }
    }
}
