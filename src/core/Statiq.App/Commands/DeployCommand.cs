using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    [Description("Executes deployment pipelines.")]
    internal class DeployCommand : PipelinesCommand<DeployCommandSettings>
    {
        public DeployCommand(
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

        protected override void SetPipelines(CommandContext commandContext, DeployCommandSettings commandSettings, IEngineManager engineManager)
        {
            engineManager.Pipelines = engineManager.Engine.Pipelines.AsEnumerable().Where(x => x.Value.Deployment).Select(x => x.Key).ToArray();
            engineManager.NormalPipelines = !commandSettings.OnlyDeploy;
        }
    }
}