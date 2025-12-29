using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a pipeline in order to provide an alternate name during registration.
    /// </summary>
    public class NamedPipelineWrapper : INamedPipelineWrapper
    {
        public NamedPipelineWrapper(string name, IPipeline pipeline)
        {
            PipelineName = name.ThrowIfNullOrEmpty(nameof(name));
            Pipeline = pipeline.ThrowIfNull(nameof(pipeline));
        }

        public string PipelineName { get; }

        public IPipeline Pipeline { get; }

        // We'll unwrap the pipeline during registration, but forward the interface anyway just in case

        public ModuleList InputModules => Pipeline.InputModules;

        public ModuleList ProcessModules => Pipeline.ProcessModules;

        public ModuleList PostProcessModules => Pipeline.PostProcessModules;

        public ModuleList OutputModules => Pipeline.OutputModules;

        public HashSet<string> Dependencies => Pipeline.Dependencies;

        public HashSet<string> DependencyOf => Pipeline.DependencyOf;

        public bool Isolated
        {
            get => Pipeline.Isolated;
            set => Pipeline.Isolated = value;
        }

        public bool Deployment
        {
            get => Pipeline.Deployment;
            set => Pipeline.Deployment = value;
        }

        public bool PostProcessHasDependencies
        {
            get => Pipeline.PostProcessHasDependencies;
            set => Pipeline.PostProcessHasDependencies = value;
        }

        public ExecutionPolicy ExecutionPolicy
        {
            get => Pipeline.ExecutionPolicy;
            set => Pipeline.ExecutionPolicy = value;
        }

        IReadOnlyCollection<string> IReadOnlyPipeline.Dependencies => Pipeline.Dependencies;

        IReadOnlyCollection<string> IReadOnlyPipeline.DependencyOf => Pipeline.DependencyOf;

        bool IReadOnlyPipeline.Isolated => Pipeline.Isolated;

        bool IReadOnlyPipeline.Deployment => Pipeline.Deployment;

        ExecutionPolicy IReadOnlyPipeline.ExecutionPolicy => Pipeline.ExecutionPolicy;
    }
}