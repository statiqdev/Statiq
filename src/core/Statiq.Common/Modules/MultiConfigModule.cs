using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on multiple config values and could apply to input documents (or not) depending
    /// on whether the config delegates require them.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public abstract class MultiConfigModule : MultiConfigModuleBase
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="configs">
        /// The delegates to use for getting a config value.
        /// </param>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="MultiConfigModuleBase.ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="MultiConfigModuleBase.ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected MultiConfigModule(IEnumerable<KeyValuePair<string, IConfig>> configs, bool forceDocumentExecution)
            : base(configs, forceDocumentExecution)
        {
        }

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="MultiConfigModuleBase.ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="MultiConfigModuleBase.ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected MultiConfigModule(bool forceDocumentExecution)
            : base(forceDocumentExecution)
        {
        }

        /// <inheritdoc />
        protected sealed override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            if (ForceDocumentExecution || Configs.Any(x => x.Value.RequiresDocument))
            {
                // Only need to evaluate the context config delegates once
                ImmutableDictionary<string, object>.Builder configValuesBuilder = ImmutableDictionary.CreateBuilder<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, IConfig> config in Configs.Where(x => !x.Value.RequiresDocument))
                {
                    configValuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
                }
                ImmutableDictionary<string, object> configValues = configValuesBuilder.ToImmutable();

                // Iterate the inputs
                IEnumerable<IDocument> aggregateResults = null;
                foreach (IDocument input in context.Inputs)
                {
                    // If the config requires a document, evaluate it each time
                    ImmutableDictionary<string, object>.Builder valuesBuilder = configValues.ToBuilder();
                    foreach (KeyValuePair<string, IConfig> config in Configs.Where(x => x.Value.RequiresDocument))
                    {
                        valuesBuilder[config.Key] = await config.Value.GetValueAsync(input, context);
                    }

                    // Get the results for this input document
                    IMetadata values = new ReadOnlyConvertingDictionary(valuesBuilder.ToImmutable());
                    IEnumerable<IDocument> results = await ExecuteInputFuncAsync(input, context, (i, c) => ExecuteConfigAsync(i, c, values));
                    if (results is object)
                    {
                        aggregateResults = aggregateResults?.Concat(results) ?? results;
                    }
                }
                return aggregateResults;
            }
            else
            {
                // Only context configs
                ImmutableDictionary<string, object>.Builder valuesBuilder = ImmutableDictionary.CreateBuilder<string, object>();
                foreach (KeyValuePair<string, IConfig> config in Configs)
                {
                    valuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
                }
                IMetadata values = new ReadOnlyConvertingDictionary(valuesBuilder.ToImmutable());
                return await ExecuteConfigAsync(null, context, values);
            }
        }
    }
}