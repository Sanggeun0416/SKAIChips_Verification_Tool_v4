using SKAIChips_Verification_Tool.Instrument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 검증 대상이 되는 칩 프로젝트(예: Chicago, Oasis 등)가 상속받아야 하는 최상위 추상 클래스입니다.
    /// 하드웨어 버스(I2C/SPI), UI 제어, 자동화 테스트 프레임워크 연동, 그리고 SCPI 계측기 제어 기능을 통합 제공합니다.
    /// </summary>
    public abstract class ProjectBase : ISpiChipProject, II2cChipProject, IChipProjectWithTests, IRegisterChip, IChipTestSuite
    {
        #region Fields

        /// <summary>진행률(ProgressBar) 표시를 위한 전체 스텝 수입니다.</summary>
        private int _totalSteps = 1;

        /// <summary>현재 진행 중인 스텝 번호입니다.</summary>
        private int _currentStep = 0;

        /// <summary>현재 연결된 하드웨어 통신 버스 (I2C 또는 SPI) 객체입니다.</summary>
        protected object? _bus;

        /// <summary>UI를 제어하거나 로그를 출력하기 위해 연결된 메인 폼 참조입니다.</summary>
        protected RegisterControlForm? _regCont;

        #endregion

        #region Protected Properties (Bus Accessors)

        /// <summary>현재 연결된 통신 버스를 SPI 형식으로 캐스팅하여 반환합니다.</summary>
        protected ISpiBus? SpiBus => _bus as ISpiBus;

        /// <summary>현재 연결된 통신 버스를 I2C 형식으로 캐스팅하여 반환합니다.</summary>
        protected II2cBus? I2cBus => _bus as II2cBus;

        /// <summary>Chicago 전용 커스텀 SPI 버스 인터페이스로 캐스팅하여 반환합니다.</summary>
        protected IChicagoSpiBus? ChicagoSpiBus => _bus as IChicagoSpiBus;

        #endregion

        #region Abstract & Virtual Properties

        /// <summary>
        /// 칩 프로젝트의 고유 이름입니다. (파생 클래스에서 반드시 구현해야 합니다)
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 레지스터 맵 엑셀 파일 등을 자동 탐색할 때 사용할 키워드 목록입니다.
        /// 기본값으로 <see cref="Name"/> 속성이 포함됩니다.
        /// </summary>
        public virtual IEnumerable<string> ProjectKeywords => new[] { Name };

        /// <summary>
        /// 해당 프로젝트(칩)가 지원하는 통신 방식 목록을 반환합니다. 기본적으로 SPI와 I2C를 반환합니다.
        /// </summary>
        public virtual IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.SPI, ProtocolRegLogType.I2C };

        /// <summary>해당 칩의 기본 통신 주파수(kHz)입니다.</summary>
        public virtual uint ComFrequency => 0;

        /// <summary>해당 칩의 기본 I2C Slave Address 입니다.</summary>
        public virtual byte DeviceAddress => 0x00;

        /// <summary>현재 활성화된 레지스터 맵 시트의 이름입니다.</summary>
        public virtual string CurrentSheetName { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 통신 버스가 할당되지 않은 상태로 인스턴스를 초기화합니다.
        /// </summary>
        protected ProjectBase()
        {
        }

        /// <summary>
        /// 지정된 SPI 버스를 사용하여 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="bus">연결된 SPI 버스 인스턴스</param>
        protected ProjectBase(ISpiBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// 지정된 I2C 버스를 사용하여 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="bus">연결된 I2C 버스 인스턴스</param>
        protected ProjectBase(II2cBus bus)
        {
            _bus = bus;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// 리플렉션을 통해 지정된 SPI 버스를 사용하는 현재 프로젝트 타입의 새 인스턴스(Chip)를 생성합니다.
        /// </summary>
        public IRegisterChip CreateChip(ISpiBus bus, ProtocolSettings settings)
        {
            if (settings.ProtocolRegLogType != ProtocolRegLogType.SPI)
                throw new InvalidOperationException($"[{Name}] Request mismatch: Expected SPI, but got {settings.ProtocolRegLogType}");

            return (IRegisterChip)Activator.CreateInstance(this.GetType(), bus)!;
        }

        /// <summary>
        /// 리플렉션을 통해 지정된 I2C 버스를 사용하는 현재 프로젝트 타입의 새 인스턴스(Chip)를 생성합니다.
        /// </summary>
        public IRegisterChip CreateChip(II2cBus bus, ProtocolSettings settings)
        {
            if (settings.ProtocolRegLogType != ProtocolRegLogType.I2C)
                throw new InvalidOperationException($"[{Name}] Request mismatch: Expected I2C, but got {settings.ProtocolRegLogType}");

            return (IRegisterChip)Activator.CreateInstance(this.GetType(), bus)!;
        }

        /// <summary>
        /// 열려 있는 UI 폼(RegisterControlForm)을 찾아 참조를 연결하고, 테스트 스위트 인스턴스로 반환합니다.
        /// </summary>
        public IChipTestSuite CreateTestSuite(IRegisterChip chip)
        {
            var instance = (ProjectBase)chip;
            instance._regCont = Application.OpenForms.OfType<RegisterControlForm>().FirstOrDefault();

            if (instance._regCont == null)
                throw new InvalidOperationException("RegisterControlForm is not open.");

            return instance;
        }

        #endregion

        #region Hardware Abstraction (IRegisterChip)

        /// <summary>
        /// UI의 "Test Slot 1~10" 버튼에 할당할 사용자 정의 동작을 반환합니다.
        /// 파생 클래스에서 오버라이드하여 구현할 수 있습니다.
        /// </summary>
        public virtual TestSlotAction[] GetTestSlotActions() => Array.Empty<TestSlotAction>();

        /// <summary>
        /// 특정 주소(Address)에 데이터(Data)를 씁니다(Write). 파생 클래스에서 하드웨어 스펙에 맞게 구현해야 합니다.
        /// </summary>
        public abstract void WriteRegister(uint address, uint data);

        /// <summary>
        /// 특정 주소(Address)의 데이터(Data)를 읽어옵니다(Read). 파생 클래스에서 하드웨어 스펙에 맞게 구현해야 합니다.
        /// </summary>
        public abstract uint ReadRegister(uint address);

        #endregion

        #region Test Suite Automation (Reflection & Execution)

        /// <summary>
        /// [ChipTest] 특성이 부여된 메서드들을 수집하여 테스트 목록으로 반환합니다.
        /// </summary>
        public IReadOnlyList<ChipTestInfo> Tests => GetTestMethods();

        /// <summary>
        /// 리플렉션을 이용해 현재 클래스(파생 클래스 포함)에 정의된 모든[ChipTest] 메서드를 탐색합니다.
        /// </summary>
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

        /// <summary>
        /// 테스트 실행 전 UI 또는 환경을 초기화하는 훅(Hook) 메서드입니다.
        /// false를 반환하면 테스트 실행이 취소됩니다.
        /// </summary>
        public virtual bool PrepareTest(string testId, ITestUiContext uiContext)
        {
            return true;
        }

        /// <summary>
        /// 지정된 테스트 ID(카테고리.이름)에 해당하는 메서드를 리플렉션으로 찾아 비동기적으로 실행합니다.
        /// 메서드의 파라미터(로거, 캔슬토큰, 컨텍스트 등)를 동적으로 매핑하여 주입합니다.
        /// </summary>
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
                // 'AUTO' 카테고리 테스트의 경우, 자동으로 Excel 리포트를 생성하여 컨텍스트에 담아줍니다.
                if (attribute != null && attribute.Category == "AUTO")
                {
                    string reportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                    if (!Directory.Exists(reportDir))
                        Directory.CreateDirectory(reportDir);

                    string nowDate = DateTime.Now.ToString("yyMMdd");
                    string reportPath = Path.Combine(reportDir, $"RunReport_{nowDate}.xlsx");

                    // Excel Interop 세션 시작 (UI 보이기)
                    var reportSession = new ExcelInteropReportSession(reportPath, visible: true);

                    ctx = new RunTestContext(reportSession);
                }

                var parameters = method.GetParameters();
                var args = new object?[parameters.Length];

                // 테스트 메서드의 매개변수 타입에 맞춰 인자를 동적으로 주입(Dependency Injection)
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

                // 테스트 메서드 호출
                var result = method.Invoke(this, args);
                if (result is Task t)
                    await t;

                // 자동 생성된 리포트가 있다면 저장
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

        #endregion

        #region Helper Methods (Logging & UI Threading)

        /// <summary>
        /// Null 참조 검사를 포함하여 안전하게 비동기 로그 델리게이트를 호출합니다.
        /// </summary>
        protected async Task SafeLogAsync(Func<string, string, Task> log, string level, string msg)
        {
            if (log != null)
                await log(level, msg);
        }

        /// <summary>
        /// 스레드 안전하게 메인 폼의 테스트 로그 창(RichTextBox)에 메시지를 출력합니다.
        /// </summary>
        protected void AppendLog(string level, string message)
        {
            Ui(() =>
            {
                _regCont?.AddTestLogRow(level, message);
            });
        }

        /// <summary>
        /// 백그라운드 스레드에서 UI 컨트롤에 접근해야 할 때, 메인 스레드로 안전하게 마샬링(Invoke)합니다.
        /// </summary>
        protected void Ui(Action action)
        {
            if (_regCont != null && _regCont.InvokeRequired)
                _regCont.Invoke(action);
            else
                action();
        }

        /// <summary>
        /// 백그라운드 스레드에서 UI 컨트롤에 접근하여 결과값을 반환받을 때 안전하게 마샬링(Invoke)합니다.
        /// </summary>
        protected T Ui<T>(Func<T> func)
        {
            if (_regCont != null && _regCont.InvokeRequired)
                return _regCont.Invoke(func);
            return func();
        }

        /// <summary>
        /// 주어진 레지스터 주소가 속해 있는 레지스터 맵 그룹(엑셀 시트 이름)을 반환합니다.
        /// </summary>
        protected string GetSheetNameByAddress(uint address)
        {
            if (_regCont?.RegMgr == null)
                return string.Empty;

            var group = _regCont.RegMgr.Groups
                .FirstOrDefault(g => g.Registers.Any(r => r.Address == address));

            return group?.Name ?? string.Empty;
        }

        /// <summary>
        /// 메인 스레드에서 안전하게 MessageBox를 띄우고 사용자의 응답(DialogResult)을 반환합니다.
        /// </summary>
        protected DialogResult ShowMsg(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            DialogResult dr = DialogResult.None;
            Ui(() => dr = MessageBox.Show(text, caption, buttons, icon));
            return dr;
        }

        #endregion

        #region Helper Methods (Progress Bar)

        /// <summary>
        /// UI의 ProgressBar를 초기화하고 총 스텝(단계) 수를 설정합니다.
        /// </summary>
        protected void InitProgress(int totalSteps)
        {
            _totalSteps = Math.Max(1, totalSteps);
            _currentStep = 0;
            ReportProgress(0);
        }

        /// <summary>
        /// 현재 단계를 1 증가시키고 UI ProgressBar를 갱신합니다.
        /// </summary>
        protected void Step()
        {
            _currentStep++;

            if (_currentStep > _totalSteps)
                _currentStep = _totalSteps;

            int percent = (int)((double)_currentStep / _totalSteps * 100.0);
            ReportProgress(percent);
        }

        /// <summary>
        /// 직접 백분율(%)을 지정하여 UI ProgressBar를 갱신합니다.
        /// </summary>
        protected void ReportProgress(int percent)
        {
            _regCont?.UpdateTestProgress(percent);
        }

        /// <summary>
        /// 현재 스텝과 총 스텝 수를 기반으로 백분율을 계산하여 UI ProgressBar를 갱신합니다.
        /// </summary>
        protected void ReportProgress(int current, int total)
        {
            if (total <= 0)
                total = 1;
            if (current > total)
                current = total;

            int percent = (int)((double)current / total * 100.0);
            ReportProgress(percent);
        }

        #endregion

        #region SCPI Instrument Accessors

        // InstrumentRegistry(계측기 레지스트리)를 통해 싱글톤으로 관리되는 SCPI 통신 객체들에 접근하기 위한 프로퍼티들입니다.
        // 테스트 스크립트 작성 시 "PowerSupply0.Query("*IDN?");" 와 같이 직관적으로 사용할 수 있게 해줍니다.

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

        #endregion
    }
}