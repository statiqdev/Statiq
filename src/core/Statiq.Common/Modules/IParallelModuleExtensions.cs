using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    // Needs to be extensions instead of default interface members to preserve module type
    public static class IParallelModuleExtensions
    {
        /// <summary>
        /// Indicates that the module should process documents in parallel.
        /// </summary>
        /// <typeparam name="TModule">The type of module.</typeparam>
        /// <param name="module">The module that should process documents in parallel.</param>
        /// <param name="parallel"><c>true</c> to process documents in parallel, <c>false</c> otherwise.</param>
        /// <returns>The module.</returns>
        public static TModule WithParallelExecution<TModule>(this TModule module, bool parallel = true)
            where TModule : IParallelModule
        {
            module.ThrowIfNull(nameof(module)).Parallel = parallel;
            return module;
        }
    }
}
