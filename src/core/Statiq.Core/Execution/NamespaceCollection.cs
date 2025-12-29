using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Core
{
    public class NamespaceCollection : INamespacesCollection
    {
        private readonly ConcurrentHashSet<string> _namespaces = new ConcurrentHashSet<string>();

        private readonly object _cacheCodeLock = new object();
        private int _cacheCode;

        public NamespaceCollection()
        {
            // This is the default set of namespaces that should brought in scope during configuration and in other dynamic modules
            _namespaces.AddRange(new[]
            {
                "System",
                "System.Threading.Tasks",
                "System.Collections.Generic",
                "System.Linq",
            });

            // Add all public namespaces from Statiq.Common
            _namespaces.AddRange(typeof(IEngine).Assembly.GetTypes()
                .Where(x => x.IsPublic && !x.Namespace.IsNullOrWhiteSpace())
                .Select(x => x.Namespace)
                .Distinct());
        }

        public virtual bool Add(string ns)
        {
            ns.ThrowIfNullOrWhiteSpace(nameof(ns));
            return _namespaces.Add(ns);
        }

        public virtual void AddRange(IEnumerable<string> namespaces)
        {
            // Iterate manually so we can throw for null or white space
            foreach (string ns in namespaces)
            {
                Add(ns);
            }
        }

        public virtual int Count => _namespaces.Count;

        public virtual IEnumerator<string> GetEnumerator() => _namespaces.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual Task<int> GetCacheCodeAsync()
        {
            if (_cacheCode == 0)
            {
                lock (_cacheCodeLock)
                {
                    CacheCode cacheCode = new CacheCode();
                    foreach (string ns in _namespaces.OrderBy(x => x))
                    {
                        cacheCode.Add(ns);
                    }
                    _cacheCode = cacheCode.ToCacheCode();
                }
            }
            return Task.FromResult(_cacheCode);
        }
    }
}