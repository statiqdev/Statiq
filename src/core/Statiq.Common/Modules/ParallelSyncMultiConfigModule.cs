using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on multiple config values and could apply to input documents in parallel
    /// (or not) depending on whether the config delegate requires them.
    /// </summary>
    public abstract class ParallelSyncMultiConfigModule : ParallelMultiConfigModule
    {
        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="configs">
        /// The delegates to use for getting a config value.
        /// </param>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ParallelSyncMultiConfigModule(IEnumerable<KeyValuePair<string, IConfig>> configs, bool forceDocumentExecution)
            : base(configs, forceDocumentExecution)
        {
        }

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ParallelSyncMultiConfigModule(bool forceDocumentExecution)
            : base(forceDocumentExecution)
        {
        }

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values) =>
            Task.FromResult(ExecuteConfig(input, context, values));

        /// <summary>
        /// Executes the module for each input document in parallel.
        /// If there aren't any input documents and the config delegate doesn't require documents,
        /// this will be called once with a null <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        /// The input document this module is currently applying to or <c>null</c> if there aren't any
        /// input documents or if the config delegate doesn't require documents.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <param name="values">The evaluated config values.</param>
        /// <returns>The result documents.</returns>
        protected abstract IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IMetadata values);
    }
}