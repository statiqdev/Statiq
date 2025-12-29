using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Tables
{
    /// <summary>
    /// Converts CSV content to HTML tables.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as CSV and outputs an HTML <c>&lt;table&gt;</c> tag
    /// containing the CSV content. No <c>&lt;html&gt;</c> or <c>&lt;body&gt;</c> tags are output. The input CSV
    /// content must use <c>,</c> as separator and enclose every value in <c>"</c>.
    /// </remarks>
    /// <category name="Content" />
    public class RenderCsvAsHtml : ParallelSyncModule
    {
        private bool _firstLineHeader = false;

        /// <summary>
        /// Treats the first line of input content as a header and generates <c>&lt;th&gt;</c> tags in the output table.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public RenderCsvAsHtml WithHeader()
        {
            _firstLineHeader = true;
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            IEnumerable<IEnumerable<string>> records;
            using (Stream stream = input.GetContentStream())
            {
                records = CsvHelper.GetTable(stream);
            }

            StringBuilder builder = new StringBuilder();
            bool firstLine = true;
            builder.AppendLine("<table>");
            foreach (IEnumerable<string> row in records)
            {
                builder.AppendLine("<tr>");
                foreach (string cell in row)
                {
                    if (_firstLineHeader && firstLine)
                    {
                        builder.AppendLine($"<th>{cell}</th>");
                    }
                    else
                    {
                        builder.AppendLine($"<td>{cell}</td>");
                    }
                }
                builder.AppendLine("</tr>");
                firstLine = false;
            }
            builder.Append("</table>");
            return input.Clone(context.GetContentProvider(builder.ToString(), MediaTypes.Html)).Yield();
        }
    }
}