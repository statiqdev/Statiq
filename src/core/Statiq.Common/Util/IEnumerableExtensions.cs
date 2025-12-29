using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Determines whether the items starts with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="source">The items.</param>
        /// <param name="values">The values.</param>
        /// <returns><c>true</c> if the items starts with all the specified values, <c>false</c> otherwise.</returns>
        public static bool StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> values) =>
            source.StartsWith(values, EqualityComparer<T>.Default);

        /// <summary>
        /// Determines whether the items starts with the specified values.
        /// </summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="source">The items.</param>
        /// <param name="values">The values.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns><c>true</c> if the items starts with all the specified values, <c>false</c> otherwise.</returns>
        public static bool StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> values, IEqualityComparer<T> comparer)
        {
            source.ThrowIfNull(nameof(source));
            values.ThrowIfNull(nameof(values));
            comparer.ThrowIfNull(nameof(comparer));

            IEnumerator<T> valuesEnumerator = values.GetEnumerator();
            foreach (T item in source)
            {
                if (!valuesEnumerator.MoveNext())
                {
                    break;
                }
                if (!comparer.Equals(item, valuesEnumerator.Current))
                {
                    return false;
                }
            }
            return !valuesEnumerator.MoveNext();
        }

        public static T GetPrevious<T>(this IEnumerable<T> source, T item) =>
            source.GetPrevious(item, EqualityComparer<T>.Default);

        public static T GetPrevious<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            source.ThrowIfNull(nameof(source));
            comparer.ThrowIfNull(nameof(comparer));

            T previous = default;  // If first item, will return default
            foreach (T current in source)
            {
                if (comparer.Equals(item, current))
                {
                    return previous;
                }
                previous = current;
            }
            return default;  // Not found
        }

        public static T GetNext<T>(this IEnumerable<T> source, T item) =>
            source.GetNext(item, EqualityComparer<T>.Default);

        public static T GetNext<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            source.ThrowIfNull(nameof(source));
            comparer.ThrowIfNull(nameof(comparer));

            bool next = false;
            foreach (T current in source)
            {
                if (next)
                {
                    return current;
                }
                if (comparer.Equals(item, current))
                {
                    next = true;
                }
            }
            return default;  // Not found or last item
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item) => source.Concat(Yield(item));

        private static IEnumerable<T> Yield<T>(T item)
        {
            yield return item;
        }
    }
}
