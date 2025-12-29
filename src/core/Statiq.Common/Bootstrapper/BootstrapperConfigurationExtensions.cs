using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class BootstrapperConfigurationExtensions
    {
        public static TBootstrapper BuildConfiguration<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IConfigurationBuilder> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableConfiguration>(x => action(x.Builder));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureServices<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IServiceCollection> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableServices>(x => action(x.Services));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureServices<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IServiceCollection, IReadOnlySettings> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableServices>(x => action(x.Services, x.Settings));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureServices<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IServiceCollection, IReadOnlySettings, IReadOnlyFileSystem> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableServices>(x => action(x.Services, x.Settings, x.FileSystem));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureEngine<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IEngine> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add(action);
            return bootstrapper;
        }
    }
}