using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestDocument : Document<TestDocument>
    {
        // The empty constructor should not call the base constructor
        // Otherwise the document will be initialized after empty construction and the factory will initialize it again
        public TestDocument()
        {
            Metadata = new TestMetadata();
            ContentProvider = new NullContent();
        }

        public TestDocument(IContentProvider contentProvider)
            : this(null, null, null, null, contentProvider)
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(null, null, null, items, contentProvider)
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, IContentProvider contentProvider = null)
            : this(null, source, destination, null, contentProvider)
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(null, source, destination, items, contentProvider)
        {
        }

        public TestDocument(IReadOnlySettings settings, in NormalizedPath source, in NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(settings, source, destination, items is null ? null : new TestMetadata(items), contentProvider)
        {
        }

        public TestDocument(IReadOnlySettings settings, in NormalizedPath source, in NormalizedPath destination, IMetadata metadata, IContentProvider contentProvider = null)
            : base(settings, source, destination, metadata is TestMetadata ? metadata : new TestMetadata(metadata), contentProvider)
        {
            // All constructors lead here
        }

        // Special test constructors

        public TestDocument(string content, string mediaType = null)
            : this(GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(Stream content, string mediaType = null)
            : this(GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(source, destination, metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(source, destination, metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, string content, string mediaType = null)
            : this(source, destination, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath source, in NormalizedPath destination, Stream content, string mediaType = null)
            : this(source, destination, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath path)
            : this(GetSourcePath(path), GetDestinationPath(path))
        {
        }

        public TestDocument(in NormalizedPath path, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata)
        {
        }

        public TestDocument(in NormalizedPath path, IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath path, IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath path, IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, contentProvider)
        {
        }

        public TestDocument(in NormalizedPath path, string content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath path, Stream content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(in NormalizedPath path, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), contentProvider)
        {
        }

        // Constructor helpers

        private static IContentProvider GetContentProvider(string content, string mediaType)
        {
            return content is null ? (IContentProvider)new NullContent() : new MemoryContent(Encoding.UTF8.GetBytes(content), mediaType);
        }

        private static IContentProvider GetContentProvider(Stream stream, string mediaType)
        {
            if (stream is null)
            {
                return new NullContent();
            }
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            byte[] buffer = new byte[stream.Length];
            using (MemoryStream bufferStream = new MemoryStream(buffer))
            {
                stream.CopyTo(bufferStream);
            }
            return new MemoryContent(buffer, mediaType);
        }

        private static NormalizedPath GetSourcePath(in NormalizedPath path) =>
            path.IsNull ? path : path.IsAbsolute ? path : new NormalizedPath("/input").Combine(path);

        private static NormalizedPath GetDestinationPath(NormalizedPath path)
        {
            if (path.IsNull || path.IsRelative)
            {
                return path;
            }
            path = new NormalizedPath("/input").GetRelativePath(path);
            return path.IsRelative ? path : NormalizedPath.Null;
        }

        // Test helpers

#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
        [PropertyMetadata(null)]
        public string Content => this.GetContentStringAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        [PropertyMetadata(null)]
        public TestMetadata TestMetadata => (TestMetadata)Metadata;

        public void Add(in KeyValuePair<string, object> item) => TestMetadata.Add(item);

        public void Add(string key, object value) => TestMetadata.Add(key, value);
    }
}