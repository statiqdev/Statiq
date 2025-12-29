using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextExecuteModulesExtensions
    {
        /// <summary>
        /// Executes the specified modules with an single initial input document with optional additional metadata and returns the result documents.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="metadata">The metadata to use.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(
            this IExecutionContext executionContext,
            IEnumerable<IModule> modules,
            IEnumerable<KeyValuePair<string, object>> metadata) =>
            await executionContext.ExecuteModulesAsync(modules, executionContext.CreateDocument(metadata).Yield());

        /// <summary>
        /// Executes the specified modules without an initial input document and returns the result documents.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(
            this IExecutionContext executionContext,
            IEnumerable<IModule> modules) =>
            await executionContext.ExecuteModulesAsync(modules, (IDocument)null);
    }
}
