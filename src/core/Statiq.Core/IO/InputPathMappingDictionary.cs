using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Maps input paths to virtual paths while ensuring the destination path is relative.
    /// </summary>
    internal class InputPathMappingDictionary : IDictionary<NormalizedPath, NormalizedPath>, IReadOnlyDictionary<NormalizedPath, NormalizedPath>
    {
        private readonly ConcurrentDictionary<NormalizedPath, NormalizedPath> _dictionary = new ConcurrentDictionary<NormalizedPath, NormalizedPath>();

        public NormalizedPath this[NormalizedPath key]
        {
            get => _dictionary[key];
            set
            {
                ValidateMapping(key, value);
                _dictionary[key] = value;
            }
        }

        public ICollection<NormalizedPath> Keys => _dictionary.Keys;

        public ICollection<NormalizedPath> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        IEnumerable<NormalizedPath> IReadOnlyDictionary<NormalizedPath, NormalizedPath>.Keys => ((IReadOnlyDictionary<NormalizedPath, NormalizedPath>)_dictionary).Keys;

        IEnumerable<NormalizedPath> IReadOnlyDictionary<NormalizedPath, NormalizedPath>.Values => ((IReadOnlyDictionary<NormalizedPath, NormalizedPath>)_dictionary).Values;

        public void Add(NormalizedPath key, NormalizedPath value)
        {
            ValidateMapping(key, value);
            ((IDictionary<NormalizedPath, NormalizedPath>)_dictionary).Add(key, value);
        }

        public void Add(KeyValuePair<NormalizedPath, NormalizedPath> item)
        {
            ValidateMapping(item.Key, item.Value);
            ((ICollection<KeyValuePair<NormalizedPath, NormalizedPath>>)_dictionary).Add(item);
        }

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<NormalizedPath, NormalizedPath> item) =>
            ((ICollection<KeyValuePair<NormalizedPath, NormalizedPath>>)_dictionary).Contains(item);

        public bool ContainsKey(NormalizedPath key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<NormalizedPath, NormalizedPath>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<NormalizedPath, NormalizedPath>>)_dictionary).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<NormalizedPath, NormalizedPath>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool Remove(NormalizedPath key) => ((IDictionary<NormalizedPath, NormalizedPath>)_dictionary).Remove(key);

        public bool Remove(KeyValuePair<NormalizedPath, NormalizedPath> item) =>
            ((ICollection<KeyValuePair<NormalizedPath, NormalizedPath>>)_dictionary).Remove(item);

        public bool TryGetValue(NormalizedPath key, [MaybeNullWhen(false)] out NormalizedPath value) => _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();

        private void ValidateMapping(NormalizedPath key, NormalizedPath value)
        {
            if (value.IsAbsolute)
            {
                throw new ArgumentException($"Mapped paths can only be relative (\"{value}\" is absolute)");
            }
        }
    }
}
