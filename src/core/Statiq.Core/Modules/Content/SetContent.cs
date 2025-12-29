using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets the content of each document.
    /// </summary>
    /// <category name="Content" />
    public class SetContent : ParallelSyncConfigModule<IContentProvider>
    {
        /// <summary>
        /// Sets the content of each document to the config value.
        /// </summary>
        /// <param name="contentProvider">A delegate that returns the content provider to set.</param>
        public SetContent(Config<IContentProvider> contentProvider)
            : base(contentProvider, true)
        {
        }

        /// <summary>
        /// Sets the content of each document to the config value.
        /// If the value is <c>null</c>, the original input document will be output
        /// (use <see cref="string.Empty"/> to clear the content).
        /// </summary>
        /// <param name="content">A delegate that returns the content to set.</param>
        public SetContent(Config<string> content)
            : this(content, Config.FromDocument(doc => doc.ContentProvider.MediaType))
        {
        }

        /// <summary>
        /// Sets the content of each document to the config value.
        /// If the value is <c>null</c>, the original input document will be output
        /// (use <see cref="string.Empty"/> to clear the content).
        /// </summary>
        /// <param name="content">A delegate that returns the content to set.</param>
        /// <param name="mediaType">The media type of the new content.</param>
        public SetContent(Config<string> content, Config<string> mediaType)
            : base(content.CombineWith(mediaType, (c, m, ctx) => ctx.GetContentProvider(c, m)), true)
        {
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IContentProvider value) =>
            value is null || value is NullContent ? input.Yield() : input.Clone(value).Yield();
    }
}