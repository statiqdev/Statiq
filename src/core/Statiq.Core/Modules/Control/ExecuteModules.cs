using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Executes modules and outputs the original input documents.
    /// </summary>
    /// <category name="Control" />
    public class ExecuteModules : SyncChildDocumentsModule
    {
        public ExecuteModules()
            : base(Array.Empty<IModule>())
        {
        }

        public ExecuteModules(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> ExecuteChildren(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            context.Inputs;
    }
}