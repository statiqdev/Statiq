using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Statiq.Razor
{
    internal class FileCompilationResult : CompilationResult
    {
        private readonly object _loadLock = new object();

        private readonly string _assemblyPath;

        private CollectibleAssemblyLoadContext _loadContext;

        private RazorCompiledItem _compiledItem;

        private bool _disposed;

        public FileCompilationResult(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        public override RazorCompiledItem GetCompiledItem()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileCompilationResult));
            }

            lock (_loadLock)
            {
                if (_compiledItem is null)
                {
                    _loadContext = new CollectibleAssemblyLoadContext();
                    string pdbPath = Path.ChangeExtension(_assemblyPath, "pdb");
                    using (FileStream assemblyStream = new FileStream(_assemblyPath, FileMode.Open, FileAccess.Read))
                    {
                        using (FileStream pdbStream = File.Exists(pdbPath)
                           ? new FileStream(pdbPath, FileMode.Open, FileAccess.Read)
                           : null)
                        {
                            Assembly assembly = _loadContext.LoadFromStream(assemblyStream, pdbStream);
                            _compiledItem = StatiqViewCompiler.CompiledItemLoader.LoadItems(assembly).SingleOrDefault();
                        }
                    }
                }
                return _compiledItem;
            }
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileCompilationResult));
            }

            if (_compiledItem is object)
            {
                _compiledItem = null;
                _loadContext.Unload();
            }

            _disposed = true;
        }
    }
}