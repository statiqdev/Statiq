using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking string-based stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public class DelegateStringItemStream<TItem> : StringItemStream<TItem>
    {
        private readonly Func<TItem, string> _getString;

        public DelegateStringItemStream(IEnumerable<TItem> items, Func<TItem, string> getString)
            : base(items)
        {
            _getString = getString.ThrowIfNull(nameof(getString));
        }

        protected override string GetItemString(TItem item) => _getString(item);
    }
}
