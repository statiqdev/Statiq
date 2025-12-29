using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public interface IEngine : IConfigurable, IExecutionState
    {
        /// <summary>
        /// Gets the state of the application when it was run.
        /// </summary>
        new IApplicationState ApplicationState { get; }

        /// <summary>
        /// Gets global events and event handlers.
        /// </summary>
        new IEventCollection Events { get; }

        /// <summary>
        /// Gets the configuration and settings.
        /// </summary>
        new ISettings Settings { get; }

        /// <summary>
        /// Gets the shortcodes.
        /// </summary>
        new IShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Executes pipeline phases and modules in serial.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>true</c> will disable most (but not all) concurrency and is useful for debugging.
        /// </remarks>
        new bool SerialExecution { get; set; }

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        new IPipelineCollection Pipelines { get; }

        new ILogger Logger { get; }

        /// <summary>
        /// A collection of analyzers.
        /// </summary>
        IAnalyzerCollection Analyzers { get; }
    }
}