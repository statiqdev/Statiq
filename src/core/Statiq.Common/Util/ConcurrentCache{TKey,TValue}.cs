using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Statiq.Common
{
    /// <summary>
    /// Use this type instead of <see cref="ConcurrentDictionary{TKey, TValue}"/> in caching scenarios.
    /// </summary>
    /// <remarks>
    /// See http://reedcopsey.com/2011/01/16/concurrentdictionarytkeytvalue-used-with-lazyt/
    /// and https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/.
    /// </remarks>
    public class ConcurrentCache<TKey, TValue> : IConcurrentCache, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;

        private readonly bool _disposeValuesOnReset;

        /// <summary>
        /// Creates a thread-safe concurrent cache.
        /// </summary>
        /// <param name="resettable">
        /// Indicates if the cache should be reset by the <see cref="IEngine"/> before each execution.
        /// </param>
        public ConcurrentCache(bool resettable)
            : this(resettable, null)
        {
        }

        /// <summary>
        /// Creates a thread-safe concurrent cache.
        /// </summary>
        /// <param name="resettable">
        /// Indicates if the cache should be reset by the <see cref="IEngine"/> before each execution.
        /// </param>
        /// <param name="disposeValuesOnReset">
        /// If <c>true</c> (the default) values will be disposed when the cache is reset if they implement
        /// <see cref="IDisposable"/> (only relevant if <paramref name="resettable"/> is <c>true</c>).
        /// </param>
        public ConcurrentCache(bool resettable, bool disposeValuesOnReset)
            : this(resettable, disposeValuesOnReset, null)
        {
        }

        /// <summary>
        /// Creates a thread-safe concurrent cache.
        /// </summary>
        /// <param name="resettable">
        /// Indicates if the cache should be reset by the <see cref="IEngine"/> before each execution.
        /// </param>
        /// <param name="comparer">
        /// The key comparer to use, or <c>null</c> to use the default comparer.
        /// </param>
        public ConcurrentCache(bool resettable, IEqualityComparer<TKey> comparer)
            : this(resettable, true, comparer)
        {
        }

        /// <summary>
        /// Creates a thread-safe concurrent cache.
        /// </summary>
        /// <param name="resettable">
        /// Indicates if the cache should be reset by the <see cref="IEngine"/> before each execution.
        /// </param>
        /// <param name="disposeValuesOnReset">
        /// If <c>true</c> (the default) values will be disposed when the cache is reset if they implement
        /// <see cref="IDisposable"/> (only relevant if <paramref name="resettable"/> is <c>true</c>).
        /// </param>
        /// <param name="comparer">
        /// The key comparer to use, or <c>null</c> to use the default comparer.
        /// </param>
        public ConcurrentCache(bool resettable, bool disposeValuesOnReset, IEqualityComparer<TKey> comparer)
        {
            if (resettable)
            {
                IConcurrentCache.AddResettableCache(this);
            }
            _disposeValuesOnReset = disposeValuesOnReset;
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>(comparer);
        }

        private static Lazy<TValue> GetValue(Func<TValue> valueFactory) =>
            new Lazy<TValue>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<TValue> GetValue(TValue value) => new Lazy<TValue>(value);

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) =>
            _dictionary
            .GetOrAdd(key, (k, args) => GetValue(() => args(k)), valueFactory)
            .Value;

        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg arg) =>
            _dictionary
                .GetOrAdd(key, (k, args) => GetValue(() => args.valueFactory(k, args.arg)), (valueFactory, arg))
                .Value;

        public TValue GetOrAdd(TKey key, TValue value) =>
            _dictionary.GetOrAdd(key, GetValue(value)).Value;

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) =>
            _dictionary
            .AddOrUpdate(
                key,
                (k, args) => GetValue(() => args.addValueFactory(k)),
                (k, v, args) => GetValue(() => args.updateValueFactory(k, v.Value)),
                (addValueFactory, updateValueFactory))
            .Value;

        public TValue AddOrUpdate<TArg>(
            TKey key,
            Func<TKey, TArg, TValue> addValueFactory,
            Func<TKey, TValue, TArg, TValue> updateValueFactory,
            TArg arg) =>
            _dictionary
                .AddOrUpdate(
                    key,
                    (k, args) => GetValue(() => args.addValueFactory(k, args.arg)),
                    (k, v, args) => GetValue(() => args.updateValueFactory(k, v.Value, args.arg)),
                    (addValueFactory, updateValueFactory, arg))
                .Value;

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) =>
            _dictionary
                .AddOrUpdate(
                    key,
                    GetValue(addValue),
                    (k, v) => GetValue(() => updateValueFactory(k, v.Value)))
                .Value;

        public bool TryAdd(TKey key, Func<TValue> valueFactory) => _dictionary.TryAdd(key, GetValue(valueFactory));

        public bool TryAdd(TKey key, TValue value) => _dictionary.TryAdd(key, GetValue(value));

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_dictionary.TryRemove(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            // Dispose any values that need disposing
            if (_disposeValuesOnReset)
            {
                foreach (KeyValuePair<TKey, Lazy<TValue>> item in _dictionary)
                {
                    if (item.Value.IsValueCreated && item.Value.Value is IDisposable disposableValue)
                    {
                        disposableValue.Dispose();
                    }
                }
            }

            _dictionary.Clear();
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key].Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values.Select(x => x.Value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}