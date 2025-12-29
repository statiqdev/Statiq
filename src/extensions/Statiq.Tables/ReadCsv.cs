using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Tables
{
    /// <summary>
    /// Reads the contents of CSV documents as a table into metadata.
    /// </summary>
    /// <remarks>The table array is stored in a "Table" key by default as a <c>IReadOnlyList&lt;IReadOnlyList&lt;string&gt;&gt;</c>.</remarks>
    /// <category name="Metadata" />
    public class ReadCsv : ParallelSyncMultiConfigModule
    {
        // Config keys
        private const string Key = nameof(Key);
        private const string Delimiter = nameof(Delimiter);

        public ReadCsv()
            : this(TablesKeys.Table)
        {
        }

        public ReadCsv(Config<string> key)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Key, key }
                },
                true)
        {
            key.ThrowIfNull(nameof(key));
        }

        /// <summary>
        /// Sets the metadata key where the table will be stored (default of "Table").
        /// </summary>
        /// <param name="key">The metadata key to store the table in.</param>
        /// <returns>The current module instance.</returns>
        public ReadCsv WithKey(Config<string> key) => (ReadCsv)SetConfig(Key, key);

        /// <summary>
        /// Sets the CSV delimiter to use.
        /// </summary>
        /// <param name="delimiter">The delimiter that separates values in the CSV data.</param>
        /// <returns>The current module instance.</returns>
        public ReadCsv WithDelimiter(Config<string> delimiter) => (ReadCsv)SetConfig(Delimiter, delimiter);

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get params
            string key = values.GetString(Key, TablesKeys.Table).ThrowIfNullOrWhiteSpace(nameof(Key));
            string delimiter = values.GetString(Delimiter);

            // Read the table
            IReadOnlyList<IReadOnlyList<string>> table;
            using (Stream stream = input.GetContentStream())
            {
                table = CsvHelper.GetTable(stream, delimiter);
            }
            return input.Clone(new MetadataDictionary
            {
                { key, table }
            }).Yield();
        }
    }
}