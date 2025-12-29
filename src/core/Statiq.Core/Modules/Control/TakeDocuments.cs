using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Takes the first X documents from the current pipeline and discards the rest.
    /// </summary>
    /// <category name="Control" />
    public class TakeDocuments : SyncModule
    {
        private readonly int _x;

        /// <summary>
        /// Takes the first X documents from the current pipeline and discards the rest.
        /// </summary>
        /// <param name="x">An integer representing the number of documents to preserve from the current pipeline.</param>
        public TakeDocuments(int x)
        {
            _x = x;
        }

        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context) => context.Inputs.Take(_x);
    }
}