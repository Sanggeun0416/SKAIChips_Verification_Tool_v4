using System;
using System.Globalization;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 현재 선택된 칩 프로젝트의 지원 통신 방식(I2C, SPI)에 따라
    /// 통신 속도, Slave 주소, SPI 동작 모드 등을 설정하는 다이얼로그 폼입니다.
    /// </summary>
    public partial class ProtocolSetupForm : Form
    {
        #region Fields & Properties

        /// <summary>현재 선택되어 통신 설정을 적용할 대상 칩 프로젝트입니다.</summary>
        private readonly IChipProject _project;

        /// <summary>UI 초기화 중 프로토콜 콤보박스의 이벤트(SelectedIndexChanged)가 트리거되는 것을 방지하는 플래그입니다.</summary>
        private bool _suppressProtocolChanged;

        /// <summary>
        /// 사용자가 설정을 완료하고 [OK] 버튼을 눌렀을 때 생성되는 최종 프로토콜 설정 정보입니다.
        /// </summary>
        public ProtocolSettings? Result
        {
            get; private set;
        }

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// ProtocolSetupForm의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="project">현재 설정이 적용될 칩 프로젝트 인스턴스 (필수)</param>
        /// <param name="current">기존에 설정되어 있던 통신 설정 객체 (없을 경우 null)</param>
        /// <exception cref="ArgumentNullException">project 매개변수가 null일 때 발생합니다.</exception>
        public ProtocolSetupForm(IChipProject project, ProtocolSettings current)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            InitializeComponent();
            InitProtocolCombo();
            InitSpiModeCombo();

            // 초기화 중 이벤트 무시 플래그 설정
            _suppressProtocolChanged = true;
            try
            {
                if (current != null)
                    ApplyCurrent(current);
                else
                    ApplyDefault();
            }
            finally
            {
                _suppressProtocolChanged = false;
            }
        }

        /// <summary>
        /// 선택된 프로젝트가 지원하는 통신 프로토콜(I2C, SPI 등)을 콤보박스에 바인딩합니다.
        /// </summary>
        private void InitProtocolCombo()
        {
            comboProtocol.Items.Clear();

            foreach (var p in _project.SupportedProtocols)
                comboProtocol.Items.Add(p);

            if (comboProtocol.Items.Count > 0)
                comboProtocol.SelectedIndex = 0;
        }

        /// <summary>
        /// SPI 통신 모드(Mode 0 ~ Mode 3)와 해당 모드의 CPOL, CPHA 값을 콤보박스에 바인딩합니다.
        /// </summary>
        private void InitSpiModeCombo()
        {
            comboSpiMode.Items.Clear();

            comboSpiMode.Items.Add(new SpiModeItem(0, 0, 0));
            comboSpiMode.Items.Add(new SpiModeItem(1, 0, 1));
            comboSpiMode.Items.Add(new SpiModeItem(2, 1, 0));
            comboSpiMode.Items.Add(new SpiModeItem(3, 1, 1));

            comboSpiMode.SelectedIndex = 0;
        }

        #endregion

        #region UI Data Binding

        /// <summary>
        /// 기존 설정이 없을 경우, 선택된 칩 프로젝트에 정의된 기본값으로 UI를 설정합니다.
        /// </summary>
        private void ApplyDefault()
        {
            ApplyDefaultSpeedForSelectedProtocol();
            txtSlaveAddr.Text = $"0x{_project.DeviceAddress:X2}";
            comboSpiMode.SelectedIndex = 0;

            UpdateControlsEnabled();
        }

        /// <summary>
        /// 현재 선택된 프로토콜에 맞추어 통신 속도(NumericUpDown)를 칩의 기본 통신 주파수(ComFrequency)로 설정합니다.
        /// </summary>
        private void ApplyDefaultSpeedForSelectedProtocol()
        {
            if (comboProtocol.SelectedItem is not ProtocolRegLogType)
                return;

            SetNumericWithinRange(numSpeed, _project.ComFrequency);
        }

        /// <summary>
        /// 기존에 설정된 프로토콜 설정값(current)을 불러와 UI에 적용합니다.
        /// </summary>
        /// <param name="current">적용할 기존 설정 데이터</param>
        private void ApplyCurrent(ProtocolSettings current)
        {
            // 1. 프로토콜 선택
            for (var i = 0; i < comboProtocol.Items.Count; i++)
            {
                if (comboProtocol.Items[i] is ProtocolRegLogType pt && pt == current.ProtocolRegLogType)
                {
                    comboProtocol.SelectedIndex = i;
                    break;
                }
            }

            // 2. I2C 상세 설정 적용
            if (current.ProtocolRegLogType == ProtocolRegLogType.I2C)
            {
                if (current.SpeedKbps > 0 && current.SpeedKbps >= (int)numSpeed.Minimum && current.SpeedKbps <= (int)numSpeed.Maximum)
                    numSpeed.Value = current.SpeedKbps;

                txtSlaveAddr.Text = $"0x{current.I2cSlaveAddress:X2}";
            }
            // 3. SPI 상세 설정 적용
            else if (current.ProtocolRegLogType == ProtocolRegLogType.SPI)
            {
                if (current.SpiClockKHz > 0 && current.SpiClockKHz >= (int)numSpeed.Minimum && current.SpiClockKHz <= (int)numSpeed.Maximum)
                    numSpeed.Value = current.SpiClockKHz;

                for (int i = 0; i < comboSpiMode.Items.Count; i++)
                {
                    if (comboSpiMode.Items[i] is SpiModeItem it && it.Value == current.SpiMode)
                    {
                        comboSpiMode.SelectedIndex = i;
                        break;
                    }
                }
            }

            UpdateControlsEnabled();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 통신 프로토콜 콤보박스의 선택 항목이 변경될 때 발생하는 이벤트입니다.
        /// 해당 프로토콜에 맞는 UI 상태 및 기본 속도로 변경합니다.
        /// </summary>
        private void comboProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressProtocolChanged)
                return;

            ApplyDefaultSpeedForSelectedProtocol();
            UpdateControlsEnabled();
        }

        /// <summary>
        /// 설정 완료 [OK] 버튼 클릭 시 입력값을 검증하고 설정 객체를 생성합니다.
        /// </summary>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (comboProtocol.SelectedItem is not ProtocolRegLogType protocol)
            {
                MessageBox.Show("통신 Protocol을 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = new ProtocolSettings
            {
                ProtocolRegLogType = protocol
            };

            // I2C 검증 및 설정
            if (protocol == ProtocolRegLogType.I2C)
            {
                settings.SpeedKbps = (int)numSpeed.Value;

                if (!TryParseHexByte(txtSlaveAddr.Text, out var slave))
                {
                    MessageBox.Show("I2C Slave Address 형식이 잘못되었습니다.\n올바른 16진수 값을 입력해 주세요. (예: 0x50, 52)", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                settings.I2cSlaveAddress = slave;
            }
            // SPI 검증 및 설정
            else if (protocol == ProtocolRegLogType.SPI)
            {
                settings.SpiClockKHz = (int)numSpeed.Value;

                if (comboSpiMode.SelectedItem is SpiModeItem it)
                    settings.SpiMode = it.Value;
                else
                {
                    MessageBox.Show("SPI Mode를 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                settings.SpiLsbFirst = false; // 기본값: MSB First
            }

            Result = settings;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// [Cancel] 버튼 클릭 시 변경사항을 취소하고 다이얼로그를 닫습니다.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region UI Logic & Helper Methods

        /// <summary>
        /// 선택된 프로토콜(I2C 또는 SPI)에 따라 불필요한 컨트롤을 숨기고 필요한 컨트롤만 표시합니다.
        /// </summary>
        private void UpdateControlsEnabled()
        {
            var selected = comboProtocol.SelectedItem;
            var isI2c = selected is ProtocolRegLogType pt1 && pt1 == ProtocolRegLogType.I2C;
            var isSpi = selected is ProtocolRegLogType pt2 && pt2 == ProtocolRegLogType.SPI;

            lblSpeed.Visible = isI2c || isSpi;
            numSpeed.Visible = isI2c || isSpi;

            lblSlaveAddr.Visible = isI2c;
            txtSlaveAddr.Visible = isI2c;

            lblSpiMode.Visible = isSpi;
            comboSpiMode.Visible = isSpi;
        }

        /// <summary>
        /// 사용자가 입력한 문자열을 안전하게 16진수 byte 타입으로 변환합니다.
        /// </summary>
        /// <param name="text">변환할 텍스트 (예: "0x50", "A2")</param>
        /// <param name="value">변환된 byte 결과값</param>
        /// <returns>변환 성공 여부</returns>
        private static bool TryParseHexByte(string text, out byte value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            // C# 8.0 이상 문법 사용: text[2..]는 Substring(2)와 동일합니다.
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text[2..];

            return byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// 입력된 값을 NumericUpDown 컨트롤의 허용 범위(Minimum ~ Maximum) 내로 제한하여 설정합니다.
        /// </summary>
        /// <param name="n">대상 NumericUpDown 컨트롤</param>
        /// <param name="value">설정하려는 값</param>
        private static void SetNumericWithinRange(NumericUpDown n, decimal value)
        {
            if (value < n.Minimum)
                value = n.Minimum;
            if (value > n.Maximum)
                value = n.Maximum;

            n.Value = value;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// SPI 통신 모드를 콤보박스에 표시하기 위한 내부 헬퍼 클래스입니다.
        /// Mode 값(0~3)과 이에 상응하는 클럭 극성(CPOL), 위상(CPHA) 정보를 담고 있습니다.
        /// </summary>
        private sealed class SpiModeItem
        {
            /// <summary>SPI 모드 번호 (0, 1, 2, 3)</summary>
            public int Value
            {
                get;
            }

            /// <summary>콤보박스에 표시될 텍스트</summary>
            public string Text
            {
                get;
            }

            /// <summary>
            /// SpiModeItem의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="value">모드 번호</param>
            /// <param name="cpol">클럭 극성 (Clock Polarity)</param>
            /// <param name="cpha">클럭 위상 (Clock Phase)</param>
            public SpiModeItem(int value, int cpol, int cpha)
            {
                Value = value;
                Text = $"{value} (CPOL={cpol}, CPHA={cpha})";
            }

            /// <summary>
            /// 콤보박스에 객체 바인딩 시 표시될 문자열을 반환합니다.
            /// </summary>
            public override string ToString() => Text;
        }

        #endregion
    }
}