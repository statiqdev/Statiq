using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public class DelegateItemStream<TItem> : ItemStream<TItem>
    {
        private readonly Func<TItem, ReadOnlyMemory<byte>> _getBytes;

        public DelegateItemStream(IEnumerable<TItem> items, Func<TItem, ReadOnlyMemory<byte>> getBytes)
            : base(items)
        {
            _getBytes = getBytes.ThrowIfNull(nameof(getBytes));
        }

        protected override ReadOnlySpan<byte> GetItemBytes(TItem item) => _getBytes(item).Span;
    }
}
