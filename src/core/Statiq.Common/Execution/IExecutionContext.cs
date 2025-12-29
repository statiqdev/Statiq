using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// All of the information that represents a given build. Also implements
    /// <see cref="IMetadata"/> to expose the global metadata.
    /// </summary>
    public interface IExecutionContext : IExecutionState, IMetadata, IServiceProvider, ILogger
    {
        private static readonly AsyncLocal<IExecutionContext> _current = new AsyncLocal<IExecutionContext>();

        /// <summary>
        /// The current execution context.
        /// </summary>
        /// <remarks>
        /// This will return an empty execution context with default values if not currently executing.
        /// </remarks>
        public static new IExecutionContext Current
        {
            get => _current.Value ?? CurrentEmptyExecutionContext;
            internal set => _current.Value = value;
        }

        /// <summary>
        /// Gets the current execution state.
        /// </summary>
        IExecutionState ExecutionState { get; }

        /// <summary>
        /// Gets the name of the currently executing pipeline.
        /// </summary>
        string PipelineName { get; }

        /// <summary>
        /// Gets the currently executing pipeline.
        /// </summary>
        IReadOnlyPipeline Pipeline { get; }

        /// <summary>
        /// Gets the currently executing pipeline phase.
        /// </summary>
        Phase Phase { get; }

        /// <summary>
        /// The parent execution context if this is a nested module execution,
        /// <c>null</c> otherwise.
        /// </summary>
        IExecutionContext Parent { get; }

        /// <summary>
        /// Gets the currently executing module.
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// The input documents to process.
        /// </summary>
        ImmutableArray<IDocument> Inputs { get; }

        /// <summary>
        /// Executes the specified modules with the specified input documents and returns the result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="inputs">The documents to execute the modules on.</param>
        /// <returns>The result documents from the executed modules.</returns>
        Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        // IMetadata

        bool IMetadata.TryGetRaw(string key, out object value) => Settings.TryGetRaw(key, out value);

        IEnumerator<KeyValuePair<string, object>> IMetadata.GetRawEnumerator() => Settings.GetRawEnumerator();

        // IReadOnlyDictionary<string, object>

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Settings.Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Settings.Values;

        object IReadOnlyDictionary<string, object>.this[string key] => Settings[key];

        bool IReadOnlyDictionary<string, object>.ContainsKey(string key) => Settings.ContainsKey(key);

        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => Settings.TryGetValue(key, out value);

        // IReadOnlyCollection<KeyValuePair<string, object>>

        int IReadOnlyCollection<KeyValuePair<string, object>>.Count => Settings.Count;

        // IEnumerable<KeyValuePair<string, object>>

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => Settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Settings.GetEnumerator();
    }
}
