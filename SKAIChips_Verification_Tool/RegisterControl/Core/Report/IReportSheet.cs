namespace SKAIChips_Verification_Tool.RegisterControl
{
    public interface IReportSheet
    {
        string Name
        {
            get;
        }

        void Write(int row, int col, object value);
        void WriteRow(int row, int startCol, params object[] values);
        object Read(int row, int col);
        void WriteRange(int startRow, int startCol, object[,] values);
        void Focus(int row, int col);
        void Merge(int r1, int c1, int r2, int c2);
        void SetFont(int r1, int c1, int r2, int c2, string fontName = null, double? fontSize = null, bool? bold = null);
        public void SetSheetFont(string fontName = null, double? fontSize = null, bool? bold = null);
        void SetNumberFormat(int r1, int c1, int r2, int c2, string format);
        void SetAlignmentCenter(int r1, int c1, int r2, int c2, bool wrapText = false);
        void SetAlignmentCenterAll();
        void SetBorderAll(int r1, int c1, int r2, int c2);

        void AutoFit(bool columns = true, bool rows = false);
    }
}
