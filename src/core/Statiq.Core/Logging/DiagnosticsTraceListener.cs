using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Statiq.Core
{
    // This routes trace and debug messages from the Trace/Debug classes to the specified logger
    internal class DiagnosticsTraceListener : TraceListener
    {
        private static readonly Dictionary<TraceEventType, LogLevel> LogLevelMapping = new Dictionary<TraceEventType, LogLevel>
        {
            { TraceEventType.Verbose, LogLevel.Debug },
            { TraceEventType.Information, LogLevel.Information },
            { TraceEventType.Warning, LogLevel.Warning },
            { TraceEventType.Error, LogLevel.Error },
            { TraceEventType.Critical, LogLevel.Critical }
        };

        private readonly ILogger _logger;

        public DiagnosticsTraceListener(ILogger logger)
        {
            _logger = logger;
        }

        public override void Write(string message) => _logger.LogDebug(message);

        public override void WriteLine(string message) => _logger.LogDebug(message);

        public override void Fail(string message) => _logger.LogError(message);

        public override void Fail(string message, string detailMessage) => _logger.LogError(message + " " + detailMessage);

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data) =>
            TraceData(eventCache, source, eventType, id, new object[] { data });

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("{");
                sb.Append(i);
                sb.Append("}");
            }

            _logger.LogDebug(sb.ToString());
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId) =>
            _logger.LogDebug(message);

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id) =>
            _logger.Log(LogLevelMapping.TryGetValue(eventType, out LogLevel logLevel) ? logLevel : LogLevel.Trace, id.ToString());

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args) =>
            _logger.Log(LogLevelMapping.TryGetValue(eventType, out LogLevel logLevel) ? logLevel : LogLevel.Trace, format, args);

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message) =>
            _logger.Log(LogLevelMapping.TryGetValue(eventType, out LogLevel logLevel) ? logLevel : LogLevel.Trace, message);
    }
}
