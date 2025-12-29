using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public class Bootstrapper : IBootstrapper
    {
        private Func<CommandServiceTypeRegistrar, ICommandApp> _getCommandApp = x => new CommandApp(x);

        // Private constructor to force factory use which returns the interface to get access to default interface implementations
        internal Bootstrapper(string[] arguments)
        {
            Arguments = arguments.ThrowIfNull(nameof(arguments));
        }

        /// <inheritdoc/>
        public ClassCatalog ClassCatalog { get; } = new ClassCatalog();

        /// <inheritdoc/>
        public IConfiguratorCollection Configurators { get; } = new ConfiguratorCollection();

        /// <inheritdoc/>
        public string[] Arguments { get; }

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; } = new FileSystem();

        /// <inheritdoc/>
        public Bootstrapper SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandApp = x => new CommandApp<TCommand>(x);
            return this;
        }

        /// <inheritdoc/>
        public async Task<int> RunAsync()
        {
            // Remove the synchronization context
            await default(SynchronizationContextRemover);

            // Populate the class catalog (if we haven't already)
            ClassCatalog.Populate();

            // Run bootstrapper configurators first
            Configurators.Configure<IBootstrapper>(this);
            Configurators.Configure(this);

            // Run the configuration configurator and get the configuration root
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            ConfigurableConfiguration configurableConfiguration = new ConfigurableConfiguration(configurationBuilder);
            Configurators.Configure(configurableConfiguration);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            Settings settings = new Settings(configurationRoot);

            // Create the service collection
            IServiceCollection serviceCollection = CreateServiceCollection() ?? new ServiceCollection();
            serviceCollection.TryAddSingleton(this);
            serviceCollection.TryAddSingleton<IBootstrapper>(this);
            serviceCollection.TryAddSingleton(ClassCatalog);  // The class catalog is retrieved later for deferred logging once a service provider is built
            serviceCollection.TryAddSingleton<IConfiguration>(settings);
            serviceCollection.TryAddSingleton<ISettings>(settings);

            // Run configurators on the service collection
            ConfigurableServices configurableServices = new ConfigurableServices(serviceCollection, settings, FileSystem);
            Configurators.Configure(configurableServices);

            // Add simple logging to make sure it's available in commands before the engine adds in,
            // but add it after the configurators have a chance to configure logging
            serviceCollection.AddLogging();

            // Configure additional settings
            ConfigurableSettings configurableSettings = new ConfigurableSettings(settings, serviceCollection, FileSystem);
            Configurators.Configure(configurableSettings);

            // Configure the file system after settings so configurators can use them if needed
            ConfigurableFileSystem configurableFileSystem = new ConfigurableFileSystem(FileSystem, settings, serviceCollection);
            Configurators.Configure(configurableFileSystem);

            // Create the stand-alone command line service container and register a few types needed for the CLI
            CommandServiceTypeRegistrar registrar = new CommandServiceTypeRegistrar();
            registrar.RegisterInstance(typeof(Settings), settings);
            registrar.RegisterInstance(typeof(IConfigurationRoot), settings);
            registrar.RegisterInstance(typeof(IServiceCollection), serviceCollection);
            registrar.RegisterInstance(typeof(IConfiguratorCollection), Configurators);
            registrar.RegisterInstance(typeof(IFileSystem), FileSystem);
            registrar.RegisterInstance(typeof(Bootstrapper), this);

            // Create the command line parser and run the command
            ICommandApp app = _getCommandApp(registrar);
            app.Configure(commandConfigurator =>
            {
                commandConfigurator.ValidateExamples();
                ConfigurableCommands configurableCommands = new ConfigurableCommands(commandConfigurator);
                Configurators.Configure(configurableCommands);
            });
            int exitCode = await app.RunAsync(Arguments);

            // Dispose all instances of the console logger to flush the message queue and stop the listening thread
            ConsoleLoggerProvider.DisposeAll();

            return exitCode;
        }

        /// <summary>
        /// Creates a service collection for use by the bootstrapper.
        /// </summary>
        /// <remarks>
        /// Override to perform post-creation configuration or to use an alternate service collection type.
        /// </remarks>
        /// <returns>A service collection for use by the bootstrapper.</returns>
        protected virtual IServiceCollection CreateServiceCollection() => null;

        /// <inheritdoc/>
        public IBaseCommand Command { get; internal set; }

        // Factory

        public static readonly BootstrapperFactory Factory = new BootstrapperFactory();
    }
}