using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    [Obsolete("Derive from MultiConfigModule or ParallelMultiConfigModule.")]
    public abstract class MultiConfigModuleBase : Module
    {
        private readonly Dictionary<string, IConfig> _configs = new Dictionary<string, IConfig>(StringComparer.OrdinalIgnoreCase);

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
        internal MultiConfigModuleBase(IEnumerable<KeyValuePair<string, IConfig>> configs, bool forceDocumentExecution)
        {
            if (configs is object)
            {
                foreach (KeyValuePair<string, IConfig> config in configs)
                {
                    SetConfig(config.Key, config.Value);
                }
            }
            ForceDocumentExecution = forceDocumentExecution;
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
        internal MultiConfigModuleBase(bool forceDocumentExecution)
            : this(null, forceDocumentExecution)
        {
        }

        public bool ForceDocumentExecution { get; }

        protected IReadOnlyDictionary<string, IConfig> Configs => _configs;

        /// <summary>
        /// Sets the config for a given key.
        /// </summary>
        /// <param name="key">The key of the config to set.</param>
        /// <param name="config">The config to set or <c>null</c> to remove the key.</param>
        /// <returns>The current instance.</returns>
        protected MultiConfigModuleBase SetConfig(string key, IConfig config)
        {
            key.ThrowIfNull(nameof(key));
            if (config is null)
            {
                _configs.Remove(key);
            }
            else
            {
                _configs[key] = config;
            }
            return this;
        }

        /// <summary>
        /// Gets the config for a given key.
        /// </summary>
        /// <param name="key">The key of the config to get.</param>
        /// <returns>The config or <c>null</c> if the key is not found.</returns>
        protected IConfig GetConfig(string key) => TryGetConfig(key, out IConfig config) ? config : null;

        /// <summary>
        /// Gets the config for a given key cast to a specific config value.
        /// </summary>
        /// <typeparam name="TValue">The value of the config.</typeparam>
        /// <param name="key">The key of the config to get.</param>
        /// <returns>The config or <c>null</c> if the key is not found.</returns>
        protected Config<TValue> GetConfig<TValue>(string key) => (Config<TValue>)GetConfig(key);

        protected bool TryGetConfig(string key, out IConfig config) => _configs.TryGetValue(key, out config);

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, TValue> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, Task<TValue>> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, IExecutionContext, TValue> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, IExecutionContext, Task<TValue>> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, IDocument, IExecutionContext, TValue> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        protected MultiConfigModuleBase CombineConfig<TValue>(string key, Config<TValue> config, Func<TValue, TValue, IDocument, IExecutionContext, Task<TValue>> combine) =>
            SetConfig(key, GetConfig<TValue>(key).CombineWith(config, combine));

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteInputAsync(input, context);

        /// <summary>
        /// Executes the module for each input document.
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
        protected abstract Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values);
    }
}