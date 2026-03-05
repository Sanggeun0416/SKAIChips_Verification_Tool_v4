using SKAIChips_Verification_Tool.RegisterControl;

namespace SKAIChips_Verification_Tool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.ApplicationExit += (_, __) =>
            {
                ExcelInteropTracker.DisposeAll();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                ExcelInteropTracker.DisposeAll();
            };
            Application.Run(new MainForm());
        }
    }
}
