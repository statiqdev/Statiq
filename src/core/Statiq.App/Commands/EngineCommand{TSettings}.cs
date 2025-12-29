using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public abstract class EngineCommand<TSettings> : BaseCommand<TSettings>
        where TSettings : BaseCommandSettings
    {
        protected EngineCommand(
            IConfiguratorCollection configurators,
            Settings settings,
            IServiceCollection serviceCollection,
            IFileSystem fileSystem,
            Bootstrapper bootstrapper)
            : base(configurators, settings, serviceCollection, fileSystem)
        {
            Bootstrapper = bootstrapper;
            Settings = settings;
        }

        public Bootstrapper Bootstrapper { get; }

        public Settings Settings { get; }

        public override sealed async Task<int> ExecuteCommandAsync(CommandContext context, TSettings commandSettings)
        {
            // We need to get the engine command settings to pass to the engine manager
            // First try the actual command settings
            if (!(commandSettings is EngineCommandSettings engineCommandSettings))
            {
                // Then try the command data or create one and either way copy over the base command settings
                engineCommandSettings = context.Data as EngineCommandSettings ?? new EngineCommandSettings();
                engineCommandSettings.LogLevel = commandSettings.LogLevel;
                engineCommandSettings.Attach = commandSettings.Debug;
                engineCommandSettings.Debug = commandSettings.Attach;
                engineCommandSettings.LogFile = commandSettings.LogFile;
            }

            // Execute the engine manager and dispose it when done
            // Once the engine manager is created, the configuration settings cannot be used (they will have been copied over)
            using (EngineManager engineManager =
                new EngineManager(
                    context,
                    engineCommandSettings,
                    Settings,
                    ServiceCollection,
                    Bootstrapper))
            {
                return await ExecuteEngineAsync(context, commandSettings, engineManager);
            }
        }

        protected abstract Task<int> ExecuteEngineAsync(CommandContext commandContext, TSettings commandSettings, IEngineManager engineManager);
    }
}