using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    public class MultiValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IEnumerable<TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, ICollection<TValue>> _dictionary;
        private readonly Func<TKey, ICollection<TValue>> _collectionFactory;

        public MultiValueDictionary()
            : this(true)
        {
        }

        public MultiValueDictionary(bool allowDuplicateValues)
            : this(allowDuplicateValues, null)
        {
        }

        public MultiValueDictionary(bool allowDuplicateValues, IEqualityComparer<TKey> keyComparer)
            : this(keyComparer, allowDuplicateValues ? (Func<TKey, ICollection<TValue>>)(_ => new List<TValue>()) : (_ => new HashSet<TValue>()))
        {
        }

        // Does not allow duplicates, uses value comparer to compare, H
        public MultiValueDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : this(keyComparer, _ => new HashSet<TValue>(valueComparer))
        {
        }

        public MultiValueDictionary(IEqualityComparer<TKey> keyComparer, Func<TKey, ICollection<TValue>> collectionFactory)
        {
            _dictionary = new Dictionary<TKey, ICollection<TValue>>(keyComparer);
            _collectionFactory = collectionFactory.ThrowIfNull(nameof(collectionFactory));
        }

        public void Add(TKey key, TValue value)
        {
            if (!_dictionary.TryGetValue(key, out ICollection<TValue> collection))
            {
                collection = _collectionFactory(key);
                if (collection is null)
                {
                    throw new Exception("Could not get collection from factory");
                }
                _dictionary.Add(key, collection);
            }
            collection.Add(value);
        }

        public bool Contains(TKey key, TValue value) =>
            _dictionary.TryGetValue(key, out ICollection<TValue> collection) && collection.Contains(value);

        public IEnumerable<TValue> this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        IEnumerable<IEnumerable<TValue>> IReadOnlyDictionary<TKey, IEnumerable<TValue>>.Values => _dictionary.Values;

        public IEnumerable<TValue> Values => _dictionary.Values.SelectMany(x => x);

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        bool IReadOnlyDictionary<TKey, IEnumerable<TValue>>.TryGetValue(TKey key, [MaybeNullWhen(false)] out IEnumerable<TValue> value)
        {
            if (_dictionary.TryGetValue(key, out ICollection<TValue> collection))
            {
                value = collection;
                return true;
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            _dictionary.SelectMany(x => x.Value.Select(v => new KeyValuePair<TKey, TValue>(x.Key, v))).GetEnumerator();

        IEnumerator<KeyValuePair<TKey, IEnumerable<TValue>>> IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>>.GetEnumerator() =>
            _dictionary.Select(x => new KeyValuePair<TKey, IEnumerable<TValue>>(x.Key, x.Value)).GetEnumerator();
    }
}
