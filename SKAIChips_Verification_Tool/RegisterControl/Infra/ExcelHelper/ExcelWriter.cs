using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 이미 실행 중인 COM 객체(예: 엑셀 애플리케이션)의 활성 인스턴스를 가져오기 위한 네이티브 API 래퍼 클래스입니다.
    /// </summary>
    internal static class ComActiveObject
    {
        [DllImport("oleaut32.dll", PreserveSig = true)]
        private static extern int GetActiveObject(ref Guid rclsid, IntPtr reserved,
            [MarshalAs(UnmanagedType.Interface)] out object ppunk);

        /// <summary>
        /// 지정된 ProgID(예: "Excel.Application")를 가진 활성화된 COM 객체 인스턴스를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">캐스팅할 COM 인터페이스 타입</typeparam>
        /// <param name="progId">COM 객체의 ProgID 문자열</param>
        /// <returns>활성화된 COM 객체 인스턴스. 실행 중인 객체가 없으면 null을 반환합니다.</returns>
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

    /// <summary>
    /// Excel Interop COM 객체를 직접 다루며 엑셀 파일의 생성, 열기, 수정, 서식 지정 및 저장을 수행하는 클래스입니다.
    /// IDisposable을 구현하여 사용이 끝난 후 COM 리소스를 안전하게 해제합니다.
    /// </summary>
    public sealed class ExcelWriter : IDisposable
    {
        private Excel.Application? _app;
        private Excel.Workbook? _wb;
        private bool _ownsApp;

        /// <summary>
        /// 엑셀 애플리케이션과 워크북이 정상적으로 열려 있는지 여부를 확인합니다.
        /// </summary>
        public bool IsOpened => _app != null && _wb != null;

        /// <summary>
        /// 지정된 경로의 엑셀 파일을 열거나, 이미 열려 있는 엑셀 프로세스에 연결합니다.
        /// 파일이 없으면 새 파일을 생성할 수도 있습니다.
        /// </summary>
        /// <param name="workbookPath">열거나 생성할 엑셀 파일의 경로</param>
        /// <param name="visible">엑셀 창을 화면에 표시할지 여부 (기본값: true)</param>
        /// <param name="readOnly">읽기 전용 모드로 열지 여부 (기본값: false)</param>
        /// <param name="createIfMissing">파일이 없을 경우 새로 생성할지 여부 (기본값: true)</param>
        /// <exception cref="FileNotFoundException">파일이 없고 createIfMissing이 false일 때 발생합니다.</exception>
        public void OpenOrAttach(string workbookPath, bool visible = true, bool readOnly = false, bool createIfMissing = true)
        {
            workbookPath = Path.GetFullPath(workbookPath);

            // 기존에 실행 중인 엑셀이 있으면 가져오고, 없으면 새로 생성합니다.
            _app ??= ComActiveObject.TryGet<Excel.Application>("Excel.Application") ?? new Excel.Application();
            _app.Visible = visible;

            // 이미 워크북이 로드되어 있고, 경로가 일치하면 그대로 사용합니다.
            if (_wb != null)
            {
                try
                {
                    if (string.Equals(Path.GetFullPath(_wb.FullName), workbookPath, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                catch { }
            }

            Excel.Workbooks workbooks = null;
            try
            {
                workbooks = _app.Workbooks;
                Excel.Workbook found = null;

                // 이미 열려 있는 워크북 목록 중 일치하는 파일이 있는지 검색합니다.
                foreach (Excel.Workbook wb in workbooks)
                {
                    try
                    {
                        if (string.Equals(Path.GetFullPath(wb.FullName), workbookPath, StringComparison.OrdinalIgnoreCase))
                        {
                            found = wb;
                            break;
                        }
                    }
                    catch { ReleaseCom(wb); }
                }

                if (found != null)
                {
                    _wb = found;
                    return;
                }

                // 파일이 존재하지 않는 경우의 처리 로직
                if (!File.Exists(workbookPath))
                {
                    if (!createIfMissing)
                        throw new FileNotFoundException("Workbook not found.", workbookPath);

                    var dir = Path.GetDirectoryName(workbookPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                        Directory.CreateDirectory(dir);

                    _wb = workbooks.Add();
                    _wb.SaveAs(workbookPath);
                    return;
                }

                // 파일이 존재하면 엽니다.
                _wb = workbooks.Open(workbookPath, ReadOnly: readOnly);
            }
            finally
            {
                ReleaseCom(workbooks);
            }
        }

        /// <summary>
        /// 열려 있는 워크북을 저장 후 닫고, 엑셀 애플리케이션 프로세스를 종료합니다.
        /// </summary>
        public void Close()
        {
            if (_wb != null)
            {
                try
                {
                    _wb.Saved = true;
                    _wb.Close(false);
                }
                catch { }
                ReleaseCom(_wb);
                _wb = null;
            }

            if (_app != null)
            {
                try
                {
                    _app.Quit();
                }
                catch { }
                ReleaseCom(_app);
                _app = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// 현재 워크북에 포함된 모든 워크시트의 이름 목록을 가져옵니다.
        /// </summary>
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

        /// <summary>
        /// 지정된 이름으로 새 워크시트를 추가합니다. 이름이 중복되거나 특수문자가 포함된 경우 안전한 이름으로 자동 변환됩니다.
        /// </summary>
        /// <param name="desiredName">원하는 워크시트 이름</param>
        /// <returns>실제로 생성 및 적용된 워크시트 이름</returns>
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

        /// <summary>
        /// 지정된 이름의 워크시트를 삭제합니다.
        /// </summary>
        /// <param name="sheetName">삭제할 워크시트 이름</param>
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

        /// <summary>
        /// 지정된 이름의 워크시트를 활성화(포커스)합니다.
        /// </summary>
        /// <param name="sheetName">선택할 워크시트 이름</param>
        /// <returns>선택된 워크시트 이름</returns>
        /// <exception cref="InvalidOperationException">해당 이름의 시트가 없을 경우 발생합니다.</exception>
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

        /// <summary>
        /// 특정 셀의 값을 읽어옵니다. (Value2 속성 사용으로 속도 최적화)
        /// </summary>
        /// <param name="sheetName">시트 이름</param>
        /// <param name="row">행 번호 (1부터 시작)</param>
        /// <param name="col">열 번호 (1부터 시작)</param>
        /// <returns>셀의 데이터 객체</returns>
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

        /// <summary>
        /// 지정된 영역의 데이터를 한 번에 2차원 배열로 읽어옵니다.
        /// </summary>
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

        /// <summary>
        /// 워크시트에서 데이터가 존재하는 모든 영역(Used Range)을 찾아 2차원 배열로 반환합니다.
        /// </summary>
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

                firstRowCell = ws.Cells.Find("*", LookIn: Excel.XlFindLookIn.xlFormulas, LookAt: Excel.XlLookAt.xlPart, SearchOrder: Excel.XlSearchOrder.xlByRows, SearchDirection: Excel.XlSearchDirection.xlNext, MatchCase: false);
                firstColCell = ws.Cells.Find("*", LookIn: Excel.XlFindLookIn.xlFormulas, LookAt: Excel.XlLookAt.xlPart, SearchOrder: Excel.XlSearchOrder.xlByColumns, SearchDirection: Excel.XlSearchDirection.xlNext, MatchCase: false);
                lastRowCell = ws.Cells.Find("*", LookIn: Excel.XlFindLookIn.xlFormulas, LookAt: Excel.XlLookAt.xlPart, SearchOrder: Excel.XlSearchOrder.xlByRows, SearchDirection: Excel.XlSearchDirection.xlPrevious, MatchCase: false);
                lastColCell = ws.Cells.Find("*", LookIn: Excel.XlFindLookIn.xlFormulas, LookAt: Excel.XlLookAt.xlPart, SearchOrder: Excel.XlSearchOrder.xlByColumns, SearchDirection: Excel.XlSearchDirection.xlPrevious, MatchCase: false);

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

        /// <summary>
        /// 특정 셀에 단일 데이터를 씁니다.
        /// </summary>
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

        /// <summary>
        /// 2차원 배열 데이터를 지정된 시작 위치부터 한 번에 입력합니다. (단일 셀 입력 반복보다 성능이 매우 뛰어납니다)
        /// </summary>
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

        /// <summary>
        /// 엑셀 UI 화면을 특정 셀 위치로 이동시킵니다.
        /// </summary>
        public void Focus(string sheetName, int row, int col)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range cell = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                cell = (Excel.Range)ws.Cells[row, col];

                _app.Goto(cell, false);
            }
            catch { }
            finally
            {
                ReleaseCom(cell);
                ReleaseCom(ws);
            }
        }

        /// <summary>
        /// 지정된 영역의 셀들을 하나로 병합합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 영역의 폰트(글꼴, 크기, 굵게)를 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 워크시트 전체 셀의 기본 폰트를 일괄 설정합니다.
        /// </summary>
        public void SetSheetFont(string sheetName, string fontName = null, double? fontSize = null, bool? bold = null)
        {
            EnsureOpened();

            Excel.Worksheet ws = null;
            Excel.Range rng = null;
            try
            {
                ws = (Excel.Worksheet)_wb.Worksheets[sheetName];
                rng = ws.Cells;
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

        /// <summary>
        /// 특정 영역의 숫자 표시 형식(포맷)을 설정합니다. (예: "0.00", "YYYY-MM-DD")
        /// </summary>
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

        /// <summary>
        /// 특정 영역의 텍스트 정렬 방식(가로/세로) 및 줄 바꿈 여부를 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 워크시트 전체 셀의 정렬 방식을 일괄 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 영역 셀의 배경색(채우기 색상)을 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 영역에 테두리 선을 그립니다.
        /// </summary>
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

        /// <summary>
        /// 내용에 맞게 열 너비와 행 높이를 자동으로 조정합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 열의 너비를 명시적으로 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 현재 변경된 사항을 파일에 저장합니다.
        /// </summary>
        public void Save()
        {
            EnsureOpened();
            _wb.Save();
        }

        /// <summary>
        /// 현재 워크북을 지정된 다른 경로에 이름으로 저장합니다.
        /// </summary>
        public void SaveAs(string fullPath)
        {
            EnsureOpened();
            fullPath = Path.GetFullPath(fullPath);

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            _wb.SaveAs(fullPath);
        }

        /// <summary>
        /// 대량의 데이터를 쓰거나 서식을 변경할 때 엑셀 애플리케이션의 화면 갱신 및 자동 계산을 비활성화하여
        /// 작업 속도를 비약적으로 향상시키는 퍼포먼스 모드를 켜거나 끕니다.
        /// </summary>
        /// <param name="enabled">퍼포먼스 모드 활성화 여부</param>
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

        /// <summary>
        /// 사용이 끝난 워크북을 저장하고 엑셀 관련 COM 리소스를 안전하게 메모리에서 해제합니다.
        /// </summary>
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

        /// <summary>
        /// 엑셀 제어 메서드를 호출하기 전 애플리케이션이 열려있는지 검증합니다.
        /// </summary>
        private void EnsureOpened()
        {
            if (_app == null || _wb == null)
                throw new InvalidOperationException("Excel is not opened. Call OpenOrAttach() first.");
        }

        /// <summary>
        /// 기존 시트 이름들과 중복되지 않도록 접미사를 붙여 고유한 시트 이름을 생성합니다.
        /// </summary>
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

        /// <summary>
        /// 엑셀 시트 이름으로 사용할 수 없는 특수문자를 제거하고, 31자 길이 제한에 맞게 조정합니다.
        /// </summary>
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

        /// <summary>
        /// COM 객체의 참조 카운트를 강제로 0으로 만들어 메모리 릭(Memory Leak)을 방지합니다.
        /// </summary>
        private static void ReleaseCom(object o)
        {
            try
            {
                if (o != null && Marshal.IsComObject(o))
                    Marshal.FinalReleaseComObject(o);
            }
            catch { }
        }
    }
}