using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableSettings : IConfigurable
    {
        public ConfigurableSettings(
            ISettings settings,
            IServiceCollection serviceCollection,
            IReadOnlyFileSystem fileSystem)
        {
            Settings = settings;
            ServiceCollection = serviceCollection;
            FileSystem = fileSystem;
        }

        public ISettings Settings { get; }

        public IServiceCollection ServiceCollection { get; }

        public IReadOnlyFileSystem FileSystem { get; }
    }
}