using System;
using System.Threading;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Unsafe;
using Statiq.Common;

namespace Statiq.App
{
    public static class IConfiguratorExtensions
    {
        public static ICommandConfigurator AddCommand<TCommand>(this IConfigurator configurator)
            where TCommand : class, ICommand =>
            configurator.AddCommand<TCommand>(GetCommandName(typeof(TCommand)));

        public static ICommandConfigurator AddCommand(this IConfigurator configurator, Type commandType) =>
            configurator.SafetyOff().AddCommand(
                GetCommandName(commandType),
                commandType.ThrowIfNull(nameof(commandType)));

        private static string GetCommandName(Type commandType)
        {
            string name = commandType.Name;
            int genericParametersIndex = name.IndexOf('`');
            if (genericParametersIndex > 0)
            {
                name = name.Substring(0, genericParametersIndex);
            }
            return name.RemoveEnd("Command", StringComparison.OrdinalIgnoreCase).ToKebab();
        }

        public static ICommandConfigurator AddPipelineCommand(
            this IConfigurator configurator,
            string name,
            params string[] pipelines) =>
            configurator.AddPipelineCommand(name, false, pipelines);

        public static ICommandConfigurator AddPipelineCommand(
            this IConfigurator configurator,
            string name,
            bool normalPipelines,
            params string[] pipelines) =>
                configurator.AddPipelineCommand(name, new PipelinesCommandSettings
                {
                    Pipelines = pipelines,
                    NormalPipelines = normalPipelines
                });

        public static ICommandConfigurator AddPipelineCommand(
            this IConfigurator configurator,
            string name,
            EngineCommandSettings commandSettings)
        {
            name.ThrowIfNull(nameof(name));
            commandSettings.ThrowIfNull(nameof(commandSettings));
            return configurator
                .AddCommand<PipelinesCommand<BaseCommandSettings>>(name)
                .WithData(commandSettings);
        }

        public static ICommandConfigurator AddDelegateCommand(
            this IConfigurator configurator,
            string name,
            Func<CommandContext, int> func,
            CancellationToken ct = default) =>
            configurator.AddDelegate<EmptyCommandSettings>(name, (c, _, ct) => func(c));
    }
}
