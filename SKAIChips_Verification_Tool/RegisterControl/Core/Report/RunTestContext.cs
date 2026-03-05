namespace SKAIChips_Verification_Tool.RegisterControl
{
    public sealed class RunTestContext
    {
        public ITestReportSession Report
        {
            get;
        }
        public RunTestContext(ITestReportSession report) => Report = report;
    }
}
