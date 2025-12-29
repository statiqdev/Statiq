using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A module that processes documents in parallel (with the option to process sequentially).
    /// </summary>
    public abstract class ParallelSyncModule : Module, IParallelModule
    {
        /// <inheritdoc />
        public bool Parallel { get; set; } = true;

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            Task.FromResult(ExecuteInput(context));

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        private IEnumerable<IDocument> ExecuteInput(IExecutionContext context)
        {
            if (Parallel && !context.SerialExecution)
            {
                return context.Inputs
                    .AsParallel()
                    .AsOrdered()
                    .WithCancellation(context.CancellationToken)
                    .Select(input => ExecuteInputFunc(input, context, ExecuteInput))
                    .SelectMany(x => x);
            }

            return context.Inputs
                .Select(input => ExecuteInputFunc(input, context, ExecuteInput))
                .SelectMany(x => x);
        }

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteInput(IExecutionContext)"/>
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
