using SKAIChips_Verification_Tool.HCIControl;
using SKAIChips_Verification_Tool.Instrument;
using SKAIChips_Verification_Tool.RegisterControl;

namespace SKAIChips_Verification_Tool
{
    public partial class MainForm : Form
    {
        private InstrumentForm _instrumentForm;

        public static string AppName { get; } = "SKAIChips_Verification";
        public static string Version { get; } = "v4.0.0 [Confidential]";

        public MainForm()
        {
            InitializeComponent();
            this.MdiChildActivate += MainForm_MdiChildActivate;
            InstrumentRegistry.Instance.Load();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = $"{AppName} {Version}";
            OpenOrShowForm<RegisterControlForm>();
        }

        private void OpenOrShowForm<T>() where T : Form, new()
        {
            var form = MdiChildren.OfType<T>().FirstOrDefault();
            if (form != null)
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form.Activate();
                return;
            }

            var newForm = new T
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };

            newForm.Show();
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            bool isRegFormActive = this.ActiveMdiChild is RegisterControlForm;

            if (menuSetupInstrument != null)
                menuSetupInstrument.Enabled = isRegFormActive;

            if (getsAllToolStripMenuItem != null)
                getsAllToolStripMenuItem.Enabled = isRegFormActive;
        }

        private void menuRegisterControl_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<RegisterControlForm>();
        }

        private void menuHCIControl_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<HCIControlForm>();
        }

        private void menuSimpleSerial_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<SimpleSerialForm>();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuSetupInstrument_Click(object sender, EventArgs e)
        {
            if (_instrumentForm == null || _instrumentForm.IsDisposed)
            {
                _instrumentForm = new InstrumentForm
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                _instrumentForm.Show(this);
            }
            else
            {
                if (!_instrumentForm.Visible)
                    _instrumentForm.Show(this);

                _instrumentForm.Activate();
            }
        }

        private async void getsAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var regForm = this.ActiveMdiChild as RegisterControlForm;
            if (regForm == null)
                return;

            regForm.AppendLog("[System] Try to Connect All Instruments...");

            var instrumentTypes = InstrumentRegistry.Instance.GetEnabledInstrumentTypes();

            int successCount = 0;
            int totalCount = 0;

            await Task.Run(() =>
            {
                foreach (var typeName in instrumentTypes)
                {
                    totalCount++;
                    try
                    {
                        var instrument = InstrumentRegistry.Instance.GetByType(typeName);

                        string idn = instrument.Query("*IDN?");

                        regForm.AppendLog($"[OK] {typeName}: {idn.Trim()}");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        regForm.AppendLog($"[FAIL] {typeName}: {ex.Message}");
                    }
                }
            });

            regForm.AppendLog($"[Result] {successCount} / {totalCount} Instruments Connected.");
        }
    }
}
