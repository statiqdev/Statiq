using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Statiq.Common;

namespace Statiq.Razor
{
    internal class DynamicCompilationResult : CompilationResult
    {
        private readonly object _loadLock = new object();

        private MemoryStream _assemblyStream;

        private MemoryStream _pdbStream;

        private CollectibleAssemblyLoadContext _loadContext;

        private RazorCompiledItem _compiledItem;

        private bool _disposed;

        public DynamicCompilationResult(MemoryStream assemblyStream, MemoryStream pdbStream, string assemblyName)
        {
            _assemblyStream = assemblyStream;
            _pdbStream = pdbStream;
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }

        public async Task<string> SaveToCacheAsync(IReadOnlyFileSystem fileSystem, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DynamicCompilationResult));
            }

            // Only save if we haven't already (shouldn't get here, but just in case)
            if (_assemblyStream is object)
            {
                string assemblyFileName = $"razor/{AssemblyName}.dll";
                IFile assemblyFile = fileSystem.GetCacheFile(assemblyFileName);
                _assemblyStream.Seek(0, SeekOrigin.Begin);
                await assemblyFile.WriteFromAsync(_assemblyStream, cancellationToken: cancellationToken);

                // Also save the PDB if we have one
                if (_pdbStream is object)
                {
                    IFile pdbFile = fileSystem.GetCacheFile(Path.ChangeExtension(assemblyFileName, ".pdb"));
                    _pdbStream.Seek(0, SeekOrigin.Begin);
                    await pdbFile.WriteFromAsync(_pdbStream, cancellationToken: cancellationToken);
                }

                return assemblyFileName;
            }

            return null;
        }

        public override RazorCompiledItem GetCompiledItem()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DynamicCompilationResult));
            }

            lock (_loadLock)
            {
                if (_compiledItem is null)
                {
                    _loadContext = new CollectibleAssemblyLoadContext();
                    _assemblyStream.Seek(0, SeekOrigin.Begin);
                    if (_pdbStream is object)
                    {
                        _pdbStream.Seek(0, SeekOrigin.Begin);
                    }
                    Assembly assembly = _loadContext.LoadFromStream(_assemblyStream, _pdbStream);
                    _compiledItem = StatiqViewCompiler.CompiledItemLoader.LoadItems(assembly).SingleOrDefault();
                }
                return _compiledItem;
            }
        }

        /// <summary>
        /// Disposes the memory streams and returns them to the pool (if they were recyclable).
        /// Can be called after the assembly has been written to disk for caching.
        /// </summary>
        public void DisposeMemoryStreams()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DynamicCompilationResult));
            }

            if (_assemblyStream is object)
            {
                _assemblyStream.Dispose();
                _assemblyStream = null;
            }
            if (_pdbStream is object)
            {
                _pdbStream.Dispose();
                _pdbStream = null;
            }
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DynamicCompilationResult));
            }

            if (_compiledItem is object)
            {
                _compiledItem = null;
                _loadContext.Unload();
            }

            DisposeMemoryStreams();

            _disposed = true;
        }
    }
}