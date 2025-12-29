using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a function that produces an <see cref="IEnumerator{T}"/> and
    /// provides it as a <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class EnumerableEnumerator<TValue> : IEnumerable<TValue>
    {
        private readonly Func<IEnumerator<TValue>> _enumeratorFunc;

        public EnumerableEnumerator(Func<IEnumerator<TValue>> enumeratorFunc)
        {
            _enumeratorFunc = enumeratorFunc.ThrowIfNull(nameof(enumeratorFunc));
        }

        public IEnumerator<TValue> GetEnumerator() => _enumeratorFunc();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}