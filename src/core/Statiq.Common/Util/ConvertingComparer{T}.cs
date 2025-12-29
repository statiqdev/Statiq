using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Adapts a typed equality comparer to untyped metadata by attempting to convert the
    /// metadata values to the comparer type before running the comparison. If either type
    /// can not be converted to <typeparamref name="T"/>, the comparison returns the default
    /// object comparison.
    /// </summary>
    /// <typeparam name="T">The value type to convert to for comparisons.</typeparam>
    public class ConvertingComparer<T> : IComparer<object>
    {
        private readonly IComparer<T> _comparer;

        public ConvertingComparer(IComparer<T> comparer)
        {
            _comparer = comparer.ThrowIfNull(nameof(comparer));
        }

        public int Compare(object x, object y) =>
            TypeHelper.TryConvert(x, out T xValue) && TypeHelper.TryConvert(y, out T yValue)
                ? _comparer.Compare(xValue, yValue)
                : Comparer<object>.Default.Compare(x, y);
    }
}