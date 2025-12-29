using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IComparerExtensions
    {
        public static ConvertingComparer<T> ToConvertingComparer<T>(this IComparer<T> comparer) =>
            new ConvertingComparer<T>(comparer);
    }
}