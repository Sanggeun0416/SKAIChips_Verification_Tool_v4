namespace SKAIChips_Verification_Tool.RegisterControl
{
    public interface ITestReportSession : IDisposable
    {
        IReportSheet CreateSheet(string sheetTitle);
        IReportSheet SelectSheet(string sheetTitle);
        void Save();
    }
}
