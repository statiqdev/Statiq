using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Iterates a sequence from metadata, setting a specified key for each iteration.
    /// </summary>
    /// <parameter name="Key">The key that contains the sequence to iterate.</parameter>
    /// <parameter name="ValueKey">A key to set with the value for each iteration.</parameter>
    /// <parameter name="IndexKey">A key to set with the current iteration index (zero-based, optional).</parameter>
    public class ForEachShortcode : SyncMultiShortcode
    {
        private const string Key = nameof(Key);
        private const string ValueKey = nameof(ValueKey);
        private const string IndexKey = nameof(IndexKey);

        public override IEnumerable<ShortcodeResult> Execute(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context)
        {
            IMetadataDictionary dictionary = args.ToDictionary(Key, ValueKey, IndexKey);
            dictionary.RequireKeys(Key, ValueKey);
            string valueKey = dictionary.GetString(ValueKey);
            if (string.IsNullOrEmpty(valueKey))
            {
                throw new ShortcodeArgumentException(ValueKey);
            }
            string indexKey = dictionary.GetString(IndexKey);

            IReadOnlyList<object> items = document.GetList<object>(dictionary.GetString(Key));
            if (items is object)
            {
                List<ShortcodeResult> results = new List<ShortcodeResult>();
                int index = 0;
                foreach (object item in items)
                {
                    MetadataItems metadata = new MetadataItems()
                    {
                        { valueKey, item }
                    };
                    if (!string.IsNullOrEmpty(indexKey))
                    {
                        metadata.Add(indexKey, index);
                    }

                    results.Add(new ShortcodeResult(content, metadata));

                    index++;
                }

                return results;
            }

            return null;
        }
    }
}