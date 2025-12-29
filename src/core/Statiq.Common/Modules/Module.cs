using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for modules.
    /// </summary>
    /// <remarks>
    /// Documents can either be processed one at a time by overriding
    /// <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/> or all
    /// at once by overriding <see cref="ExecuteContextAsync(IExecutionContext)"/>.
    /// </remarks>
    public abstract class Module : IModule
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
#pragma warning disable VSTHRD103 // Method synchronously blocks. Await BeforeExecutionAsync instead.
            // ReSharper disable once MethodHasAsyncOverload
            BeforeExecution(context);
            await BeforeExecutionAsync(context);
            try
            {
                IEnumerable<IDocument> outputs = await ExecuteContextAsync(context);
                ExecutionOutputs executionOutputs = new ExecutionOutputs(outputs);

                // ReSharper disable once MethodHasAsyncOverload
                AfterExecution(context, executionOutputs);
                await AfterExecutionAsync(context, executionOutputs);

                return executionOutputs.Outputs;
            }
            finally
            {
                // ReSharper disable once MethodHasAsyncOverload
                Finally(context);
                await FinallyAsync(context);
            }
#pragma warning restore VSTHRD103
        }

        /// <summary>
        /// Called before each module execution.
        /// </summary>
        /// <remarks>
        /// Override this method to configure module state before execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        protected virtual Task BeforeExecutionAsync(IExecutionContext context) => Task.CompletedTask;

        /// <summary>
        /// Called before each module execution.
        /// </summary>
        /// <remarks>
        /// Override this method to configure module state before execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        protected virtual void BeforeExecution(IExecutionContext context)
        {
        }

        /// <summary>
        /// Called after each module execution.
        /// </summary>
        /// <remarks>
        /// Override this method to examine or adjust the module outputs.
        /// If an exception is thrown during module execution, this method is never called.
        /// Use <see cref="FinallyAsync(IExecutionContext)"/> to clean up module state after execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <param name="outputs">
        /// The module outputs which can be modified by changing the <see cref="ExecutionOutputs.Outputs"/> property.
        /// </param>
        protected virtual Task AfterExecutionAsync(IExecutionContext context, ExecutionOutputs outputs) => Task.CompletedTask;

        /// <summary>
        /// Called after each module execution.
        /// </summary>
        /// <remarks>
        /// Override this method to examine or adjust the module outputs.
        /// If an exception is thrown during module execution, this method is never called.
        /// Use <see cref="Finally(IExecutionContext)"/> to clean up module state after execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <param name="outputs">
        /// The module outputs which can be modified by changing the <see cref="ExecutionOutputs.Outputs"/> property.
        /// </param>
        protected virtual void AfterExecution(IExecutionContext context, ExecutionOutputs outputs)
        {
        }

        /// <summary>
        /// Called after each module execution, even if an exception is thrown during execution.
        /// </summary>
        /// <remarks>
        /// Override this method to clean up module state after execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        protected virtual Task FinallyAsync(IExecutionContext context) => Task.CompletedTask;

        /// <summary>
        /// Called after each module execution, even if an exception is thrown during execution.
        /// </summary>
        /// <remarks>
        /// Override this method to clean up module state after execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        protected virtual void Finally(IExecutionContext context)
        {
        }

        /// <summary>
        /// Executes the module once for all input documents.
        /// </summary>
        /// <remarks>
        /// Override this method to execute the module once for all input documents. The default behavior
        /// calls <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            IEnumerable<IDocument> aggregateResults = null;
            foreach (IDocument input in context.Inputs)
            {
                IEnumerable<IDocument> results = await ExecuteInputFuncAsync(input, context, ExecuteInputAsync);
                if (results is object)
                {
                    aggregateResults = aggregateResults?.Concat(results) ?? results;
                }
            }
            return aggregateResults;
        }

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteContextAsync(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(null);

        /// <summary>
        /// Used by module base classes to execute an input while checking
        /// for cancellation and logging exceptions.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="executeFunc">The per-document execution function.</param>
        /// <returns>The results of the execution function.</returns>
        internal static async Task<IEnumerable<IDocument>> ExecuteInputFuncAsync(
            IDocument input,
            IExecutionContext context,
            Func<IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> executeFunc)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                return (await executeFunc(input, context)) ?? Array.Empty<IDocument>();
            }
            catch (Exception ex)
            {
                throw input.LogAndWrapException(ex);
            }
        }

        /// <summary>
        /// Used by module base classes to execute an input while checking
        /// for cancellation and logging exceptions.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="executeFunc">The per-document execution function.</param>
        /// <returns>The results of the execution function.</returns>
        internal static IEnumerable<IDocument> ExecuteInputFunc(
            IDocument input,
            IExecutionContext context,
            Func<IDocument, IExecutionContext, IEnumerable<IDocument>> executeFunc)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                return executeFunc(input, context) ?? Array.Empty<IDocument>();
            }
            catch (Exception ex)
            {
                throw input.LogAndWrapException(ex);
            }
        }
    }
}