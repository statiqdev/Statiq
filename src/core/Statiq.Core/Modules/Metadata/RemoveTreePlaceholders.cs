using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Removes tree placeholder documents (this module will not flatten a tree).
    /// </summary>
    /// <metadata cref="Keys.TreePlaceholder" usage="Input"/>
    /// <category name="Metadata" />
    public class RemoveTreePlaceholders : SyncModule
    {
        private readonly string _treePlaceholderKey;

        /// <summary>
        /// Creates a new module that removes documents with the <see cref="Keys.TreePlaceholder"/> key.
        /// </summary>
        public RemoveTreePlaceholders()
        {
            _treePlaceholderKey = Keys.TreePlaceholder;
        }

        /// <summary>
        /// Creates a new module that removes documents with the specified tree placeholder key.
        /// </summary>
        public RemoveTreePlaceholders(string treePlaceholderKey)
        {
            _treePlaceholderKey = treePlaceholderKey;
        }

        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context) =>
            context.Inputs.RemoveTreePlaceholders(_treePlaceholderKey);
    }
}