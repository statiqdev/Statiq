using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Reads text provided to the application on startup.
    /// </summary>
    /// <remarks>
    /// This modules creates a single document from text provided to Statiq on startup. In most cases, this will be text or file contents
    /// "piped" to the Statiq.exe via the command line from a file or prior chained executable. Also known as "Standard Input" or "STDIN".
    /// </remarks>
    /// <category name="Input/Output" />
    public class ReadApplicationInput : SyncModule
    {
        /// <inheritdoc />
        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            // If ApplicationInput is empty, return nothing
            if (string.IsNullOrWhiteSpace(context.ApplicationState.Input))
            {
                return Array.Empty<IDocument>();
            }

            return context.CreateDocument(
                context.GetContentProvider(context.ApplicationState.Input))
                .Yield();
        }
    }
}