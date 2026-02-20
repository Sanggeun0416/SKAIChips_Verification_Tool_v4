namespace SKAIChips_Verification_Tool.RegisterControl
{
    public sealed class ExcelInteropReportSession : ITestReportSession
    {
        private readonly ExcelWriter _xl;

        public ExcelInteropReportSession(string reportPath, bool visible = true)
        {
            _xl = new ExcelWriter();
            _xl.OpenOrAttach(reportPath, visible: visible, readOnly: false, createIfMissing: true);
        }

        public IReportSheet CreateSheet(string sheetTitle)
        {
            var name = _xl.AddSheet(sheetTitle);
            return new ExcelInteropReportSheet(_xl, name);
        }

        public IReportSheet SelectSheet(string sheetTitle)
        {
            var name = _xl.SelectSheet(sheetTitle);
            return new ExcelInteropReportSheet(_xl, name);
        }

        public void Save() => _xl.Save();

        public void Dispose() => _xl.Dispose();

        private sealed class ExcelInteropReportSheet : IReportSheet
        {
            private readonly ExcelWriter _xl;
            public string Name
            {
                get;
            }

            public ExcelInteropReportSheet(ExcelWriter xl, string name)
            {
                _xl = xl;
                Name = name;
            }

            public void Write(int row, int col, object value) => _xl.WriteCell(Name, row, col, value);

            public void WriteRow(int row, int startCol, params object[] values)
            {
                var arr = new object[1, values.Length];
                for (int i = 0; i < values.Length; i++)
                    arr[0, i] = values[i];
                _xl.WriteRange(Name, row, startCol, arr);
            }

            public object Read(int row, int col) => _xl.ReadCell(Name, row, col);

            public void WriteRange(int startRow, int startCol, object[,] values) => _xl.WriteRange(Name, startRow, startCol, values);

            public void Focus(int row, int col)
            {
                _xl.Focus(Name, row, col);
            }

            public void Merge(int r1, int c1, int r2, int c2) => _xl.Merge(Name, r1, c1, r2, c2);

            public void SetFont(int r1, int c1, int r2, int c2, string fontName = null, double? fontSize = null, bool? bold = null)
                => _xl.SetFont(Name, r1, c1, r2, c2, fontName, fontSize, bold);

            public void SetSheetFont(string fontName = null, double? fontSize = null, bool? bold = null)
                => _xl.SetSheetFont(Name, fontName, fontSize, bold);

            public void SetNumberFormat(int r1, int c1, int r2, int c2, string format)
                => _xl.SetNumberFormat(Name, r1, c1, r2, c2, format);

            public void SetAlignmentCenter(int r1, int c1, int r2, int c2, bool wrapText = false)
                => _xl.SetAlignment(Name, r1, c1, r2, c2, wrapText: wrapText);

            public void SetAlignmentCenterAll()
                => _xl.SetAlignmentAll(Name);

            public void SetBorderAll(int r1, int c1, int r2, int c2)
                => _xl.SetBorderAll(Name, r1, c1, r2, c2);

            public void AutoFit(bool columns = true, bool rows = false) => _xl.AutoFit(Name, columns, rows);
        }
    }
}
