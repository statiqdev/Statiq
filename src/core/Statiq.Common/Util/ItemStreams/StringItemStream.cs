using System.Collections.Generic;
using System.Text;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking string-based stream produced by iterating over a collection of strings.
    /// </summary>
    public class StringItemStream : StringItemStream<string>
    {
        public StringItemStream(IEnumerable<string> items)
            : this(items, Encoding.Default)
        {
        }

        public StringItemStream(IEnumerable<string> items, Encoding encoding)
            : base(items, encoding)
        {
        }

        protected sealed override string GetItemString(string item) => item;
    }
}
