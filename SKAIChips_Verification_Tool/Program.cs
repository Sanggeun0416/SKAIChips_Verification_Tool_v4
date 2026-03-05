using System;
using System.Windows.Forms;
using SKAIChips_Verification_Tool.RegisterControl;

namespace SKAIChips_Verification_Tool
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // .NET 6 이상에서 지원하는 WinForms 기본 설정 초기화 (HighDPI, VisualStyles 등)
            ApplicationConfiguration.Initialize();

            // 1. 애플리케이션이 정상적으로 종료될 때 Excel COM 객체 해제
            Application.ApplicationExit += (_, __) =>
            {
                ExcelInteropTracker.DisposeAll();
            };

            // 2. 예기치 않은 예외나 프로세스 강제 종료 시에도 Excel COM 객체 해제 보장
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                ExcelInteropTracker.DisposeAll();
            };

            // 메인 폼 실행
            Application.Run(new MainForm());
        }
    }
}