using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    public interface IReadOnlyPipeline
    {
        /// <summary>
        /// The names of pipelines this pipeline depends on.
        /// </summary>
        IReadOnlyCollection<string> Dependencies { get; }

        /// <summary>
        /// The names of pipelines that depend on this pipeline.
        /// </summary>
        IReadOnlyCollection<string> DependencyOf { get; }

        /// <summary>
        /// An isolated pipeline runs immediately without any dependencies and
        /// has restrictions on accessing documents from other pipelines.
        /// </summary>
        bool Isolated { get; }

        /// <summary>
        /// A deployment pipeline works just like other pipelines except
        /// it's <see cref="Phase.Output"/> phase will only execute when
        /// all other non-deployment pipelines have completed their
        /// <see cref="Phase.Output"/> phase (including isolated pipelines).
        /// </summary>
        bool Deployment { get; }

        /// <summary>
        /// Indicates that the post-process phase of this pipeline should have
        /// dependencies on the post-process phase(s) of it's dependencies.
        /// </summary>
        /// <remarks>
        /// Normally all post-process phases are executed concurrently after
        /// all process phases have completed. This also means that a post-process
        /// phase can access documents from the process phase of any pipeline (other
        /// than those with <see cref="Isolated"/> set to <c>true</c>) but not
        /// from the post-process phase in other pipelines. Sometimes it can be
        /// helpful for the post-process phase of a pipeline to itself have a
        /// dependency on the post-process phase of it's pipeline dependencies.
        /// Setting this to <c>true</c> will cause the post-process phase of this
        /// pipeline to wait to execute until all post-process phases of it's
        /// dependencies have completed, and will allow it to access documents
        /// from those post-process phases.
        /// </remarks>
        bool PostProcessHasDependencies { get; }

        /// <summary>
        /// Indicates when the pipeline is executed.
        /// </summary>
        ExecutionPolicy ExecutionPolicy { get; }
    }
}