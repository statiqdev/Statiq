using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(
            IServiceCollection services,
            IReadOnlySettings settings,
            IReadOnlyFileSystem fileSystem)
        {
            Services = services;
            Settings = settings;
            FileSystem = fileSystem;
        }

        public IServiceCollection Services { get; }

        public IReadOnlySettings Settings { get; }

        public IReadOnlyFileSystem FileSystem { get; }
    }
}