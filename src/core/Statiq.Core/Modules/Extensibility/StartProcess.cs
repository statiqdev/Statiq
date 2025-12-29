using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Starts a system process.
    /// </summary>
    /// <remarks>
    /// This module can start both foreground and background processes. If the process
    /// is a foreground process (the default), the module will wait for it to return
    /// and output a document with the process standard output as it's content.
    /// If the process is a background process, the module will fork it and let it run,
    /// but no output document will be generated and it will log with a debug level.
    /// </remarks>
    /// <category name="Extensibility" />
    public class StartProcess : ParallelSyncMultiConfigModule, IDisposable
    {
        /// <summary>
        /// A metadata key that contains the process exit code.
        /// </summary>
        public const string ExitCode = nameof(ExitCode);

        /// <summary>
        /// A metadata key that contains any error data from the process.
        /// </summary>
        public const string ErrorData = nameof(ErrorData);

        // Config keys
        private const string FileName = nameof(FileName);
        private const string Arguments = nameof(Arguments);
        private const string WorkingDirectory = nameof(WorkingDirectory);
        private const string Timeout = nameof(Timeout);
        private const string ContinueOnErrorKey = nameof(ContinueOnErrorKey);
        private const string KeepContentKey = nameof(KeepContentKey);
        private const string EnvironmentVariables = nameof(EnvironmentVariables);
        private const string LogOutputKey = nameof(LogOutputKey);
        private const string LogErrorsKey = nameof(LogErrorsKey);
        private const string HideArgumentsKey = nameof(HideArgumentsKey);

        private readonly ConcurrentBag<ProcessLauncher> _processLaunchers = new ConcurrentBag<ProcessLauncher>();

        private bool _background;
        private bool _onlyOnce;
        private bool _executed;
        private Func<int, bool> _isErrorExitCode = x => x != 0;

        /// <summary>
        /// Starts a process for the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        public StartProcess(Config<string> fileName)
            : this(fileName, Config.FromValue((string)null))
        {
        }

        /// <summary>
        /// Starts a process for the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        public StartProcess(Config<string> fileName, Config<string> arguments)
            : base(
                new Dictionary<string, IConfig>
                {
                    { FileName, fileName.ThrowIfNull(nameof(fileName)) },
                    { Arguments, arguments.ThrowIfNull(nameof(arguments)) },
                    { LogErrorsKey, Config.FromValue(true) }
                },
                false)
        {
        }

        /// <summary>
        /// Appends an argument to the command.
        /// </summary>
        /// <param name="argument">The argument to append.</param>
        /// <param name="quoted">Whether the argument should be quoted or not.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithArgument(Config<string> argument, bool quoted = false)
        {
            argument.ThrowIfNull(nameof(argument));
            return (StartProcess)CombineConfig(Arguments, argument, (first, second) =>
            {
                string space = first is null || second is null ? string.Empty : " ";
                first ??= string.Empty;
                second = second is null ? string.Empty : (quoted ? "\"" + second + "\"" : second);
                return first + space + second;
            });
        }

        /// <summary>
        /// Appends an argument to the command.
        /// </summary>
        /// <param name="name">The name of the argument to append including any prefixes like a dash or slash.</param>
        /// <param name="value">The value of the argument.</param>
        /// <param name="quoted">Whether the value should be quoted or not.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithArgument(Config<string> name, Config<string> value, bool quoted = false) =>
            WithArgument(name.CombineWith(value, (name, value) =>
            {
                string space = name is null || value is null ? string.Empty : " ";
                name ??= string.Empty;
                value = value is null ? string.Empty : (quoted ? "\"" + value + "\"" : value);
                return name + space + value;
            }));

        /// <summary>
        /// Appends an argument to the command.
        /// </summary>
        /// <param name="argument">The argument to append including any prefixes like a dash or slash.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithArgument(Config<StartProcessArgument> argument) =>
            WithArgument(argument.Transform(arg =>
            {
                if (arg is null)
                {
                    return string.Empty;
                }
                string space = arg.Name is null || arg.Value is null ? string.Empty : " ";
                arg.Name ??= string.Empty;
                arg.Value = arg.Value is null ? string.Empty : (arg.Quoted ? "\"" + arg.Value + "\"" : arg.Value);
                return arg.Name + space + arg.Value;
            }));

        /// <summary>
        /// Appends arguments to the command.
        /// </summary>
        /// <param name="arguments">The arguments to append including any prefixes like a dash or slash.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithArguments(Config<IReadOnlyList<StartProcessArgument>> arguments) =>
            WithArgument(arguments.Transform(args =>
            {
                if (args is null)
                {
                    return string.Empty;
                }
                string combined = string.Empty;
                foreach (StartProcessArgument arg in args)
                {
                    if (arg is object)
                    {
                        string space = arg.Name is null || arg.Value is null ? string.Empty : " ";
                        arg.Name ??= string.Empty;
                        arg.Value = arg.Value is null ? string.Empty : (arg.Quoted ? "\"" + arg.Value + "\"" : arg.Value);
                        string prefix = combined.Length > 0 ? " " : string.Empty;
                        combined = combined + prefix + arg.Name + space + arg.Value;
                    }
                }
                return combined;
            }));

        /// <summary>
        /// Sets the working directory to use for the process relative to the root path.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithWorkingDirectory(Config<string> workingDirectory) =>
            (StartProcess)SetConfig(WorkingDirectory, workingDirectory);

        /// <summary>
        /// Sets process-specific environment variables.
        /// </summary>
        /// <param name="environmentVariables">The environment variables to set.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithEnvironmentVariables(Config<IEnumerable<KeyValuePair<string, string>>> environmentVariables)
        {
            environmentVariables.ThrowIfNull(nameof(environmentVariables));
            return (StartProcess)CombineConfig(EnvironmentVariables, environmentVariables, (first, second) => second is null ? first : first?.Concat(second) ?? second);
        }

        /// <summary>
        /// Sets a process-specific environment variable.
        /// </summary>
        /// <param name="environmentVariable">The name and value of the environment variable to set.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithEnvironmentVariable(Config<KeyValuePair<string, string>> environmentVariable)
        {
            environmentVariable.ThrowIfNull(nameof(environmentVariable));
            return WithEnvironmentVariables(environmentVariable.MakeEnumerable());
        }

        /// <summary>
        /// Sets a timeout in milliseconds before the process will be terminated.
        /// </summary>
        /// <remarks>
        /// This has no effect for background processes.
        /// </remarks>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithTimeout(Config<int> timeout) => (StartProcess)SetConfig(Timeout, timeout);

        /// <summary>
        /// Starts the process and leaves it running in the background.
        /// </summary>
        /// <remarks>
        /// If the process is a background process, the module will fork it and let it run,
        /// but no output document will be generated and it will log with a debug level.
        /// </remarks>
        /// <param name="background"><c>true</c> to start this process in the background, <c>false</c> otherwise.</param>
        /// <param name="onlyOnce">
        /// <c>true</c> to start the process the first time the module is executed and not on re-execution,
        /// <c>false</c> to start a new process on every execution.
        /// </param>
        /// <returns>The current module instance.</returns>
        public StartProcess AsBackground(bool background = true, bool onlyOnce = true)
        {
            _background = background;
            _onlyOnce = onlyOnce;
            return this;
        }

        /// <summary>
        /// Only starts the process on the first module execution.
        /// </summary>
        /// <param name="onlyOnce">
        /// <c>true</c> to start the process the first time the module is executed and not on re-execution,
        /// <c>false</c> to start a new process on every execution.
        /// </param>
        /// <returns>The current module instance.</returns>
        public StartProcess OnlyOnce(bool onlyOnce = true)
        {
            _onlyOnce = onlyOnce;
            return this;
        }

        /// <summary>
        /// Toggles whether to hide the arguments list when logging the process command.
        /// </summary>
        /// <param name="hideArguments"><c>true</c> or <c>null</c> to hide the arguments, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess HideArguments(Config<bool> hideArguments = null) => (StartProcess)SetConfig(HideArgumentsKey, hideArguments ?? true);

        /// <summary>
        /// Toggles whether to log standard process output as information messages.
        /// </summary>
        /// <remarks>
        /// By default, standard process output is only logged as debug messages.
        /// </remarks>
        /// <param name="logOutput"><c>true</c> or <c>null</c> to log standard process output as information messages, <c>false</c> to log them as debug messages.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess LogOutput(Config<bool> logOutput = null) => (StartProcess)SetConfig(LogOutputKey, logOutput ?? true);

        /// <summary>
        /// Toggles whether to log error process output as error messages.
        /// </summary>
        /// <remarks>
        /// By default, error process output is logged as error messages.
        /// </remarks>
        /// <param name="logErrors"><c>true</c> or <c>null</c> to log error process output as error messages, <c>false</c> to log them as debug messages.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess LogErrors(Config<bool> logErrors = null) => (StartProcess)SetConfig(LogErrorsKey, logErrors ?? true);

        /// <summary>
        /// Toggles throwing an exception if the process exits with a non-zero exit code.
        /// </summary>
        /// <remarks>
        /// By default the module will throw an exception if the process exits with a non-zero exit code.
        /// </remarks>
        /// <param name="continueOnError"><c>true</c> or <c>null</c> to continue when the process exits with a non-zero exit code, <c>false</c> to throw an exception.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess ContinueOnError(Config<bool> continueOnError = null) => (StartProcess)SetConfig(ContinueOnErrorKey, continueOnError ?? true);

        /// <summary>
        /// Provides a function that determines if the exit code from the process was an error.
        /// </summary>
        /// <remarks>
        /// By default any non-zero exit code is considered an error. Some processes return non-zero
        /// exit codes to indicate success and this lets you treat those as successful.
        /// </remarks>
        /// <param name="isErrorExitCode">A function that determines if the exit code is an error by returning <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithErrorExitCode(Func<int, bool> isErrorExitCode)
        {
            _isErrorExitCode = isErrorExitCode.ThrowIfNull(nameof(isErrorExitCode));
            return this;
        }

        /// <summary>
        /// Keeps the existing document content instead of replacing it with the process output.
        /// </summary>
        /// <remarks>
        /// This has no effect if the process is a background process.
        /// </remarks>
        /// <param name="keepContent">
        /// <c>true</c> or <c>null</c> to keep the existing document content,
        /// <c>false</c> to replace it with the process output.
        /// </param>
        /// <returns>The current module instance.</returns>
        public StartProcess KeepContent(Config<bool> keepContent = null) => (StartProcess)SetConfig(KeepContentKey, keepContent ?? true);

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Only execute once if requested
            if (_onlyOnce && _executed)
            {
                context.LogDebug("Process was configured to execute once, returning input document");
                return input.Yield();
            }
            _executed = true;

            // Get the filename
            string fileName = values.GetString(FileName);
            if (fileName.IsNullOrWhiteSpace())
            {
                context.LogDebug("Provided file name was null or empty, skipping and returning input document");
                return input.Yield();
            }

            // Create the process launcher
            bool continueOnError = values.GetBool(ContinueOnErrorKey);
            bool logOutput = values.GetBool(LogOutputKey);
            bool logErrors = values.GetBool(LogErrorsKey);
            bool hideArguments = values.GetBool(HideArgumentsKey);
            ProcessLauncher processLauncher = new ProcessLauncher(fileName)
            {
                IsBackground = _background,
                ContinueOnError = continueOnError,
                LogOutput = logOutput,
                LogErrors = logErrors,
                HideArguments = hideArguments,
                IsErrorExitCode = _isErrorExitCode
            };
            _processLaunchers.Add(processLauncher);

            // Set arguments
            string arguments = values.GetString(Arguments);
            if (!arguments.IsNullOrEmpty())
            {
                processLauncher.Arguments = arguments;
            }

            // Set working directory
            string workingDirectory = values.GetString(WorkingDirectory);
            if (workingDirectory.IsNullOrWhiteSpace())
            {
                processLauncher.WorkingDirectory = context.FileSystem.RootPath.FullPath;
            }
            else
            {
                processLauncher.WorkingDirectory = context.FileSystem.RootPath.Combine(workingDirectory).FullPath;
            }

            // Set environment variables for the process
            processLauncher.WithEnvironmentVariables(values.GetList(EnvironmentVariables, Array.Empty<KeyValuePair<string, string>>()));

            // Start the process
            bool keepContent = values.GetBool(KeepContentKey);
            using (Stream outputStream = _background || keepContent ? null : context.GetContentStream())
            {
                using (StreamWriter outputWriter = outputStream == null ? null : new StreamWriter(outputStream, leaveOpen: true))
                {
                    using (StringWriter errorWriter = !_background && continueOnError ? new StringWriter() : null)
                    {
                        int exitCode;
                        try
                        {
                            exitCode = processLauncher.StartNew(outputWriter, errorWriter, context.Logger, context, context.CancellationToken);
                        }
                        catch (Exception ex)
                        {
                            throw new LoggedException(ex);
                        }

                        // If this is a background process, let it run and just return the original document
                        if (processLauncher.IsBackground)
                        {
                            return input.Yield();
                        }

                        // Set the metadata items and return
                        MetadataItems metadata = new MetadataItems
                        {
                            { ExitCode, exitCode }
                        };
                        string errorContent = errorWriter?.ToString();
                        if (!errorContent.IsNullOrEmpty())
                        {
                            metadata.Add(ErrorData, errorContent);
                        }
                        return context.CloneOrCreateDocument(
                            input,
                            metadata,
                            outputStream is null ? null : context.GetContentProvider(outputStream))
                            .Yield();
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (ProcessLauncher processLauncher in _processLaunchers)
            {
                processLauncher.Dispose();
            }
        }
    }
}