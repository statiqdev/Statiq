using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A dictionary with metadata type conversion superpowers.
    /// </summary>
    /// <remarks>
    /// This class wraps an underlying <see cref="IDictionary{TKey, TValue}"/> but
    /// uses the provided <see cref="IExecutionContext"/> to perform type conversions
    /// when requesting values.
    /// </remarks>
    public class ConvertingDictionary : IMetadataDictionary
    {
        private readonly IDictionary<string, object> _dictionary;

        public ConvertingDictionary()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ConvertingDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary.ThrowIfNull(nameof(dictionary));
        }

        public ConvertingDictionary(IEnumerable<KeyValuePair<string, object>> items)
        {
            items.ThrowIfNull(nameof(items));
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Copy over in case there are duplicate keys
            foreach (KeyValuePair<string, object> item in items)
            {
                _dictionary[item.Key] = item.Value;
            }
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                key.ThrowIfNull(nameof(key));
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }

            set => _dictionary[key] = value;
        }

        /// <inheritdoc />
        object IReadOnlyDictionary<string, object>.this[string key] => this[key];

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public ICollection<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => _dictionary.Select(x => TypeHelper.ExpandValue(x.Key, x.Value, this)).ToArray();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        /// <inheritdoc />
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        /// <inheritdoc />
        public void Add(string key, object value) => _dictionary.Add(key, value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item) => _dictionary.Add(item);

        /// <inheritdoc />
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item) =>
            _dictionary.Select(x => TypeHelper.ExpandKeyValuePair(x, this)).Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            _dictionary.Select(x => TypeHelper.ExpandKeyValuePair(x, this)).ToArray().CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool TryGetRaw(string key, out object value) => _dictionary.TryGetValue(key, out value);

        /// <inheritdoc />
        public bool Remove(string key) => _dictionary.Remove(key);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item) => _dictionary.Remove(item);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => this.TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            _dictionary.Select(x => TypeHelper.ExpandKeyValuePair(x, this)).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator() => _dictionary.GetEnumerator();
    }
}
