using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    internal static class ExcelInteropTracker
    {
        private sealed class Entry
        {
            public Excel.Application App
            {
                get;
            }
            public bool OwnsApp
            {
                get;
            }

            public Entry(Excel.Application app, bool ownsApp)
            {
                App = app;
                OwnsApp = ownsApp;
            }
        }

        private static readonly object _sync = new();
        private static readonly List<Entry> _apps = new();

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

        public static void Unregister(Excel.Application app)
        {
            if (app == null)
                return;
            lock (_sync)
            {
                _apps.RemoveAll(e => ReferenceEquals(e.App, app));
            }
        }

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
                            e.App.DisplayAlerts = false;
                        }
                        catch { }
                        try
                        {
                            e.App.Quit();
                        }
                        catch { }
                    }
                }
                catch { }
                finally
                {
                    ReleaseCom(e.App);
                    Unregister(e.App);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

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
