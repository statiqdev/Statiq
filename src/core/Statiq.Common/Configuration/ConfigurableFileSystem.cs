using System;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableFileSystem : IConfigurable
    {
        public ConfigurableFileSystem(
            IFileSystem fileSystem,
            IReadOnlySettings settings,
            IServiceCollection serviceCollection)
        {
            FileSystem = fileSystem;
            Settings = settings;
            ServiceCollection = serviceCollection;
        }

        public IFileSystem FileSystem { get; }

        public IReadOnlySettings Settings { get; }

        public IServiceCollection ServiceCollection { get; }
    }
}