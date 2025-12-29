using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IReadOnlyDictionaryExtensions
    {
        /// <summary>
        /// Verifies that a dictionary contains all requires keys.
        /// An <see cref="ArgumentException"/> will be thrown if the
        /// specified keys are not all present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        public static void RequireKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, params TKey[] keys) =>
            dictionary.RequireKeys((IEnumerable<TKey>)keys);

        /// <summary>
        /// Verifies that a dictionary contains all requires keys.
        /// An <see cref="ArgumentException"/> will be thrown if the
        /// specified keys are not all present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        public static void RequireKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            if (!dictionary.ContainsKeys(keys))
            {
                throw new ArgumentException("Dictionary does not contain all required keys");
            }
        }

        /// <summary>
        /// Determines whether the dictionary contains all the specified keys.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        /// <returns><c>true</c> if the dictionary contains all the specified keys, <c>false</c> otherwise.</returns>
        public static bool ContainsKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, params TKey[] keys) =>
            dictionary.ContainsKeys((IEnumerable<TKey>)keys);

        /// <summary>
        /// Determines whether the dictionary contains all the specified keys.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        /// <returns><c>true</c> if the dictionary contains all the specified keys, <c>false</c> otherwise.</returns>
        public static bool ContainsKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys) =>
            keys.All(x => dictionary.ContainsKey(x));

        /// <summary>
        /// Determines whether the dictionary contains all the specified keys.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        /// <returns><c>true</c> if the dictionary contains all the specified keys, <c>false</c> otherwise.</returns>
        public static bool ContainsAnyKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, params TKey[] keys) =>
            dictionary.ContainsAnyKeys((IEnumerable<TKey>)keys);

        /// <summary>
        /// Determines whether the dictionary contains all the specified keys.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        /// <returns><c>true</c> if the dictionary contains all the specified keys, <c>false</c> otherwise.</returns>
        public static bool ContainsAnyKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys) =>
            keys.Any(x => dictionary.ContainsKey(x));
    }
}
