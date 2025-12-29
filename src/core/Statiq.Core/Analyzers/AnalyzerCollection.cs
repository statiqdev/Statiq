using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerCollection : IAnalyzerCollection
    {
        private readonly Dictionary<string, IAnalyzer> _analyzers = new Dictionary<string, IAnalyzer>(StringComparer.OrdinalIgnoreCase);

        private readonly Engine _engine;

        internal AnalyzerCollection(Engine engine)
        {
            _engine = engine;
        }

        public void Add(string name, IAnalyzer analyzer)
        {
            name.ThrowIfNullOrWhiteSpace(nameof(name));
            analyzer.ThrowIfNull(nameof(analyzer));

            if (name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Analyzer names must not contain whitespace", nameof(name));
            }
            if (_analyzers.ContainsKey(name))
            {
                throw new ArgumentException($"An analyzer with the name {name} already exists", nameof(name));
            }
            _analyzers.Add(name, analyzer);
        }

        internal void LogResults(IEnumerable<AnalyzerResult> results)
        {
            if (results is object)
            {
                // Log the document results first grouped and sorted by document
                foreach (IGrouping<IDocument, AnalyzerResult> documentGroup in results
                    .Where(x => x.Document is object)
                    .GroupBy(x => x.Document)
                    .OrderBy(x => x.Key.ToSafeDisplayString()))
                {
                    LogResults(documentGroup.Key, documentGroup);
                }

                // Then log general results
                LogResults(null, results.Where(x => x.Document is null));
            }
        }

        private void LogResults(IDocument document, IEnumerable<AnalyzerResult> results)
        {
            foreach (AnalyzerResult result in results.OrderBy(x => x.AnalyzerName))
            {
                _engine.Logger.Log(result.LogLevel, document, $"{result.Message} ({result.AnalyzerName})");
            }
        }

        public IAnalyzer this[string key] => _analyzers[key];

        public IEnumerable<string> Keys => _analyzers.Keys;

        public IEnumerable<IAnalyzer> Values => _analyzers.Values;

        public int Count => _analyzers.Count;

        public bool ContainsKey(string key) => _analyzers.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, IAnalyzer>> GetEnumerator() => _analyzers.GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IAnalyzer value) => _analyzers.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _analyzers.GetEnumerator();
    }
}
