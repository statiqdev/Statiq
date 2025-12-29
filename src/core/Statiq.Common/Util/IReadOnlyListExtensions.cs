using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IReadOnlyListExtensions
    {
        public static int IndexOf<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value) =>
            IndexOf(search, value, EqualityComparer<T>.Default);

        public static int IndexOf<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value,
            EqualityComparer<T> comparer)
        {
            search.ThrowIfNull(nameof(search));
            value.ThrowIfNull(nameof(value));

            for (int i = 0; i <= search.Count - value.Count; i++)
            {
                if (ContainsImplementation(search, value, i, comparer))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool Contains<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value) =>
            Contains(search, value, 0, EqualityComparer<T>.Default);

        public static bool Contains<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value,
            EqualityComparer<T> comparer) =>
            Contains(search, value, 0, comparer);

        public static bool Contains<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value,
            int start) =>
            Contains(search, value, start, EqualityComparer<T>.Default);

        public static bool Contains<T>(
            this IReadOnlyList<T> search,
            IReadOnlyList<T> value,
            int start,
            EqualityComparer<T> comparer)
        {
            search.ThrowIfNull(nameof(search));
            value.ThrowIfNull(nameof(value));
            if (start < 0)
            {
                throw new ArgumentException("Start position must be greater than 0", nameof(start));
            }
            if (start > search.Count)
            {
                throw new ArgumentException("Start position must be less than the length of the search array", nameof(start));
            }
            return ContainsImplementation(search, value, start, comparer);
        }

        private static bool ContainsImplementation<T>(
            IReadOnlyList<T> search,
            IReadOnlyList<T> value,
            int start,
            EqualityComparer<T> comparer)
        {
            if (value.Count + start > search.Count)
            {
                return false;
            }
            for (int i = 0; i < value.Count; i++)
            {
                if (!comparer.Equals(value[i], search[i + start]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}