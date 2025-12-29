using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Executes the input documents against the specified child modules. This module
    /// is useful for grouping child modules into a single parent module.
    /// </summary>
    /// <category name="Control" />
    public class ForAllDocuments : ParentModule
    {
        /// <summary>
        /// Specifies the modules to execute against the input documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ForAllDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            await context.ExecuteModulesAsync(Children, context.Inputs);
    }
}