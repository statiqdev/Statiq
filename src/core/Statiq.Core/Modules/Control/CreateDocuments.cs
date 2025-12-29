using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Creates new documents.
    /// </summary>
    /// <remarks>
    /// This module does not include the input documents as part of it's output
    /// (if you need to change the content of an existing document, use <see cref="SetContent"/>).
    /// </remarks>
    /// <category name="Control" />
    public class CreateDocuments : ConfigModule<IEnumerable<IDocument>>
    {
        /// <summary>
        /// Creates a new document with the specified content.
        /// </summary>
        /// <param name="content">The content for the output document.</param>
        /// <param name="mediaType">The media type of the output document.</param>
        public CreateDocuments(Config<string> content, string mediaType = null)
            : base(
                content.Transform((content, ctx) => ctx.CreateDocument(ctx.GetContentProvider(content, mediaType)).Yield()),
                false)
        {
        }

        /// <summary>
        /// Creates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        /// <param name="mediaType">The media type of each output document.</param>
        public CreateDocuments(Config<IEnumerable<string>> content, string mediaType = null)
            : base(
                content.Transform((content, ctx) => content.Select(x => ctx.CreateDocument(ctx.GetContentProvider(x, mediaType)))),
                false)
        {
        }

        /// <summary>
        /// Creates a specified number of new empty documents.
        /// </summary>
        /// <param name="count">The number of new documents to output.</param>
        public CreateDocuments(int count)
            : base(
                Config.FromContext(ctx =>
                {
                    List<IDocument> documents = new List<IDocument>();
                    for (int c = 0; c < count; c++)
                    {
                        documents.Add(ctx.CreateDocument());
                    }
                    return (IEnumerable<IDocument>)documents;
                }),
                false)
        {
        }

        /// <summary>
        /// Creates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        public CreateDocuments(params string[] content)
            : this((IEnumerable<string>)content)
        {
        }

        /// <summary>
        /// Creates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        /// <param name="mediaType">The media type of each output document.</param>
        public CreateDocuments(IEnumerable<string> content, string mediaType = null)
            : base(
                Config.FromContext(ctx => content.Select(x => ctx.CreateDocument(ctx.GetContentProvider(x, mediaType)))),
                false)
        {
        }

        /// <summary>
        /// Creates new documents with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata for each output document.</param>
        public CreateDocuments(params IEnumerable<KeyValuePair<string, object>>[] metadata)
            : this((IEnumerable<IEnumerable<KeyValuePair<string, object>>>)metadata)
        {
        }

        /// <summary>
        /// Creates new documents with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata for each output document.</param>
        public CreateDocuments(IEnumerable<IEnumerable<KeyValuePair<string, object>>> metadata)
            : base(Config.FromContext(ctx => metadata.Select(x => ctx.CreateDocument(x))), false)
        {
        }

        /// <summary>
        /// Creates new documents with the specified content and metadata.
        /// </summary>
        /// <param name="contentAndMetadata">The content and metadata for each output document.</param>
        public CreateDocuments(params Tuple<string, IEnumerable<KeyValuePair<string, object>>>[] contentAndMetadata)
            : this((IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>>)contentAndMetadata)
        {
        }

        /// <summary>
        /// Creates new documents with the specified content and metadata.
        /// </summary>
        /// <param name="contentAndMetadata">The content and metadata for each output document.</param>
        /// <param name="mediaType">The media type of each output document.</param>
        public CreateDocuments(IEnumerable<Tuple<string, IEnumerable<KeyValuePair<string, object>>>> contentAndMetadata, string mediaType = null)
            : base(
                Config.FromContext(ctx => contentAndMetadata.Select(x => ctx.CreateDocument(x.Item2, ctx.GetContentProvider(x.Item1, mediaType)))),
                false)
        {
        }

        protected override Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<IDocument> value) => Task.FromResult(value);
    }
}