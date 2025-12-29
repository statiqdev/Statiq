using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Unsafe;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperCommandExtensions
    {
        // Extensions that can use TBootstrapper (I.e. they don't use other generic parameters) need to
        // stay in Statiq.App since commands depend on Spectre.Cli, but they should still use a generic
        // TBootstrapper in case other libraries want to use IInitalizer and depend on Statiq.App

        public static TBootstrapper ConfigureCommands<TBootstrapper>(this TBootstrapper bootstrapper, Action<IConfigurator> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<ConfigurableCommands>(x => action(x.Configurator));
            return bootstrapper;
        }

        public static Bootstrapper AddCommand<TCommand>(this Bootstrapper bootstrapper)
            where TCommand : class, ICommand =>
            bootstrapper.ConfigureCommands(x => x.AddCommand<TCommand>());

        public static Bootstrapper AddCommand<TCommand>(this Bootstrapper bootstrapper, string name)
            where TCommand : class, ICommand =>
            bootstrapper.ConfigureCommands(x => x.AddCommand<TCommand>(name));

        public static TBootstrapper AddCommand<TBootstrapper>(this TBootstrapper bootstrapper, Type commandType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddCommand(commandType));

        public static TBootstrapper AddCommand<TBootstrapper>(this TBootstrapper bootstrapper, Type commandType, string name)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.SafetyOff().AddCommand(name, commandType));

        public static TBootstrapper AddCommand<TBootstrapper>(this TBootstrapper bootstrapper, Type commandType, string name, string description)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.SafetyOff().AddCommand(name, commandType).WithDescription(description));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params string[] pipelines)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, pipelines));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string description,
            params string[] pipelines)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, pipelines).WithDescription(description));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            bool defaultPipelines,
            params string[] pipelines)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string description,
            bool defaultPipelines,
            params string[] pipelines)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines).WithDescription(description));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            EngineCommandSettings commandSettings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings));

        public static TBootstrapper AddPipelineCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string description,
            EngineCommandSettings commandSettings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings).WithDescription(description));

        public static TBootstrapper AddDelegateCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            Func<CommandContext, int> func)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddDelegateCommand(name, func));

        public static TBootstrapper AddDelegateCommand<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string description,
            Func<CommandContext, int> func)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureCommands(x => x.AddDelegateCommand(name, func).WithDescription(description));

        public static Bootstrapper AddDelegateCommand<TSettings>(
            this Bootstrapper bootstrapper,
            string name,
            Func<CommandContext, TSettings, CancellationToken, int> func)
            where TSettings : CommandSettings =>
            bootstrapper.ConfigureCommands(x => x.AddDelegate(name, func));

        public static Bootstrapper AddDelegateCommand<TSettings>(
            this Bootstrapper bootstrapper,
            string name,
            string description,
            Func<CommandContext, TSettings, CancellationToken, int> func)
            where TSettings : CommandSettings =>
            bootstrapper.ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the specified assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="assembly">The assembly to add commands from.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddCommands<TBootstrapper>(this TBootstrapper bootstrapper, Assembly assembly)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            assembly.ThrowIfNull(nameof(assembly));
            foreach (Type commandType in bootstrapper.ClassCatalog.GetTypesAssignableTo<ICommand>().Where(x => x.Assembly.Equals(assembly)))
            {
                bootstrapper.AddCommand(commandType);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the entry assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddCommands<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddCommands(Assembly.GetEntryAssembly());

        public static Bootstrapper AddCommands<TParent>(this Bootstrapper bootstrapper) => bootstrapper.AddCommands(typeof(TParent));

        public static TBootstrapper AddCommands<TBootstrapper>(this TBootstrapper bootstrapper, Type parentType)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            parentType.ThrowIfNull(nameof(parentType));
            foreach (Type commandType in parentType.GetNestedTypes().Where(x => typeof(ICommand).IsAssignableFrom(x)))
            {
                bootstrapper.AddCommand(commandType);
            }
            return bootstrapper;
        }
    }
}