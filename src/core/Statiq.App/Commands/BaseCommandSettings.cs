using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Statiq.App
{
    public class BaseCommandSettings : CommandSettings
    {
        [CommandOption("-i|--input <PATH>")]
        [Description("The path(s) of input files, can be absolute or relative to the current folder.")]
        public string[] InputPaths { get; set; }

        [CommandOption("-o|--output <PATH>")]
        [Description("The path to output files, can be absolute or relative to the current folder.")]
        public string OutputPath { get; set; }

        [CommandOption("--temp <PATH>")]
        [Description("The path to output temporary files, can be absolute or relative to the current folder.")]
        public string TempPath { get; set; }

        [CommandOption("--cache <PATH>")]
        [Description("The path to output cache files, can be absolute or relative to the current folder.")]
        public string CachePath { get; set; }

        [CommandOption("-r|--root")]
        [Description("The root folder to use.")]
        public string RootPath { get; set; }

        [CommandOption("-l|--log-level <LEVEL>")]
        [Description("Sets the minimum log level (\"Critical\", \"Error\", \"Warning\", \"Information\", \"Debug\", \"Trace\", \"None\").")]
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        [CommandOption("--attach")]
        [Description("Pause execution at the start of the program until a debugger is attached.")]
        public bool Attach { get; set; }

        [CommandOption("--debug")]
        [Description("Allows you to select a debugger to attach.")]
        public bool Debug { get; set; }

        [CommandOption("-f|--log-file <LOGFILE>")]
        [Description("Log all messages to the specified log file.")]
        public string LogFile { get; set; }

        [CommandOption("-s|--setting <SETTING>")]
        [Description("Specifies a setting as a \"[[key]]=[[value]]\" pair (the value can be omited).")]
        public string[] Settings { get; set; }

        [CommandOption("--failure-log-level <LEVEL>")]
        [Description("Indicates that the generation should fail for all log messages above the specified level threshold.")]
        public LogLevel? FailureLogLevel { get; set; }
    }
}