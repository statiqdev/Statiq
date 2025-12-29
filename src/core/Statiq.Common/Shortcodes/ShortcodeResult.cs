using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Statiq.Common
{
    public class ShortcodeResult
    {
        // Lazily get the content provider so we can construct ShortcodeResult objects before having an
        // execution context, I.e. in the bootstrapper when defining shortcodes
        private readonly Lazy<IContentProvider> _contentProviderFactory;

        public ShortcodeResult(
            Func<IContentProvider> contentProviderFactory,
            IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
        {
            _contentProviderFactory = new Lazy<IContentProvider>(
                contentProviderFactory,
                LazyThreadSafetyMode.ExecutionAndPublication);
            NestedMetadata = nestedMetadata is null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(nestedMetadata);
        }

        public ShortcodeResult(
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
            : this(() => contentProvider, nestedMetadata)
        {
        }

        public ShortcodeResult(Stream content, IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
            : this(() => content is null ? null : IExecutionContext.Current.GetContentProvider(content), nestedMetadata)
        {
        }

        public ShortcodeResult(string content, IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
            : this(() => content is null ? null : IExecutionContext.Current.GetContentProvider(content), nestedMetadata)
        {
        }

        public IContentProvider ContentProvider => _contentProviderFactory.Value;

        public IDictionary<string, object> NestedMetadata { get; }

        public static implicit operator ShortcodeResult(Stream content) => content is null ? null : new ShortcodeResult(content);

        public static implicit operator ShortcodeResult(string content) => string.IsNullOrEmpty(content) ? null : new ShortcodeResult(content);
    }
}