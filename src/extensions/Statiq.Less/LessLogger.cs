using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Statiq.Less
{
    internal class LessLogger : dotless.Core.Loggers.ILogger
    {
        private readonly ILogger _logger;

        public LessLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(dotless.Core.Loggers.LogLevel level, string message)
        {
            switch (level)
            {
                case dotless.Core.Loggers.LogLevel.Info:
                    Info(message);
                    break;
                case dotless.Core.Loggers.LogLevel.Debug:
                    Debug(message);
                    break;
                case dotless.Core.Loggers.LogLevel.Warn:
                    Warn(message);
                    break;
                case dotless.Core.Loggers.LogLevel.Error:
                    Error(message);
                    break;
            }
        }

        public void Info(string message) => _logger.LogInformation(message);

        public void Info(string message, params object[] args) => _logger.LogInformation(message, args);

        public void Debug(string message) => _logger.LogDebug(message);

        public void Debug(string message, params object[] args) => _logger.LogDebug(message, args);

        public void Warn(string message) => _logger.LogWarning(message);

        public void Warn(string message, params object[] args) => _logger.LogWarning(message, args);

        public void Error(string message) => _logger.LogError(message);

        public void Error(string message, params object[] args) => _logger.LogError(message, args);
    }
}
