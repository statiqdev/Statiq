using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConsoleLogMessage
    {
        private const string NamespacePrefix = nameof(Statiq) + ".";

        public ConsoleLogMessage(
            string categoryName,
            in DateTimeOffset timestamp,
            LogLevel logLevel,
            in EventId eventId,
            string formattedMessage,
            Exception exception,
            object state)
        {
            CategoryName = categoryName;
            Timestamp = timestamp;
            LogLevel = logLevel;
            EventId = eventId;
            FormattedMessage = formattedMessage;
            Exception = exception;
            State = state;
        }

        public string CategoryName { get; }
        public DateTimeOffset Timestamp { get; }
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public string FormattedMessage { get; }
        public Exception Exception { get; }
        public object State { get; }

        public void GetConsoleContent(List<ConsoleContent> consoleContentBuffer)
        {
            // Add the log level message if not a build server message
            if (CategoryName != typeof(BuildServerLogHelper).FullName)
            {
                string logLevelString = GetLogLevelString(LogLevel);
                consoleContentBuffer.Add(GetLogLevelConsoleContent(LogLevel, logLevelString.AsMemory()));
            }

            // If this is an information message, do some fancy colorization
            if (LogLevel == LogLevel.Information)
            {
                // Find » and color pipelines/modules differently
                int lastAngle = FormattedMessage.LastIndexOf('»');
                if (lastAngle > 0)
                {
                    int firstAngle = FormattedMessage.IndexOf('»');
                    consoleContentBuffer.Add(new ConsoleContent(ConsoleColor.Blue, FormattedMessage.AsMemory(0, firstAngle + 1)));
                    if (firstAngle < lastAngle)
                    {
                        consoleContentBuffer.Add(new ConsoleContent(ConsoleColor.Cyan, FormattedMessage.AsMemory(firstAngle + 1, lastAngle - (firstAngle + 1) + 1)));
                    }
                }
                else
                {
                    lastAngle = -1;
                }

                // Then add the category
                int normalStart = lastAngle + 1;
                if (CategoryName?.StartsWith(NamespacePrefix) == false)
                {
                    consoleContentBuffer.Add(new ConsoleContent(ConsoleColor.DarkGray, $"[{CategoryName}] ".AsMemory()));
                    consoleContentBuffer.Add(new ConsoleContent(FormattedMessage.AsMemory(normalStart, FormattedMessage.Length - normalStart)));
                }
                else
                {
                    // Scan for parenthesis and split into segments (but only if we're not in an external category)
                    int openStart = -1;
                    int openCount = 0;
                    for (int c = normalStart; c < FormattedMessage.Length; c++)
                    {
                        if (FormattedMessage[c] == '(')
                        {
                            if (openCount == 0)
                            {
                                openStart = c;
                            }
                            openCount++;
                        }
                        else if (FormattedMessage[c] == ')')
                        {
                            if (openCount > 0)
                            {
                                openCount--;
                                if (openCount == 0)
                                {
                                    // Ignore "(s)"
                                    if (c < 2 || FormattedMessage[c - 1] != 's' || FormattedMessage[c - 2] != '(')
                                    {
                                        if (openStart > normalStart)
                                        {
                                            consoleContentBuffer.Add(new ConsoleContent(FormattedMessage.AsMemory(normalStart, openStart - normalStart)));
                                        }

                                        // Inside parenthesis
                                        consoleContentBuffer.Add(new ConsoleContent(FormattedMessage.AsMemory(openStart, c - openStart + 1)));

                                        normalStart = c + 1;
                                    }

                                    openStart = -1;
                                }
                            }
                        }
                    }
                    if (normalStart <= FormattedMessage.Length - 1)
                    {
                        consoleContentBuffer.Add(new ConsoleContent(FormattedMessage.AsMemory(normalStart, FormattedMessage.Length - normalStart)));
                    }
                }
            }
            else
            {
                // Add the category
                if (CategoryName?.StartsWith(NamespacePrefix) == false)
                {
                    consoleContentBuffer.Add(new ConsoleContent(ConsoleColor.DarkGray, $"[{CategoryName}] ".AsMemory()));
                }

                // Then add the message
                consoleContentBuffer.Add(GetLogLevelConsoleContent(LogLevel, FormattedMessage.AsMemory()));
            }

            // Write any exceptions
            if (Exception is object)
            {
                consoleContentBuffer.Add(GetLogLevelConsoleContent(LogLevel.Error, (Environment.NewLine + Exception.ToString()).AsMemory()));
            }
        }

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "[TRCE] ",
                LogLevel.Debug => "[DBUG] ",
                LogLevel.Information => "[INFO] ",
                LogLevel.Warning => "[WARN] ",
                LogLevel.Error => "[ERRO] ",
                LogLevel.Critical => "[CRIT] ",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        private ConsoleContent GetLogLevelConsoleContent(LogLevel logLevel, in ReadOnlyMemory<char> message) =>
            logLevel switch
            {
                LogLevel.Critical => new ConsoleContent(ConsoleColor.DarkRed, message),
                LogLevel.Error => new ConsoleContent(ConsoleColor.Red, message),
                LogLevel.Warning => new ConsoleContent(ConsoleColor.Yellow, message),
                LogLevel.Information => new ConsoleContent(ConsoleColor.DarkGreen, message),
                LogLevel.Debug => new ConsoleContent(ConsoleColor.DarkGray, message),
                LogLevel.Trace => new ConsoleContent(ConsoleColor.DarkGray, message),
                _ => new ConsoleContent(message),
            };
    }
}