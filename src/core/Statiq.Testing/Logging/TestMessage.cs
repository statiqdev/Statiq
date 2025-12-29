using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestMessage
    {
        public string CategoryName { get; }
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public object State { get; }
        public Exception Exception { get; }
        public string FormattedMessage { get; }

        public TestMessage(string categoryName, LogLevel logLevel, in EventId eventId, object state, Exception exception, string formatted)
        {
            CategoryName = categoryName;
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
            FormattedMessage = formatted;
        }
    }
}
