using System.ComponentModel;
using Spectre.Console.Cli;

namespace Statiq.App
{
    public class GlobTestCommandSettings : GlobCommandSettings
    {
        [CommandArgument(0, "<pattern>")]
        [Description("The globbing pattern to test.")]
        public string Pattern { get; set; }

        [CommandArgument(1, "<path>")]
        [Description("The file path to test the globbing pattern against (does not need to exist).")]
        public string Path { get; set; }
    }
}
