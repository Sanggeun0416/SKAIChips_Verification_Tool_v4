using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SKAIChips_Verification_Tool.RegisterControl;

namespace SKAIChips_Verification_Tool
{
    /// <summary>
    /// 레지스터 맵 없이 Raw 바이트 단위로 I2C 또는 SPI 통신을 수행하기 위한 단순 직렬 통신(Simple Serial) 폼입니다.
    /// 통신 디버깅 및 하드웨어 기초 검증에 사용됩니다.
    /// </summary>
    public partial class SimpleSerialForm : Form
    {
        #region Fields

        /// <summary>I2C 통신을 위한 버스 인터페이스 객체입니다.</summary>
        private II2cBus? _i2c;

        /// <summary>SPI 통신을 위한 버스 인터페이스 객체입니다.</summary>
        private ISpiBus? _spi;

        /// <summary>현재 하드웨어 장비와 연결되어 있는지 여부를 나타내는 상태 플래그입니다.</summary>
        private bool _isConnected;

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// SimpleSerialForm의 새 인스턴스를 초기화합니다.
        /// </summary>
        public SimpleSerialForm()
        {
            InitializeComponent();
            InitializeControls();
        }

        /// <summary>
        /// 콤보박스 아이템 및 텍스트박스의 기본값을 초기화합니다.
        /// </summary>
        private void InitializeControls()
        {
            cmbProtocol.Items.AddRange(new object[] { "I2C", "SPI" });
            cmbProtocol.SelectedIndex = 0;

            cmbSpiMode.Items.AddRange(new object[] { "0", "1", "2", "3" });
            cmbSpiMode.SelectedIndex = 0;

            // 통신 기본값 설정
            txtSpeed.Text = "400"; // 기본 I2C 속도: 400 kHz (Fast Mode)
            txtSlaveAddr.Text = "0x50"; // 기본 I2C Slave Address (예: EEPROM)
        }

        #endregion

        #region Event Handlers (Connect, Read, Write)

        /// <summary>
        /// 하드웨어 연결 및 연결 해제를 토글하는 버튼 클릭 이벤트 핸들러입니다.
        /// 선택된 프로토콜(I2C/SPI)에 맞춰 버스를 초기화하고 연결을 시도합니다.
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // 이미 연결되어 있다면 연결 해제 수행
            if (_isConnected)
            {
                Disconnect();
                return;
            }

            try
            {
                int devIdx = (int)numDevIndex.Value;
                int speed = int.Parse(txtSpeed.Text);

                // 통신 프로토콜 공통 설정 객체 생성
                var settings = new ProtocolSettings
                {
                    // 현재 기본적으로 UM232H(또는 FT232H) 장치를 사용한다고 가정
                    // 필요 시 UI에서 장치 종류(FT4222 등)를 선택할 수 있도록 확장 가능
                    DeviceKind = DeviceKind.UM232H,
                };

                // 프로토콜 분기 처리
                if (cmbProtocol.SelectedItem?.ToString() == "I2C")
                {
                    settings.ProtocolRegLogType = ProtocolRegLogType.I2C;
                    settings.SpeedKbps = speed;

                    // I2C 버스 객체 생성 및 연결
                    _i2c = new I2cBus((uint)devIdx, settings);
                    if (!_i2c.Connect())
                        throw new Exception("I2C Connect Failed. 케이블 및 장치 연결 상태를 확인해 주세요.");
                }
                else
                {
                    settings.ProtocolRegLogType = ProtocolRegLogType.SPI;
                    settings.SpiClockKHz = speed;
                    settings.SpiMode = cmbSpiMode.SelectedIndex;

                    // SPI 버스 객체 생성 및 연결
                    _spi = new SpiBus((uint)devIdx, settings);
                    if (!_spi.Connect())
                        throw new Exception("SPI Connect Failed. 케이블 및 장치 연결 상태를 확인해 주세요.");
                }

                // 연결 성공 시 UI 상태 업데이트
                _isConnected = true;
                btnConnect.Text = "Disconnect";
                btnConnect.ForeColor = Color.Red;
                Log("Connected.");
                UpdateUiState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 열려 있는 통신 버스를 안전하게 닫고 리소스를 해제합니다.
        /// </summary>
        private void Disconnect()
        {
            _i2c?.Disconnect();
            _spi?.Disconnect();
            _i2c = null;
            _spi = null;

            _isConnected = false;

            btnConnect.Text = "Connect";
            btnConnect.ForeColor = Color.Black;
            Log("Disconnected.");
            UpdateUiState();
        }

        /// <summary>
        /// 입력된 헥사(Hex) 문자열 데이터를 파싱하여 하드웨어로 전송(Write)합니다.
        /// </summary>
        private void btnWrite_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show("장치가 연결되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] data = ParseHex(txtWriteData.Text);

                if (data.Length == 0)
                    throw new Exception("전송할 데이터가 입력되지 않았습니다.");

                if (cmbProtocol.SelectedItem?.ToString() == "I2C" && _i2c != null)
                {
                    byte addr = ParseByte(txtSlaveAddr.Text);

                    // I2C Write: 타겟 Slave Address와 데이터 배열 전달
                    _i2c.Write(addr, data);
                    Log($"[I2C WR] Addr:0x{addr:X2}, Data:{BitConverter.ToString(data)}");
                }
                else if (_spi != null)
                {
                    // SPI Write: 주소 개념 없이 CS 핀 활성화 후 데이터 전송
                    _spi.Write(data);
                    Log($"[SPI WR] Data:{BitConverter.ToString(data)}");
                }
            }
            catch (Exception ex)
            {
                Log($"[Error] Write Failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 하드웨어로부터 지정된 길이(바이트)만큼 데이터를 읽어옵니다(Read).
        /// I2C의 경우 Write 칸에 값이 있으면 Write 후 Repeated Start 조건으로 Read를 수행합니다.
        /// </summary>
        private void btnRead_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                MessageBox.Show("장치가 연결되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int len = (int)numReadLen.Value;

                if (len <= 0)
                    throw new Exception("읽어올 데이터 길이는 1 이상이어야 합니다.");

                byte[] result = new byte[len];

                if (cmbProtocol.SelectedItem?.ToString() == "I2C" && _i2c != null)
                {
                    byte addr = ParseByte(txtSlaveAddr.Text);

                    // I2C 특정 레지스터 주소를 읽기 위해 Write 후 Repeated Start 조건으로 Read 수행
                    if (!string.IsNullOrWhiteSpace(txtWriteData.Text))
                    {
                        byte[] wData = ParseHex(txtWriteData.Text);

                        // Stop 비트를 날리지 않음 (Restart 조건)
                        _i2c.Write(addr, wData, stop: false);
                    }

                    // I2C Read 수행 (타임아웃 1000ms 설정)
                    _i2c.Read(addr, result, 1000);
                    Log($"[I2C RD] Addr:0x{addr:X2}, Len:{len} -> {BitConverter.ToString(result)}");
                }
                else if (_spi != null)
                {
                    // SPI Read 수행 (필요 시 Dummy 데이터를 함께 보내면서 클럭을 발생시켜 데이터를 읽음)
                    _spi.Read(result);
                    Log($"[SPI RD] Len:{len} -> {BitConverter.ToString(result)}");
                }
            }
            catch (Exception ex)
            {
                Log($"[Error] Read Failed: {ex.Message}");
            }
        }

        #endregion

        #region UI Helpers & Parsers

        /// <summary>
        /// 통신 연결 상태 및 선택된 프로토콜에 따라 컨트롤의 활성화(Enabled) 상태를 갱신합니다.
        /// </summary>
        private void UpdateUiState()
        {
            bool isI2c = cmbProtocol.SelectedItem?.ToString() == "I2C";

            // I2C일 경우에만 Slave Address 입력창 활성화 (연결 중에는 변경 불가)
            txtSlaveAddr.Enabled = !_isConnected && isI2c;

            // SPI일 경우에만 SPI Mode 선택 콤보박스 활성화 (연결 중에는 변경 불가)
            cmbSpiMode.Enabled = !_isConnected && !isI2c;

            // 통신 프로토콜 변경은 연결 해제 상태에서만 가능
            cmbProtocol.Enabled = !_isConnected;
            txtSpeed.Enabled = !_isConnected;
            numDevIndex.Enabled = !_isConnected;
        }

        /// <summary>
        /// 통신 로그를 타임스탬프와 함께 RichTextBox 창에 출력하고 스크롤을 최하단으로 이동합니다.
        /// </summary>
        /// <param name="msg">출력할 로그 메시지</param>
        private void Log(string msg)
        {
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            rtbLog.ScrollToCaret();
        }

        /// <summary>
        /// 사용자가 입력한 16진수 포맷의 문자열을 파싱하여 바이트 배열(byte[])로 변환합니다.
        /// (예: "0x01, 0A, FF" -> byte[]{0x01, 0x0A, 0xFF})
        /// </summary>
        /// <param name="hex">입력된 헥사 문자열</param>
        /// <returns>변환된 바이트 배열</returns>
        private byte[] ParseHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();

            // 공백, 콤마, "0x" 등 불필요한 문자 제거
            hex = hex.Replace(" ", "").Replace(",", "").Replace("0x", "").Replace("0X", "");

            // 홀수 자리수 입력 시 ArgumentOutOfRangeException을 방지하기 위해 맨 앞에 '0' 패딩 추가
            if (hex.Length % 2 != 0)
                hex = "0" + hex;

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// 단일 바이트 헥사 문자열(I2C Slave 주소 등)을 byte 형식으로 변환합니다.
        /// </summary>
        /// <param name="hex">변환할 헥사 문자열 (예: "0x50")</param>
        /// <returns>변환된 byte 값</returns>
        private byte ParseByte(string hex)
        {
            hex = hex.Trim().Replace("0x", "").Replace("0X", "");
            return byte.Parse(hex, NumberStyles.HexNumber);
        }

        #endregion
    }
}