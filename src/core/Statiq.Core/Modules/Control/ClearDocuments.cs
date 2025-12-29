using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clears all input documents and outputs an empty document collection.
    /// </summary>
    /// <remarks>
    /// This module is useful as the first of a collection of child modules if the input documents should not be passed
    /// to subsequent child modules.
    /// </remarks>
    /// <category name="Control" />
    public class ClearDocuments : Module
    {
        protected override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(null);
    }
}