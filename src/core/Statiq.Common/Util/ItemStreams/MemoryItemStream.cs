using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking stream produced by iterating over a collection of byte arrays.
    /// </summary>
    public class MemoryItemStream : ItemStream<byte[]>
    {
        public MemoryItemStream(IEnumerable<byte[]> items)
            : base(items)
        {
        }

        protected sealed override ReadOnlySpan<byte> GetItemBytes(byte[] item) => item;
    }
}
