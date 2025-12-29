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
    /// This class wraps an underlying <see cref="IReadOnlyDictionary{TKey, TValue}"/> but
    /// uses the provided <see cref="IExecutionContext"/> to perform type conversions
    /// when requesting values.
    /// </remarks>
    public class ReadOnlyConvertingDictionary : IMetadata
    {
        private readonly IReadOnlyDictionary<string, object> _dictionary;

        public ReadOnlyConvertingDictionary(IReadOnlyDictionary<string, object> dictionary)
        {
            _dictionary = dictionary.ThrowIfNull(nameof(dictionary));
        }

        public ReadOnlyConvertingDictionary(IEnumerable<KeyValuePair<string, object>> items)
        {
            items.ThrowIfNull(nameof(items));
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Copy over in case there are duplicate keys
            foreach (KeyValuePair<string, object> item in items)
            {
                dictionary[item.Key] = item.Value;
            }
            _dictionary = dictionary;
        }

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public IEnumerable<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public IEnumerable<object> Values => _dictionary.Select(x => TypeHelper.ExpandValue(x.Key, x.Value, this)).ToArray();

        /// <inheritdoc />
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

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
        }

        /// <inheritdoc />
        public bool TryGetRaw(string key, out object value) => _dictionary.TryGetValue(key, out value);

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
