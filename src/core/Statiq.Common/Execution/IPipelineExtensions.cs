using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IPipelineExtensions
    {
        public static TPipeline WithInputModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithInputModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithProcessModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithPostProcessModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.PostProcessModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithPostProcessModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.PostProcessModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithOutputModules<TPipeline>(this TPipeline pipeline, IEnumerable<IModule> modules)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.AddRange(modules);
            return pipeline;
        }

        public static TPipeline WithOutputModules<TPipeline>(this TPipeline pipeline, params IModule[] modules)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(modules);
            return pipeline;
        }

        public static TPipeline WithDependencies<TPipeline>(this TPipeline pipeline, params string[] dependencies)
            where TPipeline : IPipeline
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        public static TPipeline WithDependencies<TPipeline>(this TPipeline pipeline, IEnumerable<string> dependencies)
            where TPipeline : IPipeline
        {
            pipeline.Dependencies.AddRange(dependencies);
            return pipeline;
        }

        public static TPipeline AsDependencyOf<TPipeline>(this TPipeline pipeline, params string[] dependencyOf)
            where TPipeline : IPipeline
        {
            pipeline.DependencyOf.AddRange(dependencyOf);
            return pipeline;
        }

        public static TPipeline AsDependencyOf<TPipeline>(this TPipeline pipeline, IEnumerable<string> dependencyOf)
            where TPipeline : IPipeline
        {
            pipeline.DependencyOf.AddRange(dependencyOf);
            return pipeline;
        }

        public static TPipeline AsIsolated<TPipeline>(this TPipeline pipeline, bool isolated = true)
            where TPipeline : IPipeline
        {
            pipeline.Isolated = isolated;
            return pipeline;
        }

        public static TPipeline AsDeployment<TPipeline>(this TPipeline pipeline, bool deployment = true)
            where TPipeline : IPipeline
        {
            pipeline.Deployment = deployment;
            return pipeline;
        }

        public static TPipeline AsPostProcessHasDependencies<TPipeline>(this TPipeline pipeline, bool postProcessHasDependencies = true)
            where TPipeline : IPipeline
        {
            pipeline.PostProcessHasDependencies = postProcessHasDependencies;
            return pipeline;
        }

        public static TPipeline WithExecutionPolicy<TPipeline>(this TPipeline pipeline, ExecutionPolicy policy)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = policy;
            return pipeline;
        }

        public static TPipeline NormallyExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Normal;
            return pipeline;
        }

        public static TPipeline ManuallyExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Manual;
            return pipeline;
        }

        public static TPipeline AlwaysExecute<TPipeline>(this TPipeline pipeline)
            where TPipeline : IPipeline
        {
            pipeline.ExecutionPolicy = ExecutionPolicy.Always;
            return pipeline;
        }

        /// <summary>
        /// Gets all dependencies of this pipeline including <see cref="IReadOnlyPipeline.DependencyOf"/> declarations.
        /// </summary>
        /// <remarks>This does not resolve nested dependencies, only the combination of
        /// <see cref="IReadOnlyPipeline.Dependencies"/> and <see cref="IReadOnlyPipeline.DependencyOf"/> declarations.</remarks>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="pipelines">The current pipelines.</param>
        /// <returns>All dependencies of the pipeline.</returns>
        public static IEnumerable<string> GetAllDependencies(this IReadOnlyPipeline pipeline, IReadOnlyPipelineCollection pipelines)
        {
            pipeline.ThrowIfNull(nameof(pipeline));
            pipelines.ThrowIfNull(nameof(pipelines));

            string pipelineName = pipelines.FirstOrDefault(x => x.Value.Equals(pipeline)).Key
                ?? throw new InvalidOperationException($"Could not find pipeline {pipeline.GetType().Name} in pipeline collection");
            return (pipeline.Dependencies ?? (IEnumerable<string>)Array.Empty<string>())
                .Concat(pipelines.Where(x => x.Value.DependencyOf?.Contains(pipelineName, StringComparer.OrdinalIgnoreCase) == true).Select(x => x.Key))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets all dependencies of this pipeline including <see cref="IReadOnlyPipeline.DependencyOf"/> declarations.
        /// </summary>
        /// <remarks>This does not resolve nested dependencies, only the combination of
        /// <see cref="IReadOnlyPipeline.Dependencies"/> and <see cref="IReadOnlyPipeline.DependencyOf"/> declarations.</remarks>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="executionState">The current execution state (usually an <see cref="IEngine"/> or <see cref="IExecutionContext"/>).</param>
        /// <returns>All dependencies of the pipeline.</returns>
        public static IEnumerable<string> GetAllDependencies(this IReadOnlyPipeline pipeline, IExecutionState executionState) =>
            pipeline.GetAllDependencies(executionState.ThrowIfNull(nameof(executionState)).Pipelines);
    }
}