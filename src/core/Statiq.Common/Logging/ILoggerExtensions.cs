using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class ILoggerExtensions
    {
        public static void LogDebug(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Debug, document, message);

        public static void LogTrace(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Trace, document, message);

        public static void LogInformation(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Information, document, message);

        public static void LogWarning(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Warning, document, message);

        public static void LogError(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Error, document, message);

        public static void LogCritical(this ILogger logger, IDocument document, string message) =>
            logger.Log(LogLevel.Critical, document, message);

        public static void Log(this ILogger logger, LogLevel logLevel, IDocument document, string message)
        {
            if (document is object)
            {
                logger.Log(logLevel, default, new StatiqLogState { Document = document }, null, (s, _) => InsertDocumentLogContent(message, s));
            }
            else
            {
                logger.Log(logLevel, message);
            }
        }

        public static void Log<TState>(
            this ILogger logger,
            LogLevel logLevel,
            IDocument document,
            in EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (document is object)
            {
                logger.Log(logLevel, eventId, new StatiqLogState<TState>(state) { Document = document }, exception, (s, e) => InsertDocumentLogContent(formatter(s.InnerState, e), s));
            }
            else
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        public static void Log(this ILogger logger, LogLevel logLevel, StatiqLogState state, string message) =>
            logger.Log(logLevel, default, state, null, (_, __) => message);

        private static string InsertDocumentLogContent(string message, StatiqLogState documentLogState)
        {
            if (documentLogState is object && documentLogState.Document is object)
            {
                string displayString = documentLogState.Document is IDisplayable displayable
                    ? displayable.ToSafeDisplayString()
                    : documentLogState.Document.GetType().Name;
                return message.IsNullOrEmpty()
                    ? $"[{displayString}]"
                    : message.Insert(0, $"[{displayString}] ");
            }
            return message;
        }

        /// <summary>
        /// Logs an appropriate error message for the exception, unwrapping <see cref="AggregateException"/>
        /// and <see cref="TargetInvocationException"/> exceptions and ignoring <see cref="LoggedException"/>.
        /// </summary>
        /// <remarks>
        /// When using in a catch block, you should throw the result of this method instead of the original
        /// exception to ensure the exception is wrapped in a <see cref="LoggedException"/> as it bubbles up
        /// and won't get logged again.
        /// </remarks>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <returns>The original exception or a <see cref="LoggedException"/> wrapping the original exception.</returns>
        public static Exception LogAndWrapException(this ILogger logger, Exception exception)
        {
            logger.ThrowIfNull(nameof(logger));

            if (exception is object && !(exception is OperationCanceledException) && !(exception is LoggedException))
            {
                // Unwrap aggregate and invocation exceptions
                switch (exception)
                {
                    case AggregateException aggregateException:
                        foreach (Exception innerException in aggregateException.InnerExceptions)
                        {
                            logger.LogAndWrapException(innerException);
                        }
                        break;
                    case TargetInvocationException invocationException:
                        if (invocationException.InnerException is object)
                        {
                            logger.LogAndWrapException(invocationException.InnerException);
                        }
                        break;
                    default:
                        logger.LogError(exception.Message);
                        break;
                }
                return new LoggedException(exception);
            }

            return exception;
        }
    }
}
