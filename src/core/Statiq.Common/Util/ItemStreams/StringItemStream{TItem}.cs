using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking string-based stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public abstract class StringItemStream<TItem> : ItemStream<TItem>
    {
        private readonly Encoding _encoding;
        private readonly Encoder _encoder;
        private byte[] _buffer = new byte[256];

        protected StringItemStream(IEnumerable<TItem> items)
            : this(items, Encoding.Default)
        {
        }

        protected StringItemStream(IEnumerable<TItem> items, Encoding encoding)
            : base(items)
        {
            _encoding = encoding.ThrowIfNull(nameof(encoding));
            _encoder = _encoding.GetEncoder();
        }

        public override void Reset()
        {
            base.Reset();
            _encoder.Reset();
        }

        protected sealed override ReadOnlySpan<byte> GetItemBytes(TItem item)
        {
            // Get the item string and encode it
            string itemString = GetItemString(item);
            if (itemString is null)
            {
                return default;
            }
            int byteCount = _encoder.GetByteCount(itemString, false);
            if (_buffer.Length < byteCount)
            {
                _buffer = new byte[byteCount];
            }
            Memory<byte> memory = new Memory<byte>(_buffer, 0, byteCount);
            _encoder.GetBytes(itemString, memory.Span, false);
            return memory.Span;
        }

        // If the encoder is null, this is the initial read so write the preamble (if there is one)
        // "Note that the GetBytes method does not prepend a BOM to a sequence of encoded bytes; supplying
        // a BOM at the beginning of an appropriate byte stream is the developer's responsibility."
        // From https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.preamble?view=net-5.0
        protected override ReadOnlySpan<byte> GetPrefix() => _encoding.GetPreamble();

        /// <summary>
        /// Gets the string for a given item.
        /// </summary>
        /// <param name="item">The item to get a string for (can be <c>default</c>/<c>null</c>).</param>
        /// <returns>A string that represents the item.</returns>
        protected abstract string GetItemString(TItem item);
    }
}
