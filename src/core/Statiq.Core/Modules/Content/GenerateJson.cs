using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Converts objects stored in metadata or elsewhere to JSON.
    /// </summary>
    /// <remarks>
    /// Generates JSON for a specified object (which can come from document metadata or elsewhere)
    /// and stores it as new content for each input document or in each document's metadata.
    /// </remarks>
    /// <category name="Content" />
    public class GenerateJson : ParallelSyncConfigModule<object>
    {
        private readonly string _destinationKey;
        private bool _indenting = true;
        private bool _camelCase = false;
        private Action<JsonSerializerOptions> _options = null;

        /// <summary>
        /// The object stored in metadata at the specified key is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="sourceKey">The metadata key of the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(Config<string> sourceKey, string destinationKey = null)
            : base(Config.FromDocument(async (doc, ctx) => doc.Get(await sourceKey.GetValueAsync(doc, ctx))), true)
        {
            if (sourceKey == null)
            {
                throw new ArgumentNullException(nameof(sourceKey));
            }
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// The object returned by the specified delegate is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="data">A delegate that returns the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(Config<object> data, string destinationKey = null)
            : base(data, false)
        {
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// Allows you to specify metadata keys for each input document that should be serialized as properties in a JSON object.
        /// </summary>
        /// <param name="keys">The metadata keys to serialize as properties.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(Config<IEnumerable<string>> keys, string destinationKey = null)
            : base(Config.FromDocument(async (doc, ctx) => (await keys.GetValueAsync(doc, ctx)).Where(k => doc.ContainsKey(k)).ToDictionary(k => k, k => doc[k])), true)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// Specifies whether the generated JSON should be indented.
        /// </summary>
        /// <param name="indenting">If set to <c>true</c>, the JSON is indented.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithIndenting(bool indenting = true)
        {
            _indenting = indenting;
            return this;
        }

        /// <summary>
        /// Specifies whether the generated JSON should use a camel case naming strategy for property names.
        /// The default behavior is not to generate camel case property names.
        /// </summary>
        /// <param name="camelCase">If set to <c>true</c>, camel case property names are generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithCamelCase(bool camelCase = true)
        {
            _camelCase = camelCase;
            return this;
        }

        /// <summary>
        /// Allows changing the JSON serializer options.
        /// </summary>
        /// <param name="options">An action that manipulates the serializer options.</param>
        /// <returns>The current module instance.</returns>
        public GenerateJson WithSettings(Action<JsonSerializerOptions> options)
        {
            _options = options;
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, object value)
        {
            if (value != null)
            {
                try
                {
                    JsonSerializerOptions options = new JsonSerializerOptions()
                    {
                        WriteIndented = _indenting
                    };
                    if (_camelCase)
                    {
                        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    }
                    _options?.Invoke(options);
                    string result = JsonSerializer.Serialize(value, value.GetType(), options);
                    if (string.IsNullOrEmpty(_destinationKey))
                    {
                        return context.CloneOrCreateDocument(input, context.GetContentProvider(result, MediaTypes.Json)).Yield();
                    }
                    return context.CloneOrCreateDocument(
                        input,
                        new MetadataItems
                        {
                            { _destinationKey, result }
                        })
                        .Yield();
                }
                catch (Exception ex)
                {
                    // Return original input on exception
                    context.LogError($"Error serializing JSON for {input.ToSafeDisplayString()}, returning original input document: {ex}");
                }
            }
            return input.Yield();
        }
    }
}