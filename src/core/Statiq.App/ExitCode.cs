namespace Statiq.App
{
    public enum ExitCode
    {
        Normal = 0,
        UnhandledError = 1,
        CommandLineError = -1, // Set by Spectre.Cli
        ExecutionError = 4,
        LogLevelFailure = 5,
        OperationCanceled = 6
    }
}
