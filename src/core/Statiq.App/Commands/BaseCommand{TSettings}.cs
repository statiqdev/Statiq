using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetEscapades.Extensions.Logging.RollingFile;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    /// <summary>
    /// A base command type that sets up logging and debugging.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings this command uses.</typeparam>
    public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings>, IBaseCommand
        where TSettings : BaseCommandSettings
    {
        private readonly ISettings _configurationSettings;

        protected BaseCommand(
            IConfiguratorCollection configurators,
            ISettings configurationSettings,
            IServiceCollection serviceCollection,
            IFileSystem fileSystem)
        {
            Configurators = configurators;
            ServiceCollection = serviceCollection;
            FileSystem = fileSystem;
            _configurationSettings = configurationSettings;
        }

        public IConfiguratorCollection Configurators { get; }

        public IServiceCollection ServiceCollection { get; }

        public IFileSystem FileSystem { get; }

        public sealed override async Task<int> ExecuteAsync(
            CommandContext context,
            TSettings commandSettings,
            CancellationToken cancellationToken = default)
        {
            // Set verbose tracing
            if (commandSettings.LogLevel != LogLevel.Information)
            {
                ServiceCollection.Configure<LoggerFilterOptions>(options =>
                    options.MinLevel = commandSettings.LogLevel);
            }

            // File logging
            if (!string.IsNullOrEmpty(commandSettings.LogFile))
            {
                // Add the log provider (adding it to the service collection will get picked up by the logger factory)
                ServiceCollection.AddSingleton<ILoggerProvider, FileLoggerProvider>();
                ServiceCollection.Configure<FileLoggerOptions>(options =>
                {
                    options.FileName = commandSettings.LogFile;
                    options.LogDirectory = "logs";
                });
            }

            // Set failure log level, but only if set on the command line, otherwise set it to a default if not already
            if (commandSettings.FailureLogLevel.HasValue)
            {
                _configurationSettings[Keys.FailureLogLevel] = commandSettings.FailureLogLevel.ToString();
            }
            else
            {
                _configurationSettings.TryAdd(Keys.FailureLogLevel, nameof(LogLevel.Error));
            }

            // Set paths, command settings will override any file system configuration done on the bootstrapper
            if (!string.IsNullOrEmpty(commandSettings.RootPath))
            {
                NormalizedPath currentDirectory = Directory.GetCurrentDirectory();
                FileSystem.RootPath = currentDirectory.Combine(commandSettings.RootPath);
            }
            if (commandSettings.InputPaths?.Length > 0)
            {
                // Remove the defaults if new ones are set
                // and reverse the inputs so the last one is first to match the semantics of multiple occurrence single options
                FileSystem.InputPaths.Clear();
                FileSystem.InputPaths.AddRange(commandSettings.InputPaths.Select(x => new NormalizedPath(x)).Reverse());
            }
            if (!string.IsNullOrEmpty(commandSettings.OutputPath))
            {
                FileSystem.OutputPath = commandSettings.OutputPath;
            }
            if (!string.IsNullOrEmpty(commandSettings.TempPath))
            {
                FileSystem.TempPath = commandSettings.TempPath;
            }
            if (!string.IsNullOrEmpty(commandSettings.CachePath))
            {
                FileSystem.CachePath = commandSettings.CachePath;
            }

            // Set the command in the bootstrapper
            Bootstrapper bootstrapper = ServiceCollection.GetRequiredImplementationInstance<Bootstrapper>();
            bootstrapper.Command = this;

            // Build a temporary service provider just to get a logger
            IServiceProvider services = ServiceCollection.BuildServiceProvider();
            ILogger logger = services.GetRequiredService<ILogger<Bootstrapper>>();

            // Log pending ClassCatalog messages
            ClassCatalog classCatalog = ServiceCollection.GetRequiredImplementationInstance<ClassCatalog>();
            classCatalog?.LogDebugMessagesTo(logger);

            // Debug/Attach
            if (commandSettings.Debug)
            {
                logger.LogInformation($"Waiting to launch a debugger for process {Process.GetCurrentProcess().Id}...");
                Debugger.Launch();
            }
            if ((commandSettings.Attach || commandSettings.Debug) && !Debugger.IsAttached)
            {
                if (commandSettings.Debug)
                {
                    // If we got here the debug command was unsuccessful
                    logger.LogInformation($"Could not launch a debugger, waiting for manual attach");
                }

                logger.LogInformation(
                    $"Waiting for a debugger to attach to process {Process.GetCurrentProcess().Id} (or press a key to continue)...");
                while (!Debugger.IsAttached && !Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                }
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    logger.LogInformation("Key pressed, continuing execution");
                }
                else
                {
                    logger.LogInformation("Debugger attached, continuing execution");
                }
            }

            // Add settings from the command line, note that some settings like InputPaths may already have been
            // actioned and therefore cannot be set from the command line
            if (commandSettings.Settings?.Length > 0)
            {
                foreach (KeyValuePair<string, string> setting in SettingsParser.Parse(commandSettings.Settings))
                {
                    _configurationSettings[setting.Key] = setting.Value;
                }
            }

            return await ExecuteCommandAsync(context, commandSettings);
        }

        public abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
    }
}