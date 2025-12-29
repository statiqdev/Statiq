using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestPipelineCollection : IPipelineCollection
    {
        private readonly Dictionary<string, IPipeline> _pipelines =
            new Dictionary<string, IPipeline>(StringComparer.OrdinalIgnoreCase);

        public IPipeline Add(string name)
        {
            IPipeline pipeline = new TestPipeline();
            Add(name, pipeline);
            return pipeline;
        }

        public IPipeline this[string name]
        {
            get => _pipelines[name];
            set => _pipelines[name] = value.ThrowIfNull(nameof(value));
        }

        public ICollection<string> Keys => ((IDictionary<string, IPipeline>)_pipelines).Keys;

        public ICollection<IPipeline> Values => ((IDictionary<string, IPipeline>)_pipelines).Values;

        public int Count => _pipelines.Count;

        public bool IsReadOnly => ((IDictionary<string, IPipeline>)_pipelines).IsReadOnly;

        public void Add(string name, IPipeline value) =>
            _pipelines.Add(
                name.ThrowIfNull(nameof(name)),
                value.ThrowIfNull(nameof(value)));

        public void Add(KeyValuePair<string, IPipeline> item)
        {
            item.Value.ThrowIfNull(nameof(item.Value));
            ((IDictionary<string, IPipeline>)_pipelines).Add(item);
        }

        public void Clear() => _pipelines.Clear();

        public bool Contains(KeyValuePair<string, IPipeline> item) => ((IDictionary<string, IPipeline>)_pipelines).Contains(item);

        public bool ContainsKey(string key) => _pipelines.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, IPipeline>[] array, int arrayIndex) =>
            ((IDictionary<string, IPipeline>)_pipelines).CopyTo(array, arrayIndex);

        public bool Remove(string name) => _pipelines.Remove(name);

        public bool Remove(KeyValuePair<string, IPipeline> item) =>
            ((IDictionary<string, IPipeline>)_pipelines).Remove(item);

        public bool TryGetValue(string key, out IPipeline value) =>
            _pipelines.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        // IReadOnlyPipelineCollection

        IEnumerable<string> IReadOnlyDictionary<string, IReadOnlyPipeline>.Keys => Keys;

        IEnumerable<IReadOnlyPipeline> IReadOnlyDictionary<string, IReadOnlyPipeline>.Values => Values;

        IReadOnlyPipeline IReadOnlyDictionary<string, IReadOnlyPipeline>.this[string key] => this[key];

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IReadOnlyPipeline value)
        {
            if (TryGetValue(key, out IPipeline pipeline))
            {
                value = pipeline;
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<string, IReadOnlyPipeline>> IEnumerable<KeyValuePair<string, IReadOnlyPipeline>>.GetEnumerator() =>
            _pipelines.Select(x => new KeyValuePair<string, IReadOnlyPipeline>(x.Key, x.Value)).GetEnumerator();
    }
}