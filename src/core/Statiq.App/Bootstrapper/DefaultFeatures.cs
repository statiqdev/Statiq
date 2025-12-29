using System;

namespace Statiq.App
{
    [Flags]
    public enum DefaultFeatures
    {
        None = 0,
        BootstrapperConfigurators = 1 << 0,
        Logging = 1 << 1,
        Settings = 1 << 2,
        EnvironmentVariables = 1 << 3,
        ConfigurationFiles = 1 << 4,
        BuildCommands = 1 << 5,
        CustomCommands = 1 << 6,
        Shortcodes = 1 << 7,
        Namespaces = 1 << 8,
        Pipelines = 1 << 9,
        GlobCommands = 1 << 10,
        Analyzers = 1 << 11,
        All =
            BootstrapperConfigurators
            | Logging
            | Settings
            | EnvironmentVariables
            | ConfigurationFiles
            | BuildCommands
            | CustomCommands
            | Shortcodes
            | Namespaces
            | Pipelines
            | GlobCommands
            | Analyzers
    }
}
