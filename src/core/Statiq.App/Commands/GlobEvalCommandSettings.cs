using System.ComponentModel;
using Spectre.Console.Cli;

namespace Statiq.App
{
    public class GlobEvalCommandSettings : GlobCommandSettings
    {
        [CommandArgument(0, "<pattern>")]
        [Description("The globbing pattern to evaluate.")]
        public string Pattern { get; set; }

        [CommandArgument(1, "<path>")]
        [Description("The absolute directory path to evaluate the globbing pattern against.")]
        public string Path { get; set; }
    }
}
