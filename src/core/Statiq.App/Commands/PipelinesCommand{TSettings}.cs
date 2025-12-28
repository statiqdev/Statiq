using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    [Description("Executes the specified pipelines.")]
    public class PipelinesCommand<TSettings> : EngineCommand<TSettings>
        where TSettings : BaseCommandSettings
    {
        public PipelinesCommand(
            IConfiguratorCollection configurators,
            Settings settings,
            IServiceCollection serviceCollection,
            IFileSystem fileSystem,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  settings,
                  serviceCollection,
                  fileSystem,
                  bootstrapper)
        {
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager)
        {
            SetPipelines(commandContext, commandSettings, engineManager);
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                new ConsoleListener(async () =>
                {
                    await cancellationTokenSource.CancelAsync();
                });
                return (int)await engineManager.ExecuteAsync(cancellationTokenSource);
            }
        }

        protected virtual void SetPipelines(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager)
        {
            PipelinesCommandSettings buildSettings = commandSettings as PipelinesCommandSettings ?? commandContext.Data as PipelinesCommandSettings;
            if (buildSettings is object)
            {
                engineManager.Pipelines = buildSettings.Pipelines;
                engineManager.NormalPipelines = buildSettings.Pipelines is null || buildSettings.Pipelines.Length == 0 || buildSettings.NormalPipelines;
            }
        }
    }
}