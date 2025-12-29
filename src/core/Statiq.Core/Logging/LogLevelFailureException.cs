using System;
using Microsoft.Extensions.Logging;

namespace Statiq.Core
{
    public class LogLevelFailureException : Exception
    {
        public LogLevelFailureException(LogLevel failureLogLevel)
            : base($"One or more log messages were above the failure threshold of {failureLogLevel}")
        {
        }
    }
}
