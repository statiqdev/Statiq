using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Always provides a wrapped <see cref="ILogger"/>.
    /// </summary>
    public class LoggerFactoryLoggerWrapper : ILoggerFactory
    {
        private readonly ILogger _logger;

        public LoggerFactoryLoggerWrapper(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();
    }
}
