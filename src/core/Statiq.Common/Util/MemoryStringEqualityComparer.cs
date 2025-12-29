using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public class MemoryStringEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) =>
            x.Span.SequenceEqual(y.Span);

        public int GetHashCode(ReadOnlyMemory<char> obj)
        {
            HashCode hashCode = default;
            for (int i = 0; i < obj.Span.Length; i++)
            {
                hashCode.Add(obj.Span[i]);
            }
            return hashCode.ToHashCode();
        }
    }
}
