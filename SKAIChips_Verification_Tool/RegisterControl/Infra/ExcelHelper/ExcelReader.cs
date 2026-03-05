using ClosedXML.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public static class ExcelReader
    {
        public static List<string> GetSheetNames(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var wb = new XLWorkbook(fs);

            var list = new List<string>();
            foreach (var ws in wb.Worksheets)
            {
                list.Add(ws.Name);
            }
            return list;
        }

        public static string[,] ReadUsedRangeAsStringArray(string filePath, string sheetName)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var wb = new XLWorkbook(fs);
            var ws = wb.Worksheet(sheetName);

            var firstCell = ws.FirstCellUsed();
            var lastCell = ws.LastCellUsed();

            if (firstCell == null || lastCell == null)
                return new string[0, 0];

            int firstRow = firstCell.Address.RowNumber;
            int firstCol = firstCell.Address.ColumnNumber;
            int lastRow = lastCell.Address.RowNumber;
            int lastCol = lastCell.Address.ColumnNumber;

            int rows = lastRow - firstRow + 1;
            int cols = lastCol - firstCol + 1;

            var result = new string[rows, cols];

            foreach (var cell in ws.Range(firstRow, firstCol, lastRow, lastCol).CellsUsed())
            {
                int r = cell.Address.RowNumber - firstRow;
                int c = cell.Address.ColumnNumber - firstCol;

                string val = cell.GetString();

                if (string.IsNullOrWhiteSpace(val))
                    result[r, c] = null;
                else
                    result[r, c] = val.Trim();
            }

            return result;
        }
    }
}