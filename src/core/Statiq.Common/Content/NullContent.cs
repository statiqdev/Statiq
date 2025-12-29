using System;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A special <see cref="IContentProvider"/> that you can use to indicate
    /// that a null content provider should be used instead of the existing
    /// content provider when cloning documents (because otherwise if <c>null</c>
    /// is passed in as the content provider the one from the existing document
    /// will be used in the cloned document).
    /// </summary>
    public sealed class NullContent : IContentProvider
    {
        public NullContent()
            : this(null)
        {
        }

        public NullContent(string mediaType)
        {
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream() => Stream.Null;

        /// <inheritdoc />
        public TextReader GetTextReader() => TextReader.Null;

        /// <inheritdoc />
        public long GetLength() => 0;

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            string.Equals(MediaType, mediaType, StringComparison.OrdinalIgnoreCase) ? this : new NullContent(mediaType);

        /// <inheritdoc />
        public Task<int> GetCacheCodeAsync() => Task.FromResult(0);
    }
}
