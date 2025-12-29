using System;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class BootstrapperFileSystemExtensions
    {
        public static TBootstrapper ConfigureFileSystem<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IFileSystem> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableFileSystem>(x => action(x.FileSystem));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureFileSystem<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IFileSystem, IReadOnlySettings> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableFileSystem>(x => action(x.FileSystem, x.Settings));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureFileSystem<TBootstrapper>(
            this TBootstrapper bootstrapper, Action<IFileSystem, IReadOnlySettings, IServiceCollection> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableFileSystem>(x => action(x.FileSystem, x.Settings, x.ServiceCollection));
            return bootstrapper;
        }

        public static TBootstrapper SetRootPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath rootPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.RootPath = rootPath);

        public static TBootstrapper AddInputPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath inputPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.InputPaths.Add(inputPath));

        public static TBootstrapper AddMappedInputPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath inputPath, NormalizedPath virtualPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x =>
            {
                x.InputPaths.Add(inputPath);
                x.InputPathMappings.Add(inputPath, virtualPath);
            });

        public static TBootstrapper AddExcludedPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath excludedPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.ExcludedPaths.Add(excludedPath));

        public static TBootstrapper SetOutputPath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath outputPath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureFileSystem(x => x.OutputPath = outputPath);
    }
}