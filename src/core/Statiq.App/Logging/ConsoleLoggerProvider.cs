using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.App
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private static readonly object WriteLock = new object();
        private static readonly List<ConsoleContent> ConsoleContentBuffer = new List<ConsoleContent>();
        private static readonly ConcurrentBag<ConsoleLoggerProvider> Instances = new ConcurrentBag<ConsoleLoggerProvider>();

        private readonly BlockingCollection<ConsoleLogMessage> _messages =
            new BlockingCollection<ConsoleLogMessage>(new ConcurrentQueue<ConsoleLogMessage>());
        private readonly ManualResetEvent _doneWriting = new ManualResetEvent(true);
        private readonly ManualResetEvent _doneProcessing = new ManualResetEvent(false);
        private readonly BuildServerLogHelper _buildServerLogHelper;
        private readonly LoggerFilterOptions _filterOptions;

        public ConsoleLoggerProvider(IReadOnlyFileSystem fileSystem = null, IOptions<LoggerFilterOptions> filterOptions = null)
        {
            // Switch the console to UTF8 to things like emoji work
            Console.OutputEncoding = Encoding.UTF8;

            _buildServerLogHelper = new BuildServerLogHelper(fileSystem);
            _filterOptions = filterOptions.Value ?? new LoggerFilterOptions();
            Instances.Add(this);

#pragma warning disable VSTHRD110 // Observe the awaitable result of this method call by awaiting it, assigning to a variable, or passing it to another method.
            Task.Run(MessagePumpAsync);
#pragma warning restore VSTHRD110
        }

        private Task MessagePumpAsync()
        {
            ConsoleLogMessage message;
            while ((message = TakeMessage()) is object)
            {
                try
                {
                    WriteMessage(message);
                }
                finally
                {
                    _doneWriting.Set();
                }
            }
            _doneProcessing.Set();
            return Task.CompletedTask;
        }

        private ConsoleLogMessage TakeMessage()
        {
            if (!_messages.IsCompleted)
            {
                try
                {
                    ConsoleLogMessage message = _messages.Take();
                    _doneWriting.Reset();
                    return message;
                }
                catch (InvalidOperationException)
                {
                    // The message collection was completed while waiting
                }
            }
            return null;
        }

        internal void AddMessage(ConsoleLogMessage message)
        {
            if (message.LogLevel != LogLevel.None)
            {
                _messages.Add(message);
            }
        }

        public ILogger CreateLogger(string categoryName) => new ConsoleLogger(this, categoryName, GetFilter(categoryName));

        private void WriteMessage(ConsoleLogMessage message)
        {
            lock (WriteLock)
            {
                // Get and write the main message
                ConsoleContentBuffer.Clear();
                message.GetConsoleContent(ConsoleContentBuffer);
                foreach (ConsoleContent content in ConsoleContentBuffer)
                {
                    Console.ForegroundColor = content.Foreground;
                    Console.Write(content.Message.ToString());
                }
                Console.WriteLine();
                Console.ResetColor();

                // Get and write a build server message if appropriate
                if (_buildServerLogHelper?.IsBuildServer == true && (message.State as StatiqLogState)?.LogToBuildServer == true)
                {
                    string buildServerMessage = _buildServerLogHelper.GetMessage(message.LogLevel, message.State as StatiqLogState, message.FormattedMessage);
                    if (!buildServerMessage.IsNullOrEmpty())
                    {
                        Console.WriteLine(buildServerMessage);
                    }
                }
            }
        }

        public Func<LogLevel, bool> GetFilter(string name)
        {
            foreach (string prefix in GetKeyPrefixes(name))
            {
                if (TryGetSwitch(prefix, out LogLevel level))
                {
                    return l => l >= level;
                }
            }
            return l => l >= _filterOptions.MinLevel;
        }

        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                int lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        private bool TryGetSwitch(string name, out LogLevel level)
        {
            // If we don't have any rules, return false
            if (_filterOptions.Rules.Count == 0)
            {
                level = LogLevel.None;
                return false;
            }

            // Match the rule name or a null rule name for "Default"
            LogLevel? value = _filterOptions.Rules.FirstOrDefault(s => s.CategoryName == name || (name == "Default" && s.CategoryName is null))?.LogLevel;
            if (value is null)
            {
                level = LogLevel.None;
                return false;
            }
            level = value.Value;
            return true;
        }

        /// <summary>
        /// This blocks until all console messages are written, including new ones (so don't add messages while calling this).
        /// </summary>
        public static void FlushAndWait()
        {
            foreach (ConsoleLoggerProvider instance in Instances)
            {
                while (instance._messages.Count > 0)
                {
                }
                instance._doneWriting.WaitOne();
            }
        }

        /// <summary>
        /// Disposes all provider instances.
        /// </summary>
        public static void DisposeAll()
        {
            while (Instances.TryTake(out ConsoleLoggerProvider instance))
            {
                instance.Dispose();
            }
        }

        public void Dispose()
        {
            if (!_messages.IsAddingCompleted)
            {
                try
                {
                    _messages.CompleteAdding();
                    _doneProcessing.WaitOne();
                }
                catch
                {
                }
                _messages.Dispose();
                _doneProcessing.Dispose();
                _doneWriting.Dispose();
            }
        }
    }
}