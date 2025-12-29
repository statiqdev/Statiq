using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Statiq.Tables
{
    /// <summary>
    /// Metadata keys for use with tables.
    /// </summary>
    public static class TablesKeys
    {
        /// <summary>
        /// Contains the content of the table in a two-dimensional array as [rows, columns].
        /// </summary>
        /// <type cref="T:string[,]"/>
        public const string Table = nameof(Table);
    }
}