using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Tables
{
    /// <summary>
    /// Reads the contents of Excel Open XML documents as a table into metadata.
    /// </summary>
    /// <remarks>The table array is stored in a "Table" key by default as a <c>IReadOnlyList&lt;IReadOnlyList&lt;string&gt;&gt;</c>.</remarks>
    /// <category name="Metadata" />
    public class ReadExcel : ParallelSyncMultiConfigModule
    {
        // Config keys
        private const string Key = nameof(Key);
        private const string Sheet = nameof(Sheet);

        public ReadExcel()
            : this(TablesKeys.Table, 0)
        {
        }

        public ReadExcel(Config<string> key, Config<int> sheet)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Key, key },
                    { Sheet, sheet }
                },
                true)
        {
            key.ThrowIfNull(nameof(key));
            sheet.ThrowIfNull(nameof(sheet));
        }

        /// <summary>
        /// Sets the metadata key where the table will be stored (default of "Table").
        /// </summary>
        /// <param name="key">The metadata key to store the table in.</param>
        /// <returns>The current module instance.</returns>
        public ReadExcel WithKey(Config<string> key) => (ReadExcel)SetConfig(Key, key);

        /// <summary>
        /// Sets the 0-based index of the sheet to read in the Excel file.
        /// </summary>
        /// <param name="sheet">The 0-based index of the sheet to read.</param>
        /// <returns>The current module instance.</returns>
        public ReadExcel WithSheet(Config<int> sheet) => (ReadExcel)SetConfig(Sheet, sheet);

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get params
            string key = values.GetString(Key, TablesKeys.Table).ThrowIfNullOrWhiteSpace(nameof(Key));
            int sheet = values.GetInt(Sheet);
            if (sheet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Sheet));
            }

            // Read the table
            IReadOnlyList<IReadOnlyList<string>> table;
            using (Stream stream = input.GetContentStream())
            {
                table = ExcelHelper.GetTable(stream, sheet);
            }
            return input.Clone(new MetadataDictionary
            {
                { key, table }
            }).Yield();
        }
    }
}