using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class BootstrapperPipelineExtensions
    {
        // Directly

        public static TBootstrapper AddPipelines<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Action<IPipelineCollection> action)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => action(x.Pipelines));

        public static TBootstrapper AddPipelines<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Action<IReadOnlySettings, IPipelineCollection> action)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => action(x.Settings, x.Pipelines));

        // By type

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IPipeline pipeline)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(name, pipeline));

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, IPipeline pipeline)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(pipeline));

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<IReadOnlySettings, IPipeline> pipelineFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(name, pipelineFunc(x.Settings)));

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, Func<IReadOnlySettings, IPipeline> pipelineFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Pipelines.Add(pipelineFunc(x.Settings)));

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, Type pipelineType)
            where TBootstrapper : IBootstrapper
        {
            pipelineType.ThrowIfNull(nameof(pipelineType));
            if (!typeof(IPipeline).IsAssignableFrom(pipelineType))
            {
                throw new ArgumentException("Provided type is not a pipeline");
            }
            return bootstrapper.ConfigureServices(x => x.AddSingleton(typeof(IPipeline), pipelineType));
        }

        public static TBootstrapper AddPipelines<TBootstrapper>(this TBootstrapper bootstrapper, Type parentType)
            where TBootstrapper : IBootstrapper
        {
            parentType.ThrowIfNull(nameof(parentType));
            return bootstrapper.ConfigureServices(x =>
            {
                foreach (Type pipelineType in parentType.GetNestedTypes().Where(t => typeof(IPipeline).IsAssignableFrom(t)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });
        }

        public static TBootstrapper AddPipelines<TBootstrapper>(this TBootstrapper bootstrapper, Assembly assembly)
            where TBootstrapper : IBootstrapper
        {
            assembly.ThrowIfNull(nameof(assembly));
            return bootstrapper.ConfigureServices(x =>
            {
                foreach (Type pipelineType in bootstrapper.ClassCatalog.GetTypesAssignableTo<IPipeline>().Where(x => x.Assembly.Equals(assembly)))
                {
                    x.AddSingleton(typeof(IPipeline), pipelineType);
                }
            });
        }

        public static TBootstrapper AddPipelines<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipelines(Assembly.GetEntryAssembly());
    }
}
