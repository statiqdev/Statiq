using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ConcurrentCollections;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    internal abstract class CachingCompiler
    {
        private readonly ConcurrentCache<CompilerCacheKey, CompilationResult> _compilationCache
            = new ConcurrentCache<CompilerCacheKey, CompilationResult>(false);

        // Used to track compilation result requests on each execution so stale cache entries can be cleared
        private readonly ConcurrentHashSet<CompilerCacheKey> _requestedCompilationResults = new ConcurrentHashSet<CompilerCacheKey>();

        private readonly object _phasesInitializationLock = new object();
        private bool _phasesInitialized;

        // We need to initialize lazily since restoring from the cache won't have the actual namespaces, only a cache code
        // This also needs to be initialized once-per-compiler and each one must have a different RazorProjectEngine for each
        protected void EnsurePhases(RazorProjectEngine projectEngine, string[] namespaces, CompilationParameters parameters)
        {
            if (!_phasesInitialized)
            {
                lock (_phasesInitializationLock)
                {
                    // We need to register a new document classifier phase because builder.SetBaseType() (which uses builder.ConfigureClass())
                    // use the DefaultRazorDocumentClassifierPhase which stops applying document classifier passes after DocumentIntermediateNode.DocumentKind is set
                    // (which gets set by the Razor document classifier passes registered in RazorExtensions.Register())
                    // Also need to add it just after the DocumentClassifierPhase, otherwise it'll miss the C# lowering phase
                    List<IRazorEnginePhase> phases = projectEngine.Engine.Phases.ToList();
                    StatiqDocumentClassifierPhase phase = parameters is object
                        ? new StatiqDocumentClassifierPhase(parameters.BasePageType, parameters.IsDocumentModel, namespaces, projectEngine.Engine) // Normal views with base page type declaration
                        : new StatiqDocumentClassifierPhase(namespaces, projectEngine.Engine); // Layouts and partials without the basepage type declaration
                    phases.Insert(phases.IndexOf(phases.OfType<IRazorDocumentClassifierPhase>().Last()) + 1, phase);
                    FieldInfo phasesField = projectEngine.Engine.GetType().GetField("<Phases>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    phasesField.SetValue(projectEngine.Engine, phases.ToArray());
                }
                _phasesInitialized = true;
            }
        }

        /// <summary>
        /// Populates the compiler cache with existing items.
        /// </summary>
        public int PopulateCache(IEnumerable<KeyValuePair<AssemblyCacheKey, string>> items)
        {
            int count = 0;
            foreach (KeyValuePair<AssemblyCacheKey, string> item in items)
            {
                try
                {
                    _compilationCache.TryAdd(
                        item.Key.CompilerCacheKey,
                        new FileCompilationResult(item.Value));
                    count++;
                }
                catch (Exception ex)
                {
                    IExecutionContext.Current.LogDebug($"Could not load Razor assembly at {item.Value}: {ex.Message}");
                }
            }
            return count;
        }

        /// <summary>
        /// Resets the cache and expires change tokens (typically called after each execution).
        /// </summary>
        /// <returns>The current compiler cache after removing stale entries.</returns>
        public IReadOnlyDictionary<CompilerCacheKey, CompilationResult> ResetCache()
        {
            // Remove any compilations that weren't requested in the last run
            int removed = 0;
            foreach (CompilerCacheKey compilationCacheKey in _compilationCache.Keys.ToArray())
            {
                if (!_requestedCompilationResults.Contains(compilationCacheKey)
                    && _compilationCache.TryRemove(compilationCacheKey, out CompilationResult compilationResult))
                {
                    // This will unload the assembly and dispose the memory streams if a dynamic compilation
                    compilationResult.Dispose();
                    removed++;
                }
            }
            _requestedCompilationResults.Clear();
            IExecutionContext.Current.LogDebug($"Removed {removed} stale Razor compilation results from the cache");
            return _compilationCache;
        }

        protected CompilationResult GetOrAddCachedCompilation(CompilerCacheKey cacheKey, Func<CompilerCacheKey, CompilationResult> valueFactory)
        {
            _requestedCompilationResults.Add(cacheKey);
            return _compilationCache.GetOrAdd(cacheKey, valueFactory);
        }
    }
}