using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IDictionaryExtensions
    {
        public static void AddOrReplaceRange<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            if (items is object)
            {
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    dictionary[item.Key] = item.Value;
                }
            }
        }

        public static void TryAddRange<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            if (items is object)
            {
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    dictionary.TryAdd(item.Key, item.Value);
                }
            }
        }

        public static bool TryAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> getValue)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, getValue(key));
                return true;
            }

            return false;
        }

        public static bool TryAdd<TKey, TArg, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TArg arg,
            Func<TKey, TArg, TValue> getValue)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, getValue(key, arg));
                return true;
            }

            return false;
        }
    }
}