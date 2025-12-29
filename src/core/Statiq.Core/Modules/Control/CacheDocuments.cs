using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Caches documents between executions.
    /// </summary>
    /// <remarks>
    /// This module will execute child modules and cache the output documents. On following
    /// executions, the cached documents will be output if the input documents (as well as
    /// any other specified documents) match the original inputs (as defined by a hash of
    /// the document content and metadata). Using this module can greatly improve performance
    /// on re-execution after making changes in preview mode.
    /// </remarks>
    /// <remarks>
    /// In general, caching works best when there is a one-to-one relationship between input and
    /// output documents. Modules that aggregate documents into groups such as <see cref="CreateTree"/>
    /// should not be used as child modules of the cache module. Instead they should appear after
    /// the cache module and operate on the cached outputs.
    /// </remarks>
    /// <remarks>
    /// The input documents to child modules will consist only of cache misses. If a child module needs
    /// to access the full set of input documents, it can do so via <see cref="IExecutionContext.Parent"/>.
    /// </remarks>
    /// <metadata cref="Keys.DisableCache" usage="Input" />
    /// <metadata cref="Keys.ResetCache" usage="Input" />
    /// <category name="Control" />
    public class CacheDocuments : ParentModule, IDisposable
    {
        private Dictionary<NormalizedPath, CacheEntry> _cache = null;
        private int _dependentPipelinesHash;

        private Config<bool> _invalidateDocuments;
        private string[] _pipelineDependencies;
        private Config<IEnumerable<IDocument>> _documentDependencies;
        private bool _withoutSourceMapping;

        public CacheDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        /// <summary>
        /// Specifies whether a particular document should be invalidated.
        /// </summary>
        /// <remarks>
        /// Because documents are tracked in the cache by <see cref="IDocument.Source"/>, if
        /// multiple input documents have the same source and one of them is invalidated,
        /// all of the documents with that source will be considered cache misses.
        /// </remarks>
        /// <param name="invalidateDocuments">
        /// A config delegate that should return <c>true</c> if the current cached document
        /// should be invalidated, <c>false otherwise</c>.
        /// </param>
        /// <returns>The current module instance.</returns>
        public CacheDocuments InvalidateDocumentsWhere(Config<bool> invalidateDocuments)
        {
            _invalidateDocuments = invalidateDocuments;
            return this;
        }

        /// <summary>
        /// Sets the pipelines that the cache depends on. If any document in a dependent pipeline
        /// changes, the entire cache is invalidated.
        /// </summary>
        /// <remarks>
        /// The default behavior is no dependent pipelines unless the cache module is used in the
        /// process phase in which case documents from all the pipeline dependencies of the current
        /// pipeline will be used. Set <paramref name="pipelineDependencies"/> to an empty array to
        /// override this behavior and not use any dependent pipelines in the process phase or
        /// <c>null</c> to reset to the default behavior. If the cache is used in the transform phase
        /// and the child modules use documents from any other pipelines, those pipelines should
        /// be specified.
        /// </remarks>
        /// <param name="pipelineDependencies">The pipelines the cache depends on.</param>
        /// <returns>The current module instance.</returns>
        public CacheDocuments WithPipelineDependencies(params string[] pipelineDependencies)
        {
            _pipelineDependencies = pipelineDependencies;
            return this;
        }

        /// <summary>
        /// Specifies additional documents that a document cache entry should depend on.
        /// </summary>
        /// <remarks>
        /// This should return the same documents on each execution. The aggregate hash of all
        /// configuration result documents is combined with the hash of the input document(s)
        /// to determine if the cache entry is a hit or a miss.
        /// </remarks>
        /// <param name="documentDependencies">Returns additional document dependencies for each input document.</param>
        /// <returns>The current module instance.</returns>
        public CacheDocuments WithDocumentDependencies(Config<IEnumerable<IDocument>> documentDependencies)
        {
            _documentDependencies = documentDependencies;
            return this;
        }

        /// <summary>
        /// Specifies whether source mapping of inputs to outputs should be used (the default is to use source mapping).
        /// </summary>
        /// <remarks>
        /// By default output documents are cached based on mapping their document source to the same source
        /// from input documents. This allows for fine-grained cache invalidation. Setting this to <c>true</c>
        /// will treat all input documents as a single unit and will invalidate the entire cache if any of them change.
        /// </remarks>
        /// <param name="withoutSourceMapping">
        /// <c>true</c> if source mapping should be disabled and a change in
        /// any input document should result in invalidating the entire cache.
        /// </param>
        /// <returns>The current module instance.</returns>
        public CacheDocuments WithoutSourceMapping(bool withoutSourceMapping = true)
        {
            _withoutSourceMapping = withoutSourceMapping;
            return this;
        }

        // Called when the engine is disposed just in case the module gets reused in another engine (unlikely)
        public void Dispose() => ResetCache();

        private void ResetCache()
        {
            _cache = null;
            _dependentPipelinesHash = default;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // If we're disabling the cache, clear any existing entries and just execute children
            if (context.Settings.GetBool(Keys.DisableCache))
            {
                context.LogInformation($"Cache is disabled due to {nameof(Keys.DisableCache)} metadata");
                ResetCache();
                return await context.ExecuteModulesAsync(Children, context.Inputs);
            }

            // If we're resetting the cache, reset it but then continue
            if (context.Settings.GetBool(Keys.ResetCache))
            {
                context.LogInformation($"Resetting cache due to {nameof(Keys.ResetCache)} metadata");
                ResetCache();
            }

            // Calculate the aggregate hash of dependent pipelines
            int dependentPipelinesHash = default;
            if (_pipelineDependencies?.Length > 0)
            {
                // If we've specified some dependent pipelines, calculate the hash of all their outputs
                dependentPipelinesHash = await CombineCacheCodesAsync(
                    context.Outputs.FromPipelines(_pipelineDependencies), null);
            }
            else if (_pipelineDependencies is null && context.Phase == Phase.Process)
            {
                // If we're in the process phase, the default behavior is to depend on all pipeline dependencies
                dependentPipelinesHash = await CombineCacheCodesAsync(
                    context.Outputs.FromPipelines(context.Pipeline.Dependencies.ToArray()), null);
            }
            if (_dependentPipelinesHash != dependentPipelinesHash)
            {
                if (_dependentPipelinesHash != default)
                {
                    // We don't need to log if this is the first time through or cache was reset
                    context.LogInformation($"Resetting cache due to changes to dependent pipeline outputs ({string.Join(", ", context.Pipeline.Dependencies)})");
                }
                ResetCache();
                _dependentPipelinesHash = dependentPipelinesHash;
            }

            List<IDocument> misses = new List<IDocument>();
            List<IDocument> outputs = new List<IDocument>();

            // Need to track misses by their source and map it to the aggregate input document hash
            // Go ahead and calculate a single hash for all input documents since we'd end up having to
            // get each individual hash anyway, whether while checking for a complete hit or recording for the next time
            Dictionary<NormalizedPath, int> missesBySource = new Dictionary<NormalizedPath, int>();

            // Creating a new cache and swapping is the easiest way to expire old entries
            Dictionary<NormalizedPath, CacheEntry> currentCache = _cache;
            _cache = new Dictionary<NormalizedPath, CacheEntry>();

            // Check for hits and misses
            if (currentCache is null)
            {
                // If the current cache is null, this is the first run through
                misses.AddRange(context.Inputs);
                IEnumerable<IGrouping<NormalizedPath, IDocument>> inputGroups = _withoutSourceMapping
                    ? new[]
                    {
                        // If we're not mapping sources, group everything under an empty path
                        new ExplicitGrouping<NormalizedPath, IDocument>(NormalizedPath.Empty, context.Inputs)
                    }
                    : context.Inputs
                        .Where(input => !input.Source.IsNull)
                        .GroupBy(input => input.Source);
                foreach (IGrouping<NormalizedPath, IDocument> inputGroup in inputGroups)
                {
                    missesBySource.Add(inputGroup.Key, await CombineCacheCodesAsync(inputGroup, context));
                }
            }
            else
            {
                // Note that due to cloning we could have multiple inputs and outputs with the same source
                // so we need to check all inputs with a given source and only consider a hit when they all match
                IEnumerable<IGrouping<NormalizedPath, IDocument>> inputGroups = _withoutSourceMapping
                    ? new[]
                    {
                        // If we're not mapping sources, group everything under an empty path
                        new ExplicitGrouping<NormalizedPath, IDocument>(NormalizedPath.Empty, context.Inputs)
                    }
                    : context.Inputs.GroupBy(input => input.Source);
                foreach (IGrouping<NormalizedPath, IDocument> inputGroup in inputGroups)
                {
                    string message = null;

                    // Documents with a null source are never cached
                    if (!inputGroup.Key.IsNull)
                    {
                        // Get the aggregate input hash for all documents with this source and all document dependencies
                        // (we need it one way or the other to check if it's a hit or to record for next time)
                        int inputsHash = await CombineCacheCodesAsync(inputGroup, context);

                        // Get the cache entry for this source if there is one
                        if (currentCache.TryGetValue(inputGroup.Key, out CacheEntry entry))
                        {
                            // If the aggregate hash for all inputs with this source matches then it's a hit
                            if (inputsHash == entry.InputsHash)
                            {
                                context.LogDebug($"Hashes match for source {inputGroup.Key}");

                                // Check the user-supplied predicate against each document with this source
                                bool invalidated = false;
                                if (_invalidateDocuments is object)
                                {
                                    foreach (IDocument input in inputGroup)
                                    {
                                        if (await _invalidateDocuments.GetValueAsync(input, context))
                                        {
                                            invalidated = true;
                                            break;
                                        }
                                    }
                                }
                                if (!invalidated)
                                {
                                    context.LogDebug($"Cache hit for documents with source {inputGroup.Key}, using cached outputs");
                                    _cache.Add(inputGroup.Key, entry);
                                    outputs.AddRange(entry.OutputDocuments);
                                    continue;  // Go to the next source group since misses are dealt with below
                                }
                                message = $"Cache miss for documents with source {inputGroup.Key}: document(s) were invalidated";
                            }
                            else
                            {
                                // Miss due to corresponding input hash not matching
                                message = $"Cache miss for documents with source {inputGroup.Key}: dependent documents have changed";
                            }
                        }

                        // Miss with non-null source: track source and input documents so we can cache for next pass
                        message ??= $"Cache miss for documents with source {inputGroup.Key}: source not cached";
                        missesBySource.Add(inputGroup.Key, inputsHash);
                    }

                    // Miss, add inputs to execute
                    context.LogDebug(message ?? "Cache miss for documents with null source: null sources are never cached");
                    misses.AddRange(inputGroup);
                }

                // Log hits and misses
                context.LogInformation($"Cache resulted in {outputs.Count} cache hits and {misses.Count} cache misses");
            }

            // Execute misses
            IReadOnlyList<IDocument> results = misses.Count == 0
                ? ImmutableArray<IDocument>.Empty
                : await context.ExecuteModulesAsync(Children, misses);
            Dictionary<NormalizedPath, IGrouping<NormalizedPath, IDocument>> resultsBySource = _withoutSourceMapping
                ? new Dictionary<NormalizedPath, IGrouping<NormalizedPath, IDocument>>
                {
                    // If we're not mapping sources, group everything under an empty path
                    {
                        NormalizedPath.Empty,
                        new ExplicitGrouping<NormalizedPath, IDocument>(NormalizedPath.Empty, results)
                    }
                }
                : results.Where(x => !x.Source.IsNull).GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x);
            outputs.AddRange(results);

            // Cache all miss sources, even if they resulted in an empty result document set
            foreach (KeyValuePair<NormalizedPath, int> inputGroup in missesBySource)
            {
                // Did we get any results from this input source?
                IDocument[] cacheDocuments = null;
                if (resultsBySource.TryGetValue(inputGroup.Key, out IGrouping<NormalizedPath, IDocument> sourceResults))
                {
                    cacheDocuments = sourceResults.ToArray();
                }

                // Add a cache entry
                _cache.Add(inputGroup.Key, new CacheEntry(inputGroup.Value, cacheDocuments));
            }

            return outputs;
        }

        // Pass null context to disable checking document dependencies
        private async Task<int> CombineCacheCodesAsync(IEnumerable<IDocument> documents, IExecutionContext context)
        {
            CacheCode cacheCode = new CacheCode();

            // Get document dependencies in a first pass so we can de-dupe them
            if (context is object)
            {
                HashSet<IDocument> documentDependencies = new HashSet<IDocument>();
                if (_documentDependencies is object)
                {
                    foreach (IDocument document in documents)
                    {
                        IEnumerable<IDocument> dependencies = await _documentDependencies.GetValueAsync(document, context);
                        if (dependencies is object)
                        {
                            foreach (IDocument dependency in dependencies)
                            {
                                if (documentDependencies.Add(dependency))
                                {
                                    await cacheCode.AddAsync(dependency);
                                }
                            }
                        }
                    }
                }
            }

            // Now add hash codes for all input documents
            foreach (IDocument document in documents)
            {
                await cacheCode.AddAsync(document);
            }

            return cacheCode.ToCacheCode();
        }

        private class CacheEntry
        {
            public CacheEntry(int inputsHash, IDocument[] outputDocuments)
            {
                InputsHash = inputsHash;
                OutputDocuments = outputDocuments ?? Array.Empty<IDocument>();
            }

            public int InputsHash { get; }

            public IDocument[] OutputDocuments { get; }
        }
    }
}