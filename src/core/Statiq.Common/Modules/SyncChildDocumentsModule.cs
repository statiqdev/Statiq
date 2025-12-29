using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An abstract base for modules that execute children and then combine the results with the
    /// input documents in some way.
    /// </summary>
    /// <remarks>
    /// The child modules are executed once and the original input documents
    /// are passed to the child modules. Wrap the child modules with a <c>ForEachDocument</c>
    /// module to execute the child modules for each input document individually. Add a
    /// <c>ClearDocuments</c> module as the first child if the original input documents
    /// should not be passed to the child modules.
    /// </remarks>
    public abstract class SyncChildDocumentsModule : ChildDocumentsModule
    {
        /// <summary>
        /// Executes the specified modules to get result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        protected SyncChildDocumentsModule(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteChildrenAsync(IExecutionContext context, ImmutableArray<IDocument> childOutputs) =>
            Task.FromResult(ExecuteChildren(context, childOutputs));

        /// <summary>
        /// Gets the output documents given the input documents and the output documents from the execution of child modules.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="childOutputs">The output documents from the child modules.</param>
        /// <returns>The output documents of this module.</returns>
        protected abstract IEnumerable<IDocument> ExecuteChildren(IExecutionContext context, ImmutableArray<IDocument> childOutputs);
    }
}
