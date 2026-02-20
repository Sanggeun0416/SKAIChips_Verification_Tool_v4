using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SKAIChips_Verification_Tool.RegisterControl; // ProtocolSettings, I2cBus 등 참조

namespace SKAIChips_Verification_Tool
{
    public partial class SimpleSerialForm : Form
    {
        private II2cBus _i2c;
        private ISpiBus _spi;
        private bool _isConnected;

        public SimpleSerialForm()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            cmbProtocol.Items.AddRange(new object[] { "I2C", "SPI" });
            cmbProtocol.SelectedIndex = 0;
            cmbSpiMode.Items.AddRange(new object[] { "0", "1", "2", "3" });
            cmbSpiMode.SelectedIndex = 0;

            // 기본값 설정
            txtSpeed.Text = "400"; // kHz
            txtSlaveAddr.Text = "0x50";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                Disconnect();
                return;
            }

            try
            {
                int devIdx = (int)numDevIndex.Value;
                int speed = int.Parse(txtSpeed.Text);

                // 설정 객체 생성
                var settings = new ProtocolSettings
                {
                    DeviceKind = DeviceKind.UM232H, // 혹은 FT4222 (상황에 맞게 선택 UI 추가 가능)
                };

                if (cmbProtocol.SelectedItem.ToString() == "I2C")
                {
                    settings.ProtocolRegLogType = ProtocolRegLogType.I2C;
                    settings.SpeedKbps = speed;
                    // I2C 버스 생성 및 연결
                    _i2c = new I2cBus((uint)devIdx, settings);
                    if (!_i2c.Connect())
                        throw new Exception("I2C Connect Failed");
                }
                else
                {
                    settings.ProtocolRegLogType = ProtocolRegLogType.SPI;
                    settings.SpiClockKHz = speed;
                    settings.SpiMode = cmbSpiMode.SelectedIndex;
                    // SPI 버스 생성 및 연결
                    _spi = new SpiBus((uint)devIdx, settings);
                    if (!_spi.Connect())
                        throw new Exception("SPI Connect Failed");
                }

                _isConnected = true;
                btnConnect.Text = "Disconnect";
                btnConnect.ForeColor = Color.Red;
                Log("Connected.");
                UpdateUiState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}");
            }
        }

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

        private void btnWrite_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
                return;

            try
            {
                byte[] data = ParseHex(txtWriteData.Text);

                if (cmbProtocol.SelectedItem.ToString() == "I2C")
                {
                    byte addr = ParseByte(txtSlaveAddr.Text);
                    // I2C Write: 첫 번째 인자로 Slave Address 전달
                    _i2c.Write(addr, data);
                    Log($"[I2C WR] Addr:0x{addr:X2}, Data:{BitConverter.ToString(data)}");
                }
                else
                {
                    // SPI Write
                    _spi.Write(data);
                    Log($"[SPI WR] Data:{BitConverter.ToString(data)}");
                }
            }
            catch (Exception ex)
            {
                Log($"[Error] Write Failed: {ex.Message}");
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
                return;

            try
            {
                int len = (int)numReadLen.Value;
                byte[] result = null;

                if (cmbProtocol.SelectedItem.ToString() == "I2C")
                {
                    byte addr = ParseByte(txtSlaveAddr.Text);

                    // (옵션) Write 후 Read가 필요하다면 WriteData 칸의 내용을 먼저 씀
                    if (!string.IsNullOrWhiteSpace(txtWriteData.Text))
                    {
                        byte[] wData = ParseHex(txtWriteData.Text);
                        // Stop 없이 Write (Restart 조건)
                        _i2c.Write(addr, wData, stop: false);
                    }

                    // I2C Read
                    result = new byte[len];
                    _i2c.Read(addr, result, 1000); // Timeout 1000ms
                    Log($"[I2C RD] Addr:0x{addr:X2}, Len:{len} -> {BitConverter.ToString(result)}");
                }
                else
                {
                    // SPI Read (보통 Dummy 데이터를 보내면서 읽음)
                    // 순수 Read만 필요하다면 Read 메서드 호출, Write+Read라면 WriteRead 호출
                    _spi.Read(result);
                    Log($"[SPI RD] Len:{len} -> {BitConverter.ToString(result)}");
                }
            }
            catch (Exception ex)
            {
                Log($"[Error] Read Failed: {ex.Message}");
            }
        }

        // ================= Helpers =================

        private void UpdateUiState()
        {
            bool isI2c = cmbProtocol.SelectedItem.ToString() == "I2C";
            txtSlaveAddr.Enabled = !(_isConnected) && isI2c; // 연결 전 설정, 혹은 연결 후에도 변경 가능하게 하려면 로직 수정
            cmbSpiMode.Enabled = !(_isConnected) && !isI2c;
            // 기타 컨트롤 활성화/비활성화 처리...
        }

        private void Log(string msg)
        {
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            rtbLog.ScrollToCaret();
        }

        private byte[] ParseHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();

            // 공백, 콤마, 0x 제거
            hex = hex.Replace(" ", "").Replace(",", "").Replace("0x", "").Replace("0X", "");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private byte ParseByte(string hex)
        {
            hex = hex.Trim().Replace("0x", "").Replace("0X", "");
            return byte.Parse(hex, NumberStyles.HexNumber);
        }
    }
}