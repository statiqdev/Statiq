using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A module that processes documents in parallel (with the option to process sequentially).
    /// </summary>
    public abstract class ParallelModule : Module, IParallelModule
    {
        /// <inheritdoc />
        public bool Parallel { get; set; } = true;

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            Parallel && !context.SerialExecution
                ? context.Inputs.ParallelSelectManyAsync(input => ExecuteInputFuncAsync(input, context, ExecuteInputAsync), context.CancellationToken)
                : base.ExecuteContextAsync(context);
    }
}
