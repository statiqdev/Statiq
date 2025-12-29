using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for synchronous modules.
    /// </summary>
    /// <remarks>
    /// Documents can either be processed one at a time by overriding
    /// <see cref="ExecuteInput(IDocument, IExecutionContext)"/> or all
    /// at once by overriding <see cref="ExecuteContext(IExecutionContext)"/>.
    /// </remarks>
    public abstract class SyncModule : Module
    {
        /// <inheritdoc />
        protected override sealed Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            Task.FromResult(ExecuteContext(context));

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        /// <summary>
        /// Executes the module once for all input documents.
        /// </summary>
        /// <remarks>
        /// Override this method to execute the module once for all input documents. The default behavior
        /// calls <see cref="ExecuteInput(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="ExecuteInput(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual IEnumerable<IDocument> ExecuteContext(IExecutionContext context) =>
            context.Inputs
                .Select(input => ExecuteInputFunc(input, context, ExecuteInput))
                .SelectMany(x => x);

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteContext(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context) => null;
    }
}
