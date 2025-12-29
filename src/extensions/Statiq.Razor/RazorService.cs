using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Razor compiler should be shared so that pages are only compiled once.
    /// </summary>
    internal class RazorService : IDisposable
    {
        private const string CacheFileName = "razorcache.json";

        private readonly ConcurrentCache<CompilationParameters, RazorCompiler> _compilers
            = new ConcurrentCache<CompilationParameters, RazorCompiler>(false, true);

        private Dictionary<AssemblyCacheKey, string> _cachedAssemblies = new Dictionary<AssemblyCacheKey, string>();

        private bool _firstExecution = true;

        private static RazorCompiler GetRazorCompiler(CompilationParameters parameters, IServiceProvider serviceProvider)
        {
            IExecutionContext.Current.LogDebug($"Creating new {nameof(RazorCompiler)} for {parameters.BasePageType ?? "null base page type"}");
            return new RazorCompiler(serviceProvider);
        }

        public async Task RenderAsync(RenderRequest request)
        {
            int namespacesCacheCode = await request.Context.Namespaces.GetCacheCodeAsync();
            await _compilers.AddOrUpdate(
                CompilationParameters.Get(namespacesCacheCode, request.BaseType, request.Model is IDocument),
                parameters =>
                {
                    // No compiler exists for these parameters so create a new one
                    RazorCompiler compiler = GetRazorCompiler(parameters, request.Context);
                    compiler.EnsurePhases(parameters, request.Context.Namespaces.ToArray());
                    return compiler;
                },
                (parameters, compiler) =>
                {
                    // We already have a compiler, so ensure the phases are set since it might have been from the cache
                    compiler.EnsurePhases(parameters, request.Context.Namespaces.ToArray());
                    return compiler;
                })
                .RenderPageAsync(request);
        }

        public async Task BeforeEngineExecutionAsync(BeforeEngineExecution args)
        {
            // Populate the cache if it's the first execution, but not if caching is disabled
            bool useCache = args.Engine.Settings.GetBool(Keys.UseCache);
            if (_firstExecution && useCache)
            {
                // See if we have an exiting assembly cache
                IFile cacheFile = args.Engine.FileSystem.GetCacheFile(CacheFileName);
                bool restoredCache = false;
                try
                {
                    if (cacheFile.Exists)
                    {
                        // See the SerializeJsonAsync below for the correct deserialization type
                        KeyValuePair<AssemblyCacheKey, string>[] cachedAssembliesArray =
                            await cacheFile.DeserializeJsonAsync<KeyValuePair<AssemblyCacheKey, string>[]>(null, args.Engine.CancellationToken);
                        _cachedAssemblies = cachedAssembliesArray.ToDictionary(x => x.Key, x => x.Value);
                        restoredCache = true;
                    }
                }
                catch (Exception ex)
                {
                    args.Engine.Logger.LogDebug($"Error while restoring Razor assembly cache file {cacheFile.Path.FullPath}: {ex.Message}");
                }

                // Populate the compiler caches
                if (restoredCache)
                {
                    // Get the single global view compiler
                    StatiqViewCompiler viewCompiler = (StatiqViewCompiler)args.Engine.Services.GetRequiredService<IViewCompilerProvider>().GetCompiler();

                    int count = 0;
                    foreach (IGrouping<CompilationParameters, KeyValuePair<AssemblyCacheKey, string>> cacheGroup in _cachedAssemblies.GroupBy(x => x.Key.CompilationParameters))
                    {
                        CachingCompiler compiler = cacheGroup.Key.Equals(viewCompiler.CompilationParameters)
                            ? (CachingCompiler)viewCompiler
                            : _compilers.GetOrAdd(
                                cacheGroup.Key,
                                (parameters, engine) => GetRazorCompiler(parameters, engine.Services),
                                args.Engine);
                        count += compiler.PopulateCache(cacheGroup.Select(x => new KeyValuePair<AssemblyCacheKey, string>(x.Key, args.Engine.FileSystem.GetCachePath(x.Value).FullPath)));
                    }
                    IExecutionContext.Current.LogInformation($"Restored Razor compilation cache from {cacheFile.Path.FullPath} with {count} assemblies");
                }
            }
            _firstExecution = false;
        }

        public async Task AfterEngineExecutionAsync(AfterEngineExecution args)
        {
            // Expire the internal Razor cache change tokens if this is a new execution
            // This needs to be done so that layouts/partials can be re-rendered if they've changed,
            // otherwise Razor will just use the previously cached version of them
            ((FileSystemFileProvider)args.Engine.Services.GetRequiredService<Microsoft.Extensions.FileProviders.IFileProvider>()).ExpireChangeTokens();

            // Get the single global view compiler
            StatiqViewCompiler viewCompiler = (StatiqViewCompiler)args.Engine.Services.GetRequiredService<IViewCompilerProvider>().GetCompiler();

            // Cache and clean up compilation assemblies from the compilers
            bool useCache = args.Engine.Settings.GetBool(Keys.UseCache);
            int removeCount = 0;
            int addCount = 0;
            foreach (KeyValuePair<CompilationParameters, CachingCompiler> compilerItem in _compilers
                .Select(x => new KeyValuePair<CompilationParameters, CachingCompiler>(x.Key, x.Value))
                .Append(new KeyValuePair<CompilationParameters, CachingCompiler>(viewCompiler.CompilationParameters, viewCompiler)))
            {
                IReadOnlyDictionary<CompilerCacheKey, CompilationResult> compilerCache = compilerItem.Value.ResetCache();

                // Remove stale entries
                foreach (AssemblyCacheKey removeAssemblyCacheKey in _cachedAssemblies.Keys.Where(x => x.CompilationParameters.Equals(compilerItem.Key)).ToArray())
                {
                    if (!compilerCache.ContainsKey(removeAssemblyCacheKey.CompilerCacheKey))
                    {
                        // The file will get deleted when we clean up the directory further down
                        _cachedAssemblies.Remove(removeAssemblyCacheKey);
                        removeCount++;
                    }
                }

                // Add new entries
                foreach (KeyValuePair<CompilerCacheKey, CompilationResult> compilerCacheItem in compilerCache)
                {
                    AssemblyCacheKey addAssemblyCacheKey = AssemblyCacheKey.Get(compilerItem.Key, compilerCacheItem.Key);
                    if (!_cachedAssemblies.ContainsKey(addAssemblyCacheKey) && compilerCacheItem.Value is DynamicCompilationResult dynamicCompilationResult)
                    {
                        // Only cache to disk if using the cache
                        if (useCache)
                        {
                            // Make a best effort to save the assemblies, but don't panic if they don't
                            try
                            {
                                string assemblyFileName = await dynamicCompilationResult.SaveToCacheAsync(
                                    args.Engine.FileSystem, args.Engine.CancellationToken);
                                if (assemblyFileName is object)
                                {
                                    _cachedAssemblies.Add(addAssemblyCacheKey, assemblyFileName);
                                    addCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                args.Engine.Logger.LogDebug($"Error while saving assembly or pdb file {dynamicCompilationResult.AssemblyName} to cache: {ex.Message}");
                            }
                        }

                        // Whether we saved the assembly or not, go ahead and dispose the memory streams so they can be collected
                        dynamicCompilationResult.DisposeMemoryStreams();
                    }
                }
            }
            args.Engine.Logger.LogInformation($"Removed {removeCount} stale Razor assemblies from the cache");
            args.Engine.Logger.LogInformation($"Cached and saved {addCount} new Razor assemblies");

            // Clean up and write the cache index if we're caching
            if (useCache)
            {
                // Clean up the cache folder
                IDirectory cacheDirectory = args.Engine.FileSystem.GetCacheDirectory("razor");
                if (cacheDirectory.Exists)
                {
                    int deleteCount = 0;
                    HashSet<string> cachedAssemblyFiles = new HashSet<string>(_cachedAssemblies.Values);
                    foreach (IFile assemblyFile in cacheDirectory.GetFiles())
                    {
                        if (!cachedAssemblyFiles.Contains($"razor/{assemblyFile.Path.FileNameWithoutExtension}.dll"))
                        {
                            try
                            {
                                assemblyFile.Delete();
                                deleteCount++;
                            }
                            catch (Exception ex)
                            {
                                args.Engine.Logger.LogDebug($"Error while deleting stale cached assembly or pdb file {assemblyFile.Path.FullPath}: {ex.Message}");
                            }
                        }
                    }
                    args.Engine.Logger.LogInformation($"Deleted {deleteCount} stale cached Razor assembly files");
                }

                // Write the updated cache file
                // System.Text.Json doesn't current support non-string dictionary keys, so write out as an array
                // See https://github.com/dotnet/runtime/issues/30524
                IFile cacheFile = args.Engine.FileSystem.GetCacheFile(CacheFileName);
                KeyValuePair<AssemblyCacheKey, string>[] cachedAssembliesArray = _cachedAssemblies.ToArray();
                await cacheFile.SerializeJsonAsync(cachedAssembliesArray, null, args.Engine.CancellationToken);
            }
        }

        public void Dispose() => _compilers.Reset();
    }
}