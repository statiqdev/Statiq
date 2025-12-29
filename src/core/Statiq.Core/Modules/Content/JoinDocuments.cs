using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Joins documents together with an optional delimiter to form one document.
    /// </summary>
    /// <category name="Content" />
    public class JoinDocuments : Module
    {
        private readonly string _delimiter;
        private readonly JoinedMetadata _metaDataMode;

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter and with the default metadata only.
        /// </summary>
        public JoinDocuments()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter using the specified meta data mode.
        /// </summary>
        /// <param name="metaDataMode">The specified metadata mode.</param>
        public JoinDocuments(JoinedMetadata metaDataMode)
            : this(string.Empty, metaDataMode)
        {
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document with a specified delimiter using the specified meta data mode.
        /// </summary>
        /// <param name="delimiter">The string to use as a separator between documents.</param>
        /// <param name="metaDataMode">The specified metadata mode.</param>
        public JoinDocuments(string delimiter, JoinedMetadata metaDataMode = JoinedMetadata.DefaultOnly)
        {
            _delimiter = delimiter;
            _metaDataMode = metaDataMode;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a single document containing the concatenated content of all input documents with an optional delimiter and configurable metadata options.
        /// </summary>
        /// <returns>A single document in a list.</returns>
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            if (context.Inputs.Length < 1)
            {
                return context.CreateDocument().Yield();
            }

            using (Stream contentStream = context.GetContentStream())
            {
                bool first = true;
                byte[] delimeterBytes = Encoding.UTF8.GetBytes(_delimiter);
                byte[] firstContentByte = new byte[1];
                string mediaType = null;
                foreach (IDocument document in context.Inputs)
                {
                    if (document is null)
                    {
                        continue;
                    }

                    using (Stream inputStream = document.GetContentStream())
                    {
                        // Peek the first byte to see if this document has content
                        if (await inputStream.ReadAsync(firstContentByte, context.CancellationToken) == 1)
                        {
                            if (first)
                            {
                                first = false;
                                mediaType = document.ContentProvider.MediaType;
                            }
                            else
                            {
                                await contentStream.WriteAsync(delimeterBytes, 0, delimeterBytes.Length, context.CancellationToken);
                                if (!document.MediaTypeEquals(mediaType))
                                {
                                    mediaType = null;
                                }
                            }

                            await contentStream.WriteAsync(firstContentByte, context.CancellationToken);
                            await inputStream.CopyToAsync(contentStream, context.CancellationToken);
                        }
                    }
                }

                return context.CreateDocument(
                    GetMetadataForOutputDocument(context.Inputs),
                    context.GetContentProvider(contentStream, mediaType))
                    .Yield();
            }
        }

        /// <summary>
        /// Returns the correct metadata for the new document based on the provided list of documents and the selected metadata mode.
        /// </summary>
        /// <param name="inputs">The list of input documents.</param>
        /// <returns>The set of metadata for all input documents.</returns>
        private IEnumerable<KeyValuePair<string, object>> GetMetadataForOutputDocument(ImmutableArray<IDocument> inputs)
        {
            switch (_metaDataMode)
            {
                case JoinedMetadata.FirstDocument:
                    return inputs.First().ToList();

                case JoinedMetadata.LastDocument:
                    return inputs.Last().ToList();

                case JoinedMetadata.AllWithFirstDuplicates:
                    return inputs.SelectMany(a => a).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.First().Value).ToArray();

                case JoinedMetadata.AllWithLastDuplicates:
                    return inputs.SelectMany(a => a).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.Last().Value).ToArray();

                case JoinedMetadata.DefaultOnly:
                    return new List<KeyValuePair<string, object>>();

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(JoinedMetadata)} option was not expected.");
            }
        }
    }
}