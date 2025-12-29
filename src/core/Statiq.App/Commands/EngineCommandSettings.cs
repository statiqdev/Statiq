using System.ComponentModel;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public class EngineCommandSettings : BaseCommandSettings
    {
        [CommandOption("--noclean")]
        [Description("Prevents cleaning of the output path on each execution (same as \"--clean-mode None\").")]
        public bool NoClean { get; set; }

        [CommandOption("--clean-mode")]
        [Description("Specifies how the output path will be cleaned between each execution.")]
        public CleanMode? CleanMode { get; set; }

        [CommandOption("--nocache")]
        [Description("Prevents caching information during execution (less memory usage but slower execution).")]
        public bool NoCache { get; set; }

        [CommandOption("--stdin")]
        [Description("Reads standard input at startup and sets ApplicationInput in the execution context.")]
        public bool StdIn { get; set; }

        [CommandOption("--serial")]
        [Description("Executes pipeline phases and modules in serial.")]
        public bool SerialExecution { get; set; }

        [CommandOption("-a|--analyzer <ANALYZER>")]
        [Description("Specifies the log level for the specified analyzer as \"[[analyzer]]=[[log level]]\" (log level is optional, \"All\" to set all analyzers).")]
        public string[] Analyzers { get; set; }
    }
}