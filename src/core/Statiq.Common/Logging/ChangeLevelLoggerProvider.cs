using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public class ChangeLevelLoggerProvider : InterceptingLoggerProvider
    {
        private readonly Func<LogLevel, LogLevel> _changeLevel;

        public ChangeLevelLoggerProvider(ILoggerProvider provider, Func<LogLevel, LogLevel> changeLevel)
            : base(provider)
        {
            _changeLevel = changeLevel.ThrowIfNull(nameof(changeLevel));
        }

        protected override void Log<TState>(
            ILogger logger,
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter) =>
            base.Log(logger, _changeLevel(logLevel), eventId, state, exception, formatter);
    }
}
