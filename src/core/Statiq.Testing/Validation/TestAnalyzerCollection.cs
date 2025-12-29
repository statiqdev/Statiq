using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestAnalyzerCollection : IAnalyzerCollection
    {
        public Dictionary<string, IAnalyzer> Analyzers { get; } = new Dictionary<string, IAnalyzer>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, IAnalyzer analyzer) => Analyzers.Add(name, analyzer);

        public IAnalyzer this[string key] => Analyzers[key];

        public IEnumerable<string> Keys => Analyzers.Keys;

        public IEnumerable<IAnalyzer> Values => Analyzers.Values;

        public int Count => Analyzers.Count;

        public bool ContainsKey(string key) => Analyzers.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, IAnalyzer>> GetEnumerator() => Analyzers.GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IAnalyzer value) => Analyzers.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => Analyzers.GetEnumerator();
    }
}
