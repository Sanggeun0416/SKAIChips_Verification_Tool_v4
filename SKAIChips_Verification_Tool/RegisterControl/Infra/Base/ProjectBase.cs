using SKAIChips_Verification_Tool.Instrument;
using System.Reflection;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public abstract class ProjectBase : ISpiChipProject, II2cChipProject, IChipProjectWithTests, IRegisterChip, IChipTestSuite
    {
        private int _totalSteps = 1;
        private int _currentStep = 0;

        protected object? _bus; protected RegisterControlForm? _regCont;
        protected ISpiBus? SpiBus => _bus as ISpiBus;
        protected II2cBus? I2cBus => _bus as II2cBus;
        protected IChicagoSpiBus? ChicagoSpiBus => _bus as IChicagoSpiBus;

        public abstract string Name
        {
            get;
        }

        public virtual IEnumerable<string> ProjectKeywords => new[] { Name };

        public virtual IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.SPI, ProtocolRegLogType.I2C };

        public virtual uint ComFrequency => 0;
        public virtual byte DeviceAddress => 0x00;

        protected ProjectBase()
        {
        }

        protected ProjectBase(ISpiBus bus)
        {
            _bus = bus;
        }

        protected ProjectBase(II2cBus bus)
        {
            _bus = bus;
        }

        public IRegisterChip CreateChip(ISpiBus bus, ProtocolSettings settings)
        {
            if (settings.ProtocolRegLogType != ProtocolRegLogType.SPI)
                throw new InvalidOperationException($"[{Name}] Request mismatch: Expected SPI, but got {settings.ProtocolRegLogType}");

            return (IRegisterChip)Activator.CreateInstance(this.GetType(), bus)!;
        }

        public IRegisterChip CreateChip(II2cBus bus, ProtocolSettings settings)
        {
            if (settings.ProtocolRegLogType != ProtocolRegLogType.I2C)
                throw new InvalidOperationException($"[{Name}] Request mismatch: Expected I2C, but got {settings.ProtocolRegLogType}");

            return (IRegisterChip)Activator.CreateInstance(this.GetType(), bus)!;
        }

        public IChipTestSuite CreateTestSuite(IRegisterChip chip)
        {
            var instance = (ProjectBase)chip;
            instance._regCont = Application.OpenForms.OfType<RegisterControlForm>().FirstOrDefault();

            if (instance._regCont == null)
                throw new InvalidOperationException("RegisterControlForm is not open.");

            return instance;
        }

        public virtual string CurrentSheetName { get; set; } = string.Empty;

        public virtual TestSlotAction[] GetTestSlotActions() => Array.Empty<TestSlotAction>();

        public abstract void WriteRegister(uint address, uint data);
        public abstract uint ReadRegister(uint address);

        public IReadOnlyList<ChipTestInfo> Tests => GetTestMethods();

        private List<ChipTestInfo> GetTestMethods()
        {
            var list = new List<ChipTestInfo>();
            foreach (var m in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<ChipTestAttribute>();
                if (attr != null)
                {
                    list.Add(new ChipTestInfo($"{attr.Category}.{attr.Name}", attr.Name, attr.Description, attr.Category));
                }
            }
            return list;
        }

        public async Task Run_TEST(string testId, Func<string, string, Task> log, CancellationToken ct)
        {
            var method = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m =>
                {
                    var attr = m.GetCustomAttribute<ChipTestAttribute>();
                    return attr != null && $"{attr.Category}.{attr.Name}" == testId;
                });

            if (method == null)
            {
                await SafeLogAsync(log, "ERROR", $"Test method not found for ID: {testId}");
                return;
            }

            var attribute = method.GetCustomAttribute<ChipTestAttribute>();
            RunTestContext? ctx = null;

            try
            {
                if (attribute != null && attribute.Category == "AUTO")
                {
                    string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                    if (!Directory.Exists(reportDir))
                        Directory.CreateDirectory(reportDir);

                    string nowDate = DateTime.Now.ToString("yyMMdd");
                    string reportPath = Path.Combine(reportDir, $"RunReport_{nowDate}.xlsx");

                    var reportSession = new ExcelInteropReportSession(reportPath, visible: true);

                    ctx = new RunTestContext(reportSession);
                }

                var parameters = method.GetParameters();
                var args = new object?[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var pType = parameters[i].ParameterType;

                    if (pType == typeof(Func<string, string, Task>))
                        args[i] = log;
                    else if (pType == typeof(CancellationToken))
                        args[i] = ct;
                    else if (pType == typeof(RunTestContext))
                        args[i] = ctx;
                    else
                        args[i] = null;
                }

                var result = method.Invoke(this, args);
                if (result is Task t)
                    await t;

                if (ctx != null)
                {
                    ctx.Report.Save();
                }
            }
            catch (Exception ex)
            {
                var actualEx = ex.InnerException ?? ex;
                string level = actualEx is OperationCanceledException ? "STOP" : "ERROR";
                await SafeLogAsync(log, level, actualEx.Message);
            }
        }

        protected async Task SafeLogAsync(Func<string, string, Task> log, string level, string msg)
        {
            if (log != null)
                await log(level, msg);
        }

        protected void AppendLog(string level, string message)
        {
            Ui(() =>
            {
                _regCont?.AddTestLogRow(level, message);
            });
        }

        protected void Ui(Action action)
        {
            if (_regCont != null && _regCont.InvokeRequired)
                _regCont.Invoke(action);
            else
                action();
        }

        protected T Ui<T>(Func<T> func)
        {
            if (_regCont != null && _regCont.InvokeRequired)
                return _regCont.Invoke(func);
            return func();
        }

        protected string GetSheetNameByAddress(uint address)
        {
            if (_regCont?.RegMgr == null)
                return string.Empty;

            var group = _regCont.RegMgr.Groups
                .FirstOrDefault(g => g.Registers.Any(r => r.Address == address));

            return group?.Name ?? string.Empty;
        }

        protected void InitProgress(int totalSteps)
        {
            _totalSteps = Math.Max(1, totalSteps);
            _currentStep = 0;
            ReportProgress(0);
        }

        protected void Step()
        {
            _currentStep++;

            if (_currentStep > _totalSteps)
                _currentStep = _totalSteps;

            int percent = (int)((double)_currentStep / _totalSteps * 100.0);
            ReportProgress(percent);
        }

        protected void ReportProgress(int percent)
        {
            _regCont?.UpdateTestProgress(percent);
        }

        protected void ReportProgress(int current, int total)
        {
            if (total <= 0)
                total = 1;
            if (current > total)
                current = total;

            int percent = (int)((double)current / total * 100.0);
            ReportProgress(percent);
        }

        protected DialogResult ShowMsg(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            DialogResult dr = DialogResult.None;
            Ui(() => dr = MessageBox.Show(text, caption, buttons, icon));
            return dr;
        }

        protected IScpiClient? PowerSupply0 => InstrumentRegistry.Instance.GetByType("PowerSupply0");
        protected IScpiClient? PowerSupply1 => InstrumentRegistry.Instance.GetByType("PowerSupply1");
        protected IScpiClient? PowerSupply2 => InstrumentRegistry.Instance.GetByType("PowerSupply2");
        protected IScpiClient? DigitalMultimeter0 => InstrumentRegistry.Instance.GetByType("DigitalMultimeter0");
        protected IScpiClient? DigitalMultimeter1 => InstrumentRegistry.Instance.GetByType("DigitalMultimeter1");
        protected IScpiClient? DigitalMultimeter2 => InstrumentRegistry.Instance.GetByType("DigitalMultimeter2");
        protected IScpiClient? DigitalMultimeter3 => InstrumentRegistry.Instance.GetByType("DigitalMultimeter3");
        protected IScpiClient? OscilloScope0 => InstrumentRegistry.Instance.GetByType("OscilloScope0");
        protected IScpiClient? SpectrumAnalyzer => InstrumentRegistry.Instance.GetByType("SpectrumAnalyzer");
        protected IScpiClient? TempChamber => InstrumentRegistry.Instance.GetByType("TempChamber");
        protected IScpiClient? SignalGenerator0 => InstrumentRegistry.Instance.GetByType("SignalGenerator0");
        protected IScpiClient? SignalGenerator1 => InstrumentRegistry.Instance.GetByType("SignalGenerator1");
        protected IScpiClient? ElectronicLoad => InstrumentRegistry.Instance.GetByType("ElectronicLoad");

    }
}