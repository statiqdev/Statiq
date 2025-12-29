using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Combines all of the input documents into a single output document.
    /// </summary>
    /// <remarks>
    /// The first input document serves as the basis for the combine operation. The content of every
    /// following input document is appended to the existing combined content, and the metadata of
    /// every following document replaces that of the previous documents (any metadata for which
    /// keys don't exist in the following documents is retained). A single output document with
    /// the combined content and metadata is output.
    /// </remarks>
    /// <category name="Control" />
    public class CombineDocuments : Module
    {
        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            IDocument result = null;
            foreach (IDocument input in context.Inputs)
            {
                result = result is null
                    ? input
                    : result.Clone(
                        input,
                        context.GetContentProvider(
                            await result.GetContentStringAsync() + await input.GetContentStringAsync(),
                            result.ContentProvider.MediaType is null
                                ? null
                                : (result.MediaTypeEquals(input.ContentProvider.MediaType) ? result.ContentProvider.MediaType : null)));
            }
            return result.Yield();
        }
    }
}