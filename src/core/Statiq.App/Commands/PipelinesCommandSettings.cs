using System.ComponentModel;
using Spectre.Console.Cli;

namespace Statiq.App
{
    public class PipelinesCommandSettings : EngineCommandSettings
    {
        [CommandOption("-n|--normal")]
        [Description("Executes normal pipelines as well as those specified.")]
        public bool NormalPipelines { get; set; }

        [CommandArgument(0, "[pipelines]")]
        [Description("The pipeline(s) to execute.")]
        public string[] Pipelines { get; set; }
    }
}
