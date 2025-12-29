using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a named collection of modules that should be executed by the engine.
    /// </summary>
    /// <remarks>
    /// If the pipeline implements <see cref="IDisposable"/>, <see cref="IDisposable.Dispose"/>
    /// will be called when the engine is disposed (I.e., on application exit).
    /// </remarks>
    public interface IPipeline : IReadOnlyPipeline
    {
        /// <summary>
        /// Modules that will execute during the input phase.
        /// </summary>
        ModuleList InputModules { get; }

        /// <summary>
        /// Modules that will execute during the process phase.
        /// </summary>
        ModuleList ProcessModules { get; }

        /// <summary>
        /// Modules that will execute during the post-process phase.
        /// </summary>
        ModuleList PostProcessModules { get; }

        /// <summary>
        /// Modules that will execute during the output phase.
        /// </summary>
        ModuleList OutputModules { get; }

        /// <inheritdoc/>
        new HashSet<string> Dependencies { get; }

        /// <inheritdoc/>
        new HashSet<string> DependencyOf { get; }

        /// <inheritdoc/>
        new bool Isolated { get; set; }

        /// <inheritdoc/>
        new bool Deployment { get; set; }

        /// <inheritdoc/>
        new bool PostProcessHasDependencies { get; set; }

        /// <inheritdoc/>
        new ExecutionPolicy ExecutionPolicy { get; set; }
    }
}