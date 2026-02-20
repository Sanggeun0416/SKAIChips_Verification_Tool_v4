using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    internal static class ComActiveObject
    {
        [DllImport("oleaut32.dll", PreserveSig = true)]
        private static extern int GetActiveObject(ref Guid rclsid, IntPtr reserved,
            [MarshalAs(UnmanagedType.Interface)] out object ppunk);

        public static T TryGet<T>(string progId) where T : class
        {
            var type = Type.GetTypeFromProgID(progId, throwOnError: false);
            if (type == null)
                return null;

            var clsid = type.GUID;
            var hr = GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
            if (hr != 0)
                return null;

            return obj as T;
        }
    }

    public sealed class ExcelWriter : IDisposable
    {
        private Excel.Application? _app;
        private Excel.Workbook? _wb;
        private bool _ownsApp;

        public bool IsOpened => _app != null && _wb != null;

        public void OpenOrAttach(string workbookPath, bool visible = true, bool readOnly = false, bool createIfMissing = true)
        {
            workbookPath = Path.GetFullPath(workbookPath);

            _app ??= ComActiveObject.TryGet<Excel.Application>("Excel.Application") ?? new Excel.Application();
            _app.Visible = visible;

            if (_wb != null)
            {
                try
                {
                    if (string.Equals(Path.GetFullPath(_wb.FullName), workbookPath, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                catch { }
            }

            Excel.Workbook found = null;

            foreach (Excel.Workbook wb in _app.Workbooks)
            {
                try
                {
                    if (string.Equals(Path.GetFullPath(wb.FullName), workbookPath, StringComparison.OrdinalIgnoreCase))
                    {
                        found = wb;
                        break;
                    }
                }
                catch
                {
                    ReleaseCom(wb);
                }
            }

            if (found != null)
            {
                _wb = found;
                return;
            }

            if (!File.Exists(workbookPath))
            {
                if (!createIfMissing)
                    throw new FileNotFoundException("Workbook not found.", workbookPath);

                var dir = Path.GetDirectoryName(workbookPath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                var newWb = _app.Workbooks.Add();
                newWb.SaveAs(workbookPath);
                _wb = newWb;
                return;
            }

            _wb = _app.Workbooks.Open(workbookPath, ReadOnly: readOnly);
        }

        // [ExcelWriter.cs] 파일의 ExcelWriter 클래스 내부에 추가

        public void Close()
        {
            // 1. 워크북 닫기
            if (_wb != null)
            {
                try
                {
                    _wb.Saved = true; // 저장 여부 묻지 않음 (변경사항 무시)
                    _wb.Close(false); // 닫기
                }
                catch { }
                ReleaseCom(_wb);
                _wb = null;
            }

            // 2. 엑셀 애플리케이션 종료
            if (_app != null)
            {
                try
                {
                    _app.Quit(); // 엑셀 프로세스 종료
                }
                catch { }
                ReleaseCom(_app);
                _app = null;
            }

            // 3. 나머지 리소스 정리 (Dispose 로직과 중복 방지)
            Dispose();
        }

        public List<string> GetSheetNames()
        {
            EnsureOpened();

            var list = new List<string>();
            foreach (Excel.Worksheet ws in _wb.Worksheets)
            {
                try
                {
                    list.Add(ws.Name);
                }
                finally { ReleaseCom(ws); }
            }
            return list;
        }

        public string AddSheet(string desiredName)
        {
            EnsureOpened();

            var safe = MakeSafeSheetName(desiredName);
            var unique = MakeUniqueSheetName(safe);

            Excel.Worksheet ws = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets.Add(After: _wb.Worksheets[_wb.Worksheets.Count]);
                ws.Name = unique;
                ws.Activate();
                return ws.Name;
            }
            finally
            {
                ReleaseCom(ws);
            }
        }

        public void DeleteSheet(string sheetName)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                ws.Delete();
            }
            finally
            {
                ReleaseCom(ws);
            }
        }

        public string SelectSheet(string sheetName)
        {
            EnsureOpened();

            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException("sheetName is null or empty.", nameof(sheetName));

            foreach (Excel.Worksheet ws in _wb.Worksheets)
            {
                try
                {
                    if (string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                    {
                        ws.Activate();
                        return ws.Name;
                    }
                }
                finally
                {
                    ReleaseCom(ws);
                }
            }

            throw new InvalidOperationException($"Worksheet not found: '{sheetName}'");
        }

        public object ReadCell(string sheetName, int row, int col)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range cell = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                cell = (Excel.Range)ws.Cells[row, col];
                return cell.Value2;
            }
            finally
            {
                ReleaseCom(cell);
                ReleaseCom(ws);
            }
        }

        public object[,] ReadRangeValue2(string sheetName, int row1, int col1, int row2, int col2)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];

                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                var v = rng.Value2;
                if (v is object[,] m)
                    return m;

                var single = new object[1, 1];
                single[0, 0] = v;
                return single;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public object[,] ReadUsedRangeValue2(string sheetName)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range firstRowCell = null;
            Excel.Range firstColCell = null;
            Excel.Range lastRowCell = null;
            Excel.Range lastColCell = null;

            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];

                firstRowCell = ws.Cells.Find(
                    What: "*",
                    LookIn: Excel.XlFindLookIn.xlFormulas,
                    LookAt: Excel.XlLookAt.xlPart,
                    SearchOrder: Excel.XlSearchOrder.xlByRows,
                    SearchDirection: Excel.XlSearchDirection.xlNext,
                    MatchCase: false);

                firstColCell = ws.Cells.Find(
                    What: "*",
                    LookIn: Excel.XlFindLookIn.xlFormulas,
                    LookAt: Excel.XlLookAt.xlPart,
                    SearchOrder: Excel.XlSearchOrder.xlByColumns,
                    SearchDirection: Excel.XlSearchDirection.xlNext,
                    MatchCase: false);

                lastRowCell = ws.Cells.Find(
                    What: "*",
                    LookIn: Excel.XlFindLookIn.xlFormulas,
                    LookAt: Excel.XlLookAt.xlPart,
                    SearchOrder: Excel.XlSearchOrder.xlByRows,
                    SearchDirection: Excel.XlSearchDirection.xlPrevious,
                    MatchCase: false);

                lastColCell = ws.Cells.Find(
                    What: "*",
                    LookIn: Excel.XlFindLookIn.xlFormulas,
                    LookAt: Excel.XlLookAt.xlPart,
                    SearchOrder: Excel.XlSearchOrder.xlByColumns,
                    SearchDirection: Excel.XlSearchDirection.xlPrevious,
                    MatchCase: false);

                if (firstRowCell == null || firstColCell == null || lastRowCell == null || lastColCell == null)
                    return new object[0, 0];

                int firstRow = firstRowCell.Row;
                int firstCol = firstColCell.Column;
                int lastRow = lastRowCell.Row;
                int lastCol = lastColCell.Column;

                start = (Excel.Range)ws.Cells[firstRow, firstCol];
                end = (Excel.Range)ws.Cells[lastRow, lastCol];
                rng = ws.Range[start, end];

                var v = rng.Value2;
                if (v is object[,] m)
                    return m;

                var single = new object[1, 1];
                single[0, 0] = v;
                return single;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);

                ReleaseCom(lastColCell);
                ReleaseCom(lastRowCell);
                ReleaseCom(firstColCell);
                ReleaseCom(firstRowCell);

                ReleaseCom(ws);
            }
        }

        public void WriteCell(string sheetName, int row, int col, object value)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range cell = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                cell = (Excel.Range)ws.Cells[row, col];
                cell.Value2 = value;
            }
            finally
            {
                ReleaseCom(cell);
                ReleaseCom(ws);
            }
        }

        public void WriteRange(string sheetName, int startRow, int startCol, object[,] values)
        {
            EnsureOpened();

            int rows = values.GetLength(0);
            int cols = values.GetLength(1);

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];

                start = (Excel.Range)ws.Cells[startRow, startCol];
                end = (Excel.Range)ws.Cells[startRow + rows - 1, startCol + cols - 1];
                rng = ws.Range[start, end];

                rng.Value2 = values;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void Focus(string sheetName, int row, int col)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range cell = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                cell = (Excel.Range)ws.Cells[row, col];

                // Application.Goto 메서드는 해당 Range를 선택하고 화면을 스크롤합니다.
                // Scroll: true -> 해당 셀이 좌측 상단에 오도록 스크롤
                _app.Goto(cell, false);
            }
            catch
            {
            }
            finally
            {
                ReleaseCom(cell);
                ReleaseCom(ws);
            }
        }

        public void Merge(string sheetName, int row1, int col1, int row2, int col2)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];
                rng.Merge();
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void SetFont(string sheetName, int row1, int col1, int row2, int col2, string fontName = null, double? fontSize = null, bool? bold = null)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                if (!string.IsNullOrWhiteSpace(fontName))
                    rng.Font.Name = fontName;

                if (fontSize.HasValue)
                    rng.Font.Size = fontSize.Value;

                if (bold.HasValue)
                    rng.Font.Bold = bold.Value;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void SetSheetFont(string sheetName, string fontName = null, double? fontSize = null, bool? bold = null)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range rng = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                rng = ws.Cells; // 시트 전체

                if (!string.IsNullOrWhiteSpace(fontName))
                    rng.Font.Name = fontName;

                if (fontSize.HasValue)
                    rng.Font.Size = fontSize.Value;

                if (bold.HasValue)
                    rng.Font.Bold = bold.Value;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(ws);
            }
        }

        public void SetNumberFormat(string sheetName, int row1, int col1, int row2, int col2, string format)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                rng.NumberFormat = format;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void SetAlignment(string sheetName, int row1, int col1, int row2, int col2,
            Excel.XlHAlign hAlign = Excel.XlHAlign.xlHAlignCenter,
            Excel.XlVAlign vAlign = Excel.XlVAlign.xlVAlignCenter,
            bool wrapText = false)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                rng.HorizontalAlignment = hAlign;
                rng.VerticalAlignment = vAlign;
                rng.WrapText = wrapText;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void SetAlignmentAll(string sheetName,
            Excel.XlHAlign hAlign = Excel.XlHAlign.xlHAlignCenter,
            Excel.XlVAlign vAlign = Excel.XlVAlign.xlVAlignCenter,
            bool wrapText = false)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range allCells = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                allCells = ws.Cells;

                allCells.HorizontalAlignment = hAlign;
                allCells.VerticalAlignment = vAlign;
                allCells.WrapText = wrapText;
            }
            finally
            {
                ReleaseCom(allCells);
                ReleaseCom(ws);
            }
        }

        public void SetFillColor(string sheetName, int row1, int col1, int row2, int col2, Color color)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                rng.Interior.Color = ColorTranslator.ToOle(color);
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void SetBorderAll(string sheetName, int row1, int col1, int row2, int col2,
            Excel.XlLineStyle lineStyle = Excel.XlLineStyle.xlContinuous,
            Excel.XlBorderWeight weight = Excel.XlBorderWeight.xlThin)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range start = null;
            Excel.Range end = null;
            Excel.Range rng = null;

            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                start = (Excel.Range)ws.Cells[row1, col1];
                end = (Excel.Range)ws.Cells[row2, col2];
                rng = ws.Range[start, end];

                rng.Borders.LineStyle = lineStyle;
                rng.Borders.Weight = weight;
            }
            finally
            {
                ReleaseCom(rng);
                ReleaseCom(end);
                ReleaseCom(start);
                ReleaseCom(ws);
            }
        }

        public void AutoFit(string sheetName, bool columns = true, bool rows = false)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                if (columns)
                    ws.Columns.AutoFit();
                if (rows)
                    ws.Rows.AutoFit();
            }
            finally
            {
                ReleaseCom(ws);
            }
        }

        public void SetColumnWidth(string sheetName, int col, double width)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range column = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                column = (Excel.Range)ws.Columns[col];
                column.ColumnWidth = width;
            }
            finally
            {
                ReleaseCom(column);
                ReleaseCom(ws);
            }
        }

        public void Save()
        {
            EnsureOpened();
            _wb.Save();
        }

        public void SaveAs(string fullPath)
        {
            EnsureOpened();
            fullPath = Path.GetFullPath(fullPath);

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            _wb.SaveAs(fullPath);
        }

        public void SetPerformanceMode(bool enabled)
        {
            EnsureOpened();

            if (enabled)
            {
                _app.ScreenUpdating = false;
                _app.DisplayAlerts = false;
                _app.EnableEvents = false;
                _app.Calculation = Excel.XlCalculation.xlCalculationManual;
            }
            else
            {
                _app.Calculation = Excel.XlCalculation.xlCalculationAutomatic;
                _app.EnableEvents = true;
                _app.DisplayAlerts = true;
                _app.ScreenUpdating = true;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_wb != null)
                {
                    try
                    {
                        _wb.Save();
                    }
                    catch { }
                }
            }
            finally
            {
                if (_wb != null)
                    ReleaseCom(_wb);
                if (_app != null)
                    ReleaseCom(_app);
                _wb = null;
                _app = null;
            }
        }

        private void EnsureOpened()
        {
            if (_app == null || _wb == null)
                throw new InvalidOperationException("Excel is not opened. Call OpenOrAttach() first.");
        }

        private string MakeUniqueSheetName(string baseName)
        {
            var name = baseName;
            int n = 1;

            bool Exists(string s)
            {
                foreach (Excel.Worksheet ws in _wb.Worksheets)
                {
                    try
                    {
                        if (string.Equals(ws.Name, s, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    finally
                    {
                        ReleaseCom(ws);
                    }
                }
                return false;
            }

            while (Exists(name))
            {
                var suffix = $"_{n++}";
                var cut = Math.Min(31 - suffix.Length, baseName.Length);
                name = baseName.Substring(0, cut) + suffix;
            }

            return name;
        }

        private static string MakeSafeSheetName(string name)
        {
            var s = (name ?? "Sheet").Trim();
            foreach (var ch in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                s = s.Replace(ch, '_');
            if (s.Length > 31)
                s = s.Substring(0, 31);
            if (string.IsNullOrWhiteSpace(s))
                s = "Sheet";
            return s;
        }

        private static void ReleaseCom(object o)
        {
            if (o != null && Marshal.IsComObject(o))
                Marshal.ReleaseComObject(o);
        }
    }
}
