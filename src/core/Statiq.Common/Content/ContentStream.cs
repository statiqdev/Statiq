using System;
using System.IO;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A special writable <see cref="Stream"/> that can be used when creating new content
    /// that provides an appropriate <see cref="IContentProvider"/>.
    /// Instances of this stream should be disposed when writing is complete.
    /// </summary>
    internal abstract class ContentStream : DelegatingStream, IContentProviderFactory
    {
        public ContentStream(Stream stream)
            : base(stream)
        {
        }

        public abstract IContentProvider GetContentProvider(string mediaType);

        public IContentProvider GetContentProvider() => GetContentProvider(null);
    }
}
