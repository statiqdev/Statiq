using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperPipelineExtensions
    {
        /// <summary>
        /// Registers a pipeline of type <typeparamref name="TPipeline"/>.
        /// </summary>
        /// <remarks>
        /// Note that this method registers the pipeline with the dependency injection container
        /// which the engine uses to populate the initial set of pipelines on instantiation. Therefore
        /// any pipelines added through this method will take effect before anything that happens in
        /// <c>ConfigureEngine()</c>, even if the call to this method is after the call to that one.
        /// To add pipelines after <c>ConfigureEngine</c> is called you will need to manipulate the
        /// <see cref="IEngine.Pipelines"/> collection directly.
        /// </remarks>
        public static Bootstrapper AddPipeline<TPipeline>(this Bootstrapper bootstrapper)
            where TPipeline : IPipeline =>
            bootstrapper.ConfigureServices(x => x.AddSingleton(typeof(IPipeline), typeof(TPipeline)));

        /// <summary>
        /// Registers a pipeline of type <typeparamref name="TPipeline"/> with the specified name.
        /// </summary>
        /// <remarks>
        /// Note that this method registers the pipeline with the dependency injection container
        /// which the engine uses to populate the initial set of pipelines on instantiation. Therefore
        /// any pipelines added through this method will take effect before anything that happens in
        /// <c>ConfigureEngine()</c>, even if the call to this method is after the call to that one.
        /// To add pipelines after <c>ConfigureEngine</c> is called you will need to manipulate the
        /// <see cref="IEngine.Pipelines"/> collection directly.
        /// </remarks>
        public static Bootstrapper AddPipeline<TPipeline>(this Bootstrapper bootstrapper, string name)
            where TPipeline : class, IPipeline
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            return bootstrapper.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddTransient<TPipeline>();
                serviceCollection.AddSingleton(
                    typeof(IPipeline),
                    s => new NamedPipelineWrapper(name, s.GetRequiredService<TPipeline>()));
            });
        }

        /// <summary>
        /// Registers all pipelines defined in <typeparamref name="TParent"/>.
        /// </summary>
        /// <remarks>
        /// Note that this method registers the pipeline with the dependency injection container
        /// which the engine uses to populate the initial set of pipelines on instantiation. Therefore
        /// any pipelines added through this method will take effect before anything that happens in
        /// <c>ConfigureEngine()</c>, even if the call to this method is after the call to that one.
        /// To add pipelines after <c>ConfigureEngine</c> is called you will need to manipulate the
        /// <see cref="IEngine.Pipelines"/> collection directly.
        /// </remarks>
        public static Bootstrapper AddPipelines<TParent>(this Bootstrapper bootstrapper) =>
            bootstrapper.AddPipelines(typeof(TParent));

        /// <summary>
        /// Modifies an existing pipeline with name <paramref name="name"/>.
        /// </summary>
        public static Bootstrapper ModifyPipeline(this Bootstrapper bootstrapper, string name, Action<IPipeline> action) =>
            bootstrapper.ConfigureEngine(x => action.ThrowIfNull(nameof(action))(x.Pipelines[name.ThrowIfNullOrEmpty(nameof(name))]));

        // Builder

        public static TBootstrapper BuildPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, Action<PipelineBuilder> buildAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x =>
            {
                PipelineBuilder builder = new PipelineBuilder(x.Pipelines, x.Settings, x.Services);
                buildAction(builder);
                IPipeline pipeline = builder.Build();
                if (pipeline is object)
                {
                    x.Pipelines.Add(name, pipeline);
                }
            });

        // Normal

        public static TBootstrapper AddPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Normal, name, readFilesPattern, destinationPath, processModules, null);

        // Deployment

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(this TBootstrapper bootstrapper, string name, IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddDeploymentPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<string> dependencies,
            params IModule[] outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(dependencies, PipelineType.Deployment, name, (IEnumerable<IModule>)null, null, null, outputModules);

        // Serial

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddSerialPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Serial, name, readFilesPattern, destinationPath, processModules, null);

        // Isolated

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            IEnumerable<IModule> inputModules = null,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, inputModules, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null,
            IEnumerable<IModule> outputModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, processModules, transformModules, outputModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, writeFiles, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationExtension, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules = null,
            IEnumerable<IModule> transformModules = null)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationPath, processModules, transformModules);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, (IEnumerable<IModule>)null, processModules, null, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            bool writeFiles,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, writeFiles, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            string destinationExtension,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationExtension, processModules, null);

        public static TBootstrapper AddIsolatedPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            params IModule[] processModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddPipeline(null, PipelineType.Isolated, name, readFilesPattern, destinationPath, processModules, null);

        // Helpers for adding pipelines from modules

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            IEnumerable<IModule> inputModules,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithInputModules(inputModules)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputModules(outputModules);
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules,
            IEnumerable<IModule> outputModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputModules(outputModules);
                if (readFilesPattern is object)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            bool writeFiles,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules);
                if (readFilesPattern is object)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
                if (writeFiles)
                {
                    builder.WithOutputWriteFiles();
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            string destinationExtension,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputWriteFiles(destinationExtension);
                if (readFilesPattern is object)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static TBootstrapper AddPipeline<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IEnumerable<string> dependencies,
            PipelineType pipelineType,
            string name,
            string readFilesPattern,
            Config<NormalizedPath> destinationPath,
            IEnumerable<IModule> processModules,
            IEnumerable<IModule> transformModules)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.BuildPipeline(name, builder =>
            {
                ConfigureBuilder(builder, dependencies, pipelineType)
                    .WithProcessModules(processModules)
                    .WithPostProcessModules(transformModules)
                    .WithOutputWriteFiles(destinationPath);
                if (readFilesPattern is object)
                {
                    builder.WithInputReadFiles(readFilesPattern);
                }
            });

        private static PipelineBuilder ConfigureBuilder(PipelineBuilder builder, IEnumerable<string> dependencies, PipelineType pipelineType)
        {
            if (dependencies is object)
            {
                builder.WithDependencies(dependencies);
            }
            if (pipelineType == PipelineType.Isolated)
            {
                builder.AsIsolated();
            }
            else if (pipelineType == PipelineType.Serial)
            {
                builder.AsSerial();
            }
            else if (pipelineType == PipelineType.Deployment)
            {
                builder.AsDeployment();
            }
            return builder;
        }

        private enum PipelineType
        {
            Normal,
            Isolated,
            Serial,
            Deployment
        }
    }
}