using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Appends the specified content to the existing content of each document.
    /// </summary>
    /// <category name="Content" />
    public class AppendContent : ParallelConfigModule<string>
    {
        /// <summary>
        /// Appends the string value of the returned object to the content of each document.
        /// This allows you to specify different content to append for each document depending
        /// on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public AppendContent(Config<string> content)
            : base(content, true)
        {
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, string value)
        {
            if (input is null)
            {
                return context.CreateDocument(context.GetContentProvider(value)).Yield();
            }
            return value is null
                ? input.Yield()
                : input.Clone(context.GetContentProvider(await input.GetContentStringAsync() + value, input.ContentProvider.MediaType)).Yield();
        }
    }
}