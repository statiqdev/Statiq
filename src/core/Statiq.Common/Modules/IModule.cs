using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// The primary module interface for classes that can transform or otherwise operate on documents.
    /// </summary>
    /// <remarks>
    /// If the module implements <see cref="IDisposable"/>, <see cref="IDisposable.Dispose"/>
    /// will be called when the engine is disposed (I.e., on application exit).
    /// </remarks>
    public interface IModule
    {
        /// <summary>
        /// This should not be called directly, instead call <c>IExecutionContext.Execute()</c> if you need to execute a module from within another module.
        /// </summary>
        /// <param name="context">The execution context that includes input documents, information about the environment, and engine services.</param>
        /// <returns>A set of result documents (possibly the same as the input documents).</returns>
        // While it may seem like IAsyncEnumerable<IDocument> would be the better return type, the engine immediately iterates the return value to construct a
        // immutable array. Therefore returning an async enumerable doesn't gain any advantage because the engine blocks on enumeration as soon as the
        // module is called. Returning a IEnumerable<IDocument> as a task also helps avoid the async state machine via Task.FromResult() when the module
        // executes synchronously (as opposed to IAsyncEnumerable<IDocument> which would require converting the IEnumerable<IDocument> to an async enumerable
        // just to block while enumerating it once returned).
        Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context);
    }
}
