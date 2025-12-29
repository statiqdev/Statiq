using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public class PipelineBuilder
    {
        private readonly List<Action<IPipeline>> _actions = new List<Action<IPipeline>>();

        private readonly IPipelineCollection _collection;

        internal PipelineBuilder(IPipelineCollection collection, IReadOnlySettings settings, IServiceProvider services)
        {
            _collection = collection;
            Settings = settings;
            Services = services;
        }

        public IReadOnlySettings Settings { get; }

        public IServiceProvider Services { get; }

        internal IPipeline Build()
        {
            IPipeline pipeline = new Pipeline();
            foreach (Action<IPipeline> action in _actions)
            {
                action(pipeline);
            }
            return pipeline;
        }

        public PipelineBuilder WithInputReadFiles(params string[] patterns)
        {
            _actions.Add(x => x.InputModules.Add(new ReadFiles((Config<IEnumerable<string>>)patterns)));
            return this;
        }

        public PipelineBuilder WithInputReadFiles(IEnumerable<string> patterns)
        {
            if (patterns is object)
            {
                _actions.Add(x => x.InputModules.Add(new ReadFiles((Config<IEnumerable<string>>)patterns)));
            }
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles()
        {
            _actions.Add(x => x.OutputModules.Add(new WriteFiles()));
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles(string extension)
        {
            _actions.Add(x => x.OutputModules.Add(
                new SetDestination(extension),
                new WriteFiles()));
            return this;
        }

        public PipelineBuilder WithOutputWriteFiles(Config<NormalizedPath> path)
        {
            _actions.Add(x => x.OutputModules.Add(
                new SetDestination(path),
                new WriteFiles()));
            return this;
        }

        public PipelineBuilder AsSerial()
        {
            // Make sure not to add isolated pipelines as dependencies
            _actions.Add(x => x.Dependencies.AddRange(_collection.AsEnumerable().Where(p => !p.Value.Isolated).Select(p => p.Key)));
            return this;
        }

        public PipelineBuilder WithInputModules(IEnumerable<IModule> modules)
        {
            if (modules is object)
            {
                _actions.Add(x => x.WithInputModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithInputModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithInputModules(modules));
            return this;
        }

        public PipelineBuilder WithProcessModules(IEnumerable<IModule> modules)
        {
            if (modules is object)
            {
                _actions.Add(x => x.WithProcessModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithProcessModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithPostProcessModules(IEnumerable<IModule> modules)
        {
            if (modules is object)
            {
                _actions.Add(x => x.WithPostProcessModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithPostProcessModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithPostProcessModules(modules));
            return this;
        }

        public PipelineBuilder WithOutputModules(IEnumerable<IModule> modules)
        {
            if (modules is object)
            {
                _actions.Add(x => x.WithOutputModules(modules));
            }
            return this;
        }

        public PipelineBuilder WithOutputModules(params IModule[] modules)
        {
            _actions.Add(x => x.WithOutputModules(modules));
            return this;
        }

        public PipelineBuilder WithInputConfig(Config<object> config)
        {
            _actions.Add(x => x.WithInputConfig(config));
            return this;
        }

        public PipelineBuilder WithProcessConfig(Config<object> config)
        {
            _actions.Add(x => x.WithProcessConfig(config));
            return this;
        }

        public PipelineBuilder WithPostProcessConfig(Config<object> config)
        {
            _actions.Add(x => x.WithPostProcessConfig(config));
            return this;
        }

        public PipelineBuilder WithOutputConfig(Config<object> config)
        {
            _actions.Add(x => x.WithOutputConfig(config));
            return this;
        }

        public PipelineBuilder WithDependencies(params string[] dependencies)
        {
            _actions.Add(x => x.WithDependencies(dependencies));
            return this;
        }

        public PipelineBuilder WithDependencies(IEnumerable<string> dependencies)
        {
            if (dependencies is object)
            {
                _actions.Add(x => x.WithDependencies(dependencies));
            }
            return this;
        }

        public PipelineBuilder AsDependencyOf(params string[] dependencyOf)
        {
            _actions.Add(x => x.AsDependencyOf(dependencyOf));
            return this;
        }

        public PipelineBuilder AsDependencyOf(IEnumerable<string> dependencyOf)
        {
            if (dependencyOf is object)
            {
                _actions.Add(x => x.AsDependencyOf(dependencyOf));
            }
            return this;
        }

        public PipelineBuilder AsIsolated(bool isolated = true)
        {
            _actions.Add(x => x.AsIsolated(isolated));
            return this;
        }

        public PipelineBuilder AsDeployment(bool deployment = true)
        {
            _actions.Add(x => x.AsDeployment(deployment));
            return this;
        }

        public PipelineBuilder AsPostProcessHasDependencies(bool postProcessHasDependencies = true)
        {
            _actions.Add(x => x.AsPostProcessHasDependencies(postProcessHasDependencies));
            return this;
        }

        public PipelineBuilder WithExecutionPolicy(ExecutionPolicy policy)
        {
            _actions.Add(x => x.WithExecutionPolicy(policy));
            return this;
        }

        public PipelineBuilder ManuallyExecute()
        {
            _actions.Add(x => x.ManuallyExecute());
            return this;
        }

        public PipelineBuilder AlwaysExecute()
        {
            _actions.Add(x => x.AlwaysExecute());
            return this;
        }
    }
}