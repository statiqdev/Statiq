using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Statiq.Common;

namespace Statiq.Core
{
    public class ParseJson : ParallelModule
    {
        private readonly bool _populateDocument;
        private readonly string _key;
        private JsonSerializerOptions _options;

        public ParseJson()
            : this(null, true)
        {
        }

        public ParseJson(string key, bool populateDocument = false)
        {
            _key = key;
            _populateDocument = populateDocument;

            // Set default serializer options with the object-to-primitive converter
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new MetadataDictionaryJsonConverter());
        }

        public ParseJson WithOptions(JsonSerializerOptions options)
        {
            _options = options.ThrowIfNull(nameof(options));
            return this;
        }

        public ParseJson WithOptions(Action<JsonSerializerOptions> modifyOptions)
        {
            modifyOptions?.Invoke(_options);
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            MetadataDictionary metadata;
            using (Stream contentStream = input.GetContentStream())
            {
                metadata = await JsonSerializer.DeserializeAsync<MetadataDictionary>(contentStream, _options, context.CancellationToken);
            }
            if (!string.IsNullOrEmpty(_key))
            {
                input = input.Clone(new[] { new KeyValuePair<string, object>(_key, metadata) });
            }
            return _populateDocument ? input.Clone(metadata).Yield() : input.Yield();
        }
    }
}