using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// Excel Interop을 통해 생성된 COM 객체(Excel Application)들을 전역적으로 추적하고,
    /// 프로그램 종료 시 메모리 누수(좀비 프로세스) 없이 완전히 해제하도록 관리하는 정적 유틸리티 클래스입니다.
    /// 멀티스레드 환경에서도 안전하도록 락(lock)을 사용하여 동기화 처리됩니다.
    /// </summary>
    internal static class ExcelInteropTracker
    {
        /// <summary>
        /// 추적 중인 개별 엑셀 애플리케이션 인스턴스의 정보와 소유권 상태를 보관하는 내부 클래스입니다.
        /// </summary>
        private sealed class Entry
        {
            /// <summary>
            /// 추적 중인 Excel.Application 객체 인스턴스입니다.
            /// </summary>
            public Excel.Application App
            {
                get;
            }

            /// <summary>
            /// 현재 프로그램이 이 엑셀 프로세스의 생명주기를 직접 소유하고 관리(생성/종료)하는지 여부입니다.
            /// true인 경우 프로그램 종료 시 해당 엑셀 프로세스도 강제로 종료(Quit)됩니다.
            /// </summary>
            public bool OwnsApp
            {
                get;
            }

            /// <summary>
            /// Entry 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="app">추적할 엑셀 애플리케이션 객체</param>
            /// <param name="ownsApp">앱의 강제 종료 소유권 여부</param>
            public Entry(Excel.Application app, bool ownsApp)
            {
                App = app;
                OwnsApp = ownsApp;
            }
        }

        private static readonly object _sync = new();
        private static readonly List<Entry> _apps = new();

        /// <summary>
        /// 생성되거나 참조된 엑셀 애플리케이션 객체를 추적 리스트에 등록합니다.
        /// 동일한 객체가 중복으로 등록되지 않도록 방지합니다.
        /// </summary>
        /// <param name="app">등록할 Excel.Application COM 객체</param>
        /// <param name="ownsApp">프로그램이 이 앱의 종료를 책임질 것인지 여부 (직접 생성한 경우 true)</param>
        public static void Register(Excel.Application app, bool ownsApp)
        {
            if (app == null)
                return;
            lock (_sync)
            {
                if (_apps.Any(e => ReferenceEquals(e.App, app)))
                    return;
                _apps.Add(new Entry(app, ownsApp));
            }
        }

        /// <summary>
        /// 특정 엑셀 애플리케이션 객체를 더 이상 추적하지 않도록 리스트에서 제거합니다.
        /// (일반적으로 수동으로 객체를 닫고 해제했을 때 호출됩니다.)
        /// </summary>
        /// <param name="app">추적 리스트에서 제거할 Excel.Application COM 객체</param>
        public static void Unregister(Excel.Application app)
        {
            if (app == null)
                return;
            lock (_sync)
            {
                _apps.RemoveAll(e => ReferenceEquals(e.App, app));
            }
        }

        /// <summary>
        /// 추적 중인 모든 엑셀 애플리케이션에 대해 경고 창을 끄고 강제 종료(Quit)를 시도한 뒤,
        /// 할당된 COM 객체 메모리를 완전히 해제(Release)하고 .NET 가비지 컬렉터(GC)를 강제 호출합니다.
        /// 주로 프로그램이 종료될 때(AppDomain Exit) 한 번 호출됩니다.
        /// </summary>
        public static void DisposeAll()
        {
            Entry[] snapshot;
            lock (_sync)
                snapshot = _apps.ToArray();

            foreach (var e in snapshot)
            {
                try
                {
                    if (e.OwnsApp)
                    {
                        try
                        {
                            // 엑셀 종료 시 "저장하시겠습니까?" 등의 팝업 알림 무시
                            e.App.DisplayAlerts = false;
                        }
                        catch { }
                        try
                        {
                            // 엑셀 애플리케이션(프로세스) 종료 명령
                            e.App.Quit();
                        }
                        catch { }
                    }
                }
                catch { }
                finally
                {
                    // 엑셀 프로세스가 닫혔더라도 메모리에 남아있는 COM 참조 카운트를 강제로 0으로 만들어 해제
                    ReleaseCom(e.App);
                    Unregister(e.App);
                }
            }

            // COM 객체가 완전히 해제되도록 메모리 수집을 두 번 강제 실행 (Generation 정리)
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// 지정된 COM 객체의 참조 카운트를 즉시 0으로 감소시켜 메모리에서 안전하게 분리(Release)합니다.
        /// </summary>
        /// <param name="o">해제할 COM 객체 (일반적으로 Excel 인터페이스 객체)</param>
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