using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Tables
{
    /// <summary>
    /// Transforms Excel content to CSV.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as Excel OOXML and outputs CSV content.
    /// The output CSV content uses <c>,</c> as separator and encloses every value in <c>"</c>.
    /// </remarks>
    /// <category name="Content" />
    public class ConvertExcelToCsv : ParallelSyncModule
    {
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            IReadOnlyList<IReadOnlyList<string>> table;
            using (Stream stream = input.GetContentStream())
            {
                table = ExcelHelper.GetTable(stream);
            }

            using (Stream contentStream = context.GetContentStream())
            {
                CsvHelper.WriteTable(table, contentStream);
                return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Get(".csv"))).Yield();
            }
        }
    }
}