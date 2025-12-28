using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OfficeOpenXml;

namespace Statiq.Tables
{
    internal static class ExcelHelper
    {
        public static IReadOnlyList<IReadOnlyList<string>> GetTable(Stream stream, int sheetNumber = 0)
        {
            SetLicense();
            using (ExcelPackage excel = new ExcelPackage(stream))
            {
                excel.Compatibility.IsWorksheets1Based = false;
                if (sheetNumber > excel.Workbook.Worksheets.Count)
                {
                    return null;
                }

                ExcelWorksheet sheet = excel.Workbook.Worksheets[sheetNumber];

                return GetTable(sheet);
            }
        }

        public static IReadOnlyList<IReadOnlyList<string>> GetTable(ExcelWorksheet sheet)
        {
            SetLicense();
            ExcelAddressBase dimension = sheet.Dimension;

            if (dimension is null)
            {
                return null;
            }

            int rowCount = dimension.Rows;
            int columnCount = dimension.Columns;
            List<List<string>> table = new List<List<string>>(rowCount);

            for (int r = 1; r <= rowCount; r++)
            {
                List<string> rowValues = new List<string>(columnCount);
                for (int c = 1; c <= columnCount; c++)
                {
                    ExcelRangeBase cell = sheet.Cells[r, c].FirstOrDefault();
                    rowValues.Add(cell?.Value?.ToString());
                }

                table.Add(rowValues);
            }

            return table;
        }

        private static void SetLicense()
        {
            ExcelPackage.License.SetNonCommercialOrganization("StatiqDev");
        }
    }
}
