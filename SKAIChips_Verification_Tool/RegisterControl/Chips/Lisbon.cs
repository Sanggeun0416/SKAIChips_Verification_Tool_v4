using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Lisbon : ProjectBase
    {
        public override string Name => "Lisbon";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Lisbon",
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.I2C };

        public override uint ComFrequency => 400;
        public override byte DeviceAddress => 0x3C;

        public Lisbon()
        {
        }

        public Lisbon(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return null;
            //return new[]
            //{
            //    new TestSlotAction("EFUSE_W", () => Task.Run(() => WriteEfuseData())),
            //    new TestSlotAction("EFUSE_R", () => Task.Run(() => ReadEfuseData())),
            //};
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte>();

            sendData.Add((byte)(address & 0xff));
            sendData.Add((byte)(data & 0xff));
            I2cBus.Write(DeviceAddress, sendData.ToArray());
            sendData.Clear();
        }

        public override uint ReadRegister(uint address)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte>();
            byte[] rcvBuf = new byte[1];
            uint result = 0xFF;

            sendData.Add((byte)(address & 0xff));
            I2cBus.Write(DeviceAddress, sendData.ToArray(), false);
            I2cBus.Read(DeviceAddress, rcvBuf, 1000);
            sendData.Clear();
            result = (uint)(rcvBuf[0] & 0xFF);

            return result;
        }

        private uint ReadRegister3Byte(uint address)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte>();
            byte[] rcvBuf = new byte[3];
            uint result = 0xFFFFFF;

            sendData.Add((byte)(address & 0xff));
            I2cBus.Write(DeviceAddress, sendData.ToArray(), false);
            I2cBus.Read(DeviceAddress, rcvBuf, 1000);
            sendData.Clear();
            result = (uint)((rcvBuf[2] & 0xFF) | ((rcvBuf[1] << 8) & 0xFF) | ((rcvBuf[0] << 16) & 0xFF));

            return result;
        }

        private byte[] ShowRegisterGridDialog(byte[] defaultData)
        {
            Form prompt = new Form()
            {
                Width = 650,
                Height = 500,
                Text = "eFuse Register Map Input",
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            FlowLayoutPanel flowPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                Height = 400,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            List<TextBox> textBoxList = new List<TextBox>();

            for (int i = 0; i < 33; i++)
            {
                Panel itemPanel = new Panel()
                {
                    Width = 80,
                    Height = 50,
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                Label lbl = new Label()
                {
                    Text = $"REG{i:X2}",
                    Location = new Point(5, 5),
                    Width = 70,
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    ForeColor = Color.Gray
                };

                TextBox txt = new TextBox()
                {
                    Text = $"{defaultData[i]:X2}",
                    Location = new Point(15, 22),
                    Width = 50,
                    MaxLength = 2,
                    TextAlign = HorizontalAlignment.Center,
                    CharacterCasing = CharacterCasing.Upper
                };

                txt.Tag = i;

                itemPanel.Controls.Add(lbl);
                itemPanel.Controls.Add(txt);

                flowPanel.Controls.Add(itemPanel);
                textBoxList.Add(txt);
            }

            Button btnOk = new Button() { Text = "Write", Left = 420, Top = 420, Width = 100, Height = 30, DialogResult = DialogResult.OK };
            Button btnCancel = new Button() { Text = "Cancel", Left = 530, Top = 420, Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(flowPanel);
            prompt.Controls.Add(btnOk);
            prompt.Controls.Add(btnCancel);
            prompt.AcceptButton = btnOk;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                byte[] resultData = new byte[33];
                try
                {
                    for (int i = 0; i < 33; i++)
                    {
                        string hexInput = textBoxList[i].Text.Trim();

                        if (string.IsNullOrEmpty(hexInput))
                            resultData[i] = 0x00;
                        else
                            resultData[i] = Convert.ToByte(hexInput, 16);
                    }
                    return resultData;
                }
                catch
                {
                    MessageBox.Show("올바르지 않은 Hex 값이 포함되어 있습니다. (00~FF 사이 입력)", "입력 오류");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        [ChipTest("MANUAL", "Write Efuse Data", "Write EFUSE Data.")]
        private async Task WriteEfuseData(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            byte[] initialData = new byte[33];
            for (int i = 0; i < 32; i++)
            {
                initialData[i] = (byte)ReadRegister((uint)i);
                await Task.Delay(1);
            }
            initialData[32] = 0xCA;

            byte[] writeData = ShowRegisterGridDialog(initialData);

            if (writeData == null)
            {
                MessageBox.Show("작업이 취소되었습니다.");
                return;
            }
            if (writeData.Length != 33)
            {
                Array.Resize(ref writeData, 33);
            }

            WriteRegister(0x27, 0x414 & 0xFF); // Addr Low (0x414 & 0xFF)
            WriteRegister(0x28, (0x414 >> 8) & 0xFF); // Addr High (0x414 >> 8)
            WriteRegister(0x29, 0x04); // Data

            WriteRegister(0x27, 0x415 & 0xFF); // Addr Low (0x414 & 0xFF)
            WriteRegister(0x28, (0x415 >> 8) & 0xFF); // Addr High (0x414 >> 8)
            WriteRegister(0x29, 0x08); // Data

            WriteRegister(0x27, 0x416 & 0xFF); // Addr Low (0x414 & 0xFF)
            WriteRegister(0x28, (0x416 >> 8) & 0xFF); // Addr High (0x414 >> 8)
            WriteRegister(0x29, 0x00); // Data

            WriteRegister(0x27, 0x420 & 0xFF); // Addr Low (0x420 & 0xFF)
            WriteRegister(0x28, (0x420 >> 8) & 0xFF); // Addr High (0x420 >> 8)
            WriteRegister(0x29, 0x15); // Data

            WriteRegister(0x27, 0x421 & 0xFF); // Addr Low (0x421 & 0xFF)
            WriteRegister(0x28, (0x421 >> 8) & 0xFF); // Addr High (0x421 >> 8) & 0xFF
            WriteRegister(0x29, 0xA5); // Data

            InitProgress(33);

            for (int address = 0; address < 33; address++)
            {
                ct.ThrowIfCancellationRequested();

                AppendLog("INFO", $"Fusing Address = 0x{address:X2}");

                byte targetValue = writeData[address];

                for (int bit = 0; bit < 8; bit++)
                {
                    byte data0x27 = (byte)(0x00 | ((bit << 7) & 0x80) | (address & 0x7F));
                    byte data0x28 = (byte)(0x00 | ((bit >> 1) & 0x03));
                    uint data = 0;

                    if (((targetValue >> bit) & 0x01) == 1)
                    {
                        data = 1;
                    }

                    WriteRegister(0x27, data0x27);
                    WriteRegister(0x28, data0x28);
                    WriteRegister(0x29, data); // Data
                }
                Step();
            }

            ReportProgress(100);
        }

        [ChipTest("MANUAL", "Read Efuse Data", "Read EFUSE Data.")]
        private async Task ReadEfuseData(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            //uint rcvData = new byte();
            //WriteRegister(0x27, 0x414 & 0xFF); // Addr Low (0x414 & 0xFF)
            //WriteRegister(0x28, (0x414 >> 8) & 0xFF); // Addr High (0x414 >> 8)
            //rcvData = ReadRegister3Byte(0x29); // Data
            //AppendLog("INFO", $"0x27 - 0x{0x414 & 0xFF:X2}");
            //AppendLog("INFO", $"0x28 - 0x{(0x414 >> 8) & 0xFF:X2}");
            //AppendLog("INFO", $"0x29 - 0x{rcvData:X2}");

            //rcvData = new byte();
            //WriteRegister(0x27, 0x415 & 0xFF); // Addr Low (0x415 & 0xFF)
            //WriteRegister(0x28, (0x415 >> 8) & 0xFF); // Addr High (0x415 >> 8) & 0xFF
            //rcvData = ReadRegister3Byte(0x29); // Data
            //AppendLog("INFO", $"0x27 - 0x{0x415 & 0xFF:X2}");
            //AppendLog("INFO", $"0x28 - 0x{(0x415 >> 8) & 0xFF:X2}");
            //AppendLog("INFO", $"0x29 - 0x{rcvData:X2}");

            //rcvData = new byte();
            //WriteRegister(0x27, 0x416 & 0xFF); // Addr Low (0x416 & 0xFF)
            //WriteRegister(0x28, (0x416 >> 8) & 0xFF); // Addr High (0x416 >> 8) & 0xFF
            //rcvData = ReadRegister3Byte(0x29); // Data
            //AppendLog("INFO", $"0x27 - 0x{0x416 & 0xFF:X2}");
            //AppendLog("INFO", $"0x28 - 0x{(0x416 >> 8) & 0xFF:X2}");
            //AppendLog("INFO", $"0x29 - 0x{rcvData:X2}");

            //rcvData = new byte();
            //WriteRegister(0x27, 0x420 & 0xFF); // Addr Low (0x420 & 0xFF)
            //WriteRegister(0x28, (0x420 >> 8) & 0xFF); // Addr High (0x420 >> 8)
            //rcvData = ReadRegister3Byte(0x29); // Data
            //AppendLog("INFO", $"0x27 - 0x{0x420 & 0xFF:X2}");
            //AppendLog("INFO", $"0x28 - 0x{(0x420 >> 8) & 0xFF:X2}");
            //AppendLog("INFO", $"0x29 - 0x{rcvData:X2}");

            //rcvData = new byte();
            //WriteRegister(0x27, 0x421 & 0xFF); // Addr Low (0x421 & 0xFF)
            //WriteRegister(0x28, (0x421 >> 8) & 0xFF); // Addr High (0x421 >> 8) & 0xFF
            //rcvData = ReadRegister3Byte(0x29); // Data
            //AppendLog("INFO", $"0x27 - 0x{0x421 & 0xFF:X2}");
            //AppendLog("INFO", $"0x28 - 0x{(0x421 >> 8) & 0xFF:X2}");
            //AppendLog("INFO", $"0x29 - 0x{rcvData:X2}");

            byte[] readBuffer = new byte[33];
            InitProgress(33);

            for (int address = 0; address < 33; address++)
            {
                ct.ThrowIfCancellationRequested();

                byte data0x27 = (byte)(0x00 | (address & 0x7F));
                byte data0x28 = (byte)(0x00);

                WriteRegister(0x27, data0x27);
                WriteRegister(0x28, data0x28);

                uint readVal = ReadRegister3Byte(0x29);
                readBuffer[address] = (byte)readVal;
                AppendLog("INFO", $"ReadEfuse: 0x{address:X2} = 0x{readBuffer[address]:X2}");

                Step();
            }

            ReportProgress(100);
        }
    }
}