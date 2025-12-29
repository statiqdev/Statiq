using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IEqualityComparerExtensions
    {
        public static ConvertingEqualityComparer<T> ToConvertingEqualityComparer<T>(this IEqualityComparer<T> comparer) =>
            new ConvertingEqualityComparer<T>(comparer);
    }
}
