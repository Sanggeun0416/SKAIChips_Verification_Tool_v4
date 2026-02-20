using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Okinawa : ProjectBase
    {
        public override string Name => "Okinawa";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Okinawa",
            "SCH1511",
            "STD1402Q"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.I2C };

        public override uint ComFrequency => 100;
        public override byte DeviceAddress => 0x29;

        public Okinawa()
        {
        }

        public Okinawa(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return new[]
            {
                new TestSlotAction("E/W_0", () => Task.Run(() => WriteFlash_0x00())),
                new TestSlotAction("E/W_1", () => Task.Run(() => WriteFlash_0xFF())),
                new TestSlotAction("E/W_D", () => Task.Run(() => WriteFlash_Default())),
                new TestSlotAction("GPIO_FT4222", () => Task.Run(() => TestGpio_FT4222())),
                new TestSlotAction("GPIO_ACBUS", () => Task.Run(() => TestGpio_ACBUS())),
                new TestSlotAction("GPIO_ADBUS", () => Task.Run(() => TestGpio_ADBUS()))
            };
        }

        private async Task TestGpio_FT4222()
        {
            if (!(_bus is IGpioController gpio))
            {
                MessageBox.Show("Current Bus does not support GPIO Control.");
                return;
            }

            AppendLog("INFO", "=== Start FT4222 GPIO Test (0~3) ===");

            for (int i = 0; i <= 3; i++)
            {
                try
                {
                    AppendLog("INFO", $"[GPIO {i}] Set Output -> High");
                    gpio.SetGpioDirection(i, true); // Output
                    gpio.SetGpioValue(i, true);     // High
                    await Task.Delay(100);

                    AppendLog("INFO", $"[GPIO {i}] Set Low");
                    gpio.SetGpioValue(i, false);    // Low
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    AppendLog("ERROR", $"GPIO {i} Fail: {ex.Message}");
                }
            }
            AppendLog("INFO", "=== End FT4222 GPIO Test ===");
        }

        private async Task TestGpio_ACBUS()
        {
            if (!(_bus is IGpioController gpio))
            {
                MessageBox.Show("Current Bus does not support GPIO Control.");
                return;
            }

            AppendLog("INFO", "=== Start UM232H ACBUS Test (0~7) ===");

            for (int i = 0; i <= 7; i++)
            {
                try
                {
                    AppendLog("INFO", $"[ACBUS {i}] Set Output -> High");
                    gpio.SetGpioDirection(i, true); // Output
                    gpio.SetGpioValue(i, true);     // High
                    await Task.Delay(50);

                    AppendLog("INFO", $"[ACBUS {i}] Set Low");
                    gpio.SetGpioValue(i, false);    // Low
                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    AppendLog("ERROR", $"ACBUS {i} Fail: {ex.Message}");
                }
            }
            AppendLog("INFO", "=== End ACBUS Test ===");
        }

        private async Task TestGpio_ADBUS()
        {
            if (!(_bus is IGpioController gpio))
            {
                MessageBox.Show("Current Bus does not support GPIO Control.");
                return;
            }

            AppendLog("INFO", "=== Start UM232H ADBUS Test (4~7) ===");

            for (int i = 8; i <= 11; i++)
            {
                int adbusNum = i - 8 + 4;
                try
                {
                    AppendLog("INFO", $"[ADBUS {adbusNum}] Set Output -> Low");
                    gpio.SetGpioDirection(i, true); // Output
                    gpio.SetGpioValue(i, false);
                    await Task.Delay(1000);

                    AppendLog("INFO", $"[ADBUS {adbusNum}] Set High");
                    gpio.SetGpioValue(i, true);
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    AppendLog("ERROR", $"ADBUS {adbusNum} Fail: {ex.Message}");
                }
            }
            AppendLog("INFO", "=== End ADBUS Test ===");
        }

        private void CommandEraseFlash()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte> { 0x00, 0x10, 0x00, 0x00 };
            I2cBus.Write(DeviceAddress, sendData.ToArray());
            Thread.Sleep(300);
        }

        private void CommandWriteFlash()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte> { 0x00, 0x12, 0x00, 0x00 };
            I2cBus.Write(DeviceAddress, sendData.ToArray());
            Thread.Sleep(150);
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            string sheetName = string.IsNullOrEmpty(this.CurrentSheetName) ? GetSheetNameByAddress(address) : this.CurrentSheetName;
            List<byte> sendData = new List<byte>();

            switch (sheetName)
            {
                case "Flash":
                    CommandEraseFlash();

                    sendData.Add((byte)((address >> 8) & 0xff));
                    sendData.Add((byte)(address & 0xff));
                    sendData.Add((byte)((data >> 8) & 0xff));
                    sendData.Add((byte)(data & 0xff));
                    I2cBus.Write(DeviceAddress, sendData.ToArray());
                    Thread.Sleep(5);

                    CommandWriteFlash();
                    break;

                default:
                    sendData.Add((byte)((address >> 8) & 0xff));
                    sendData.Add((byte)(address & 0xff));
                    sendData.Add((byte)((data >> 8) & 0xff));
                    sendData.Add((byte)(data & 0xff));
                    I2cBus.Write(DeviceAddress, sendData.ToArray());
                    break;
            }
        }

        public override uint ReadRegister(uint address)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> sendData = new List<byte> { 0x00, 0x11, 0x00, 0x00 };
            I2cBus.Write(DeviceAddress, sendData.ToArray());
            Thread.Sleep(5);

            sendData.Clear();
            sendData.Add(0x00);
            sendData.Add(0x20);
            sendData.Add(0x00);
            if (address >= 0x30 && address <= 0x37)
                sendData.Add((byte)(address - 0x2f));
            else
                sendData.Add((byte)address);

            I2cBus.Write(DeviceAddress, sendData.ToArray(), false);

            byte[] rcvBuf = new byte[2];
            I2cBus.Read(DeviceAddress, rcvBuf, 200);
            return (uint)((rcvBuf[0] << 8) | rcvBuf[1]);
        }

        private void WriteFlash_0x00()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            if (Ui(() => MessageBox.Show("Write Flash All to 0x00.", "Flash Write", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes))
                return;

            try
            {
                InitProgress(0x37 - 0x30 + 1);
                CommandEraseFlash();
                List<byte> sendData = new List<byte>();

                for (int address = 0x30; address <= 0x37; address++)
                {
                    sendData.Add((byte)((address >> 8) & 0xff));
                    sendData.Add((byte)(address & 0xff));
                    sendData.Add(0x00);
                    sendData.Add(address == 0x37 ? (byte)0x80 : (byte)0x00);

                    I2cBus.Write(DeviceAddress, sendData.ToArray());
                    sendData.Clear();
                    Thread.Sleep(10);
                    Step();
                }
                CommandWriteFlash();
                ReportProgress(100);
            }
            catch (Exception ex)
            {
                Ui(() => MessageBox.Show($"Error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        }

        private void WriteFlash_0xFF()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            if (Ui(() => MessageBox.Show("Write Flash All to 0xFF.", "Flash Write", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes))
                return;

            try
            {
                InitProgress(0x37 - 0x30 + 1);
                CommandEraseFlash();
                List<byte> sendData = new List<byte>();

                for (int address = 0x30; address <= 0x37; address++)
                {
                    sendData.Add((byte)((address >> 8) & 0xff));
                    sendData.Add((byte)(address & 0xff));
                    sendData.Add(0x00);
                    sendData.Add(0xFF);

                    I2cBus.Write(DeviceAddress, sendData.ToArray());
                    sendData.Clear();
                    Thread.Sleep(10);
                    Step();
                }
                CommandWriteFlash();
                ReportProgress(100);
            }
            catch (Exception ex)
            {
                Ui(() => MessageBox.Show($"Error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        }

        private void WriteFlash_Default()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            List<byte> flashData = new List<byte> { 0x9A, 0x54, 0x96, 0x06, 0x59, 0xC4, 0x20, 0xA0 };
            string confirmationMessage = "Check Flash Parameters.\n\n";
            for (int addr = 0x30; addr <= 0x37; addr++)
            {
                confirmationMessage += $"0x{Convert.ToString(addr, 16).ToUpper()}: 0x{Convert.ToString(flashData[addr - 0x30], 16).ToUpper()}\n";
            }
            if (Ui(() => MessageBox.Show(confirmationMessage, "Flash Write", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes))
                return;

            try
            {
                InitProgress(0x37 - 0x30 + 1);
                CommandEraseFlash();
                List<byte> sendData = new List<byte>();

                for (int address = 0x30; address <= 0x37; address++)
                {
                    sendData.Add((byte)((address >> 8) & 0xff));
                    sendData.Add((byte)(address & 0xff));
                    sendData.Add(0x00);
                    sendData.Add(flashData[address - 0x30]);

                    I2cBus.Write(DeviceAddress, sendData.ToArray());
                    sendData.Clear();
                    Thread.Sleep(5);
                    Step();
                }
                CommandWriteFlash();
                ReportProgress(100);
            }
            catch (Exception ex)
            {
                Ui(() => MessageBox.Show($"Error : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
        }

        [ChipTest("AUTO", "Version Select", "Start Version Select Test Sequence.")]
        private async Task RunVersionSelectTest(RunTestContext ctx, CancellationToken ct)
        {
            if (PowerSupply0 == null || DigitalMultimeter0 == null)
                throw new InvalidOperationException("PowerSupply0 or DMM0 is not connected.");

            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("Current Bus does not support GPIO Control.");

            IReportSheet verSheet;
            int y_pos = 2;

            try
            {
                verSheet = ctx.Report.SelectSheet("VER_SEL");
                while (true)
                {
                    var val = verSheet.Read(y_pos, 1);
                    if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                        break;
                    y_pos++;
                }
            }
            catch
            {
                verSheet = ctx.Report.CreateSheet("VER_SEL");
                verSheet.SetSheetFont("Consolas", 10);
                verSheet.Write(1, 1, "No.");
                verSheet.Write(1, 2, "Version");
                verSheet.Write(1, 3, "Measure[V]");
                verSheet.Write(1, 4, "Version");
                verSheet.Write(1, 5, "Count");

                for (int i = 1; i <= 14; i++)
                {
                    verSheet.Write(1 + i, 4, $"V{i}");
                    verSheet.Write(1 + i, 5, "0");
                    if (i == 14)
                    {
                        verSheet.Write(1 + i + 1, 4, $"Fail");
                        verSheet.Write(1 + i + 1, 5, "0");
                    }
                }
                verSheet.SetBorderAll(1, 4, 16, 5);
                verSheet.SetAlignmentCenterAll();
                verSheet.AutoFit();
            }

            PowerSupply0.Write("OUTP OFF,(@1:3)");
            await Task.Delay(100, ct);

            PowerSupply0.Write("VOLT 1.8,(@1)"); // VPP
            PowerSupply0.Write("VOLT 5.0,(@2)"); // VREF CP_EN
            PowerSupply0.Write("VOLT 8.0,(@3)"); // VCC
            await Task.Delay(100, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioValue(8, false);

            if (ShowMsg("CH1 : VPP = 1.8V\nCH2 : VREF CP_EN = 5V\nCH3 : VCC = 8V\n\n장비 설정을 확인해주세요.",
                "Version Select Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
            {
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                if (ShowMsg("새로운 칩을 넣고 [확인]을 눌러주세요.\r\n[취소]를 누르면 테스트를 종료합니다.",
                    "Ready to Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    break;
                }

                AppendLog("INFO", $"Testing Chip #{y_pos - 1}...");
                verSheet.Write(y_pos, 1, (y_pos - 1).ToString());

                try
                {
                    PowerSupply0.Write("OUTP ON,(@3)"); // VCC
                    await Task.Delay(500, ct);

                    PowerSupply0.Write("OUTP ON,(@1)"); // VPP
                    await Task.Delay(1000, ct);

                    PowerSupply0.Write("OUTP ON,(@2)"); // VREF CP_EN
                    await Task.Delay(500, ct);

                    gpio.SetGpioValue(8, true);     // High
                    await Task.Delay(500, ct);

                    string resp = DigitalMultimeter0.Query(":MEAS:VOLT:DC?");
                    if (!double.TryParse(resp, out double VRT_VAL))
                    {
                        VRT_VAL = 0.0;
                        AppendLog("ERROR", "Invalid DMM Response");
                    }

                    await Task.Delay(500, ct);

                    PowerSupply0.Write("OUTP OFF,(@2)"); // VREF
                    PowerSupply0.Write("OUTP OFF,(@1)"); // VPP
                    PowerSupply0.Write("OUTP OFF,(@3)"); // VCC

                    gpio.SetGpioValue(8, false); // Low
                    await Task.Delay(100, ct);

                    string version;
                    if (VRT_VAL > 2.8 && VRT_VAL < 2.9)
                        version = "V1";
                    else if (VRT_VAL > 2.65 && VRT_VAL < 2.75)
                        version = "V2";
                    else if (VRT_VAL > 2.5 && VRT_VAL < 2.6)
                        version = "V3";
                    else if (VRT_VAL > 2.35 && VRT_VAL < 2.45)
                        version = "V4";
                    else if (VRT_VAL > 2.2 && VRT_VAL < 2.3)
                        version = "V5";
                    else if (VRT_VAL > 2.05 && VRT_VAL < 2.15)
                        version = "V6";
                    else if (VRT_VAL > 1.9 && VRT_VAL < 2.0)
                        version = "V7";
                    else if (VRT_VAL > 1.75 && VRT_VAL < 1.85)
                        version = "V8";
                    else if (VRT_VAL > 1.6 && VRT_VAL < 1.7)
                        version = "V9";
                    else if (VRT_VAL > 1.45 && VRT_VAL < 1.55)
                        version = "V10";
                    else if (VRT_VAL > 1.3 && VRT_VAL < 1.4)
                        version = "V11";
                    else if (VRT_VAL > 1.15 && VRT_VAL < 1.25)
                        version = "V12";
                    else if (VRT_VAL > 1.0 && VRT_VAL < 1.1)
                        version = "V13";
                    else if (VRT_VAL > 0.85 && VRT_VAL < 0.95)
                        version = "V14";
                    else
                        version = "Fail";

                    verSheet.Write(y_pos, 2, version);
                    verSheet.Write(y_pos, 3, VRT_VAL.ToString("F4"));

                    AppendLog("INFO", $"Result: {version} ({VRT_VAL:F4}V)");
                    y_pos++;
                }
                catch (Exception ex)
                {
                    AppendLog("ERROR", $"Test Failed: {ex.Message}");
                    try
                    {
                        PowerSupply0.Write("OUTP OFF,(@1:3)");
                    }
                    catch { }
                    if (ShowMsg($"Error occurred: {ex.Message}\nContinue?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes)
                    {
                        break;
                    }
                }
            }
        }

        private async Task RunCalFs5vAdimFsfTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null ||
                DigitalMultimeter0 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments (PS0-2, DMM0, OSC0, SG0) must be connected.");

            if (_regCont?.RegMgr == null)
                throw new InvalidOperationException("Register Map is not loaded.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");
            if (I2cBus == null)
                throw new InvalidOperationException("I2cBus is not connected.");

            int actualNum;
            IReportSheet calSheet;

            try
            {
                calSheet = ctx.Report.SelectSheet("CAL_VAL");
            }
            catch
            {
                throw new InvalidOperationException("Sheet not found name of 'CAL_VAL'");
            }

            if (!num.HasValue)
            {
                string inputStr = "";
                Ui(() => { inputStr = Microsoft.VisualBasic.Interaction.InputBox("Enter the Start Num in Decimal.", "CAL FS5V/ADIM/FSF", "1"); });
                if (!int.TryParse(inputStr, out actualNum))
                    throw new InvalidOperationException("Invalid start num.");
            }
            else
                actualNum = num.Value;

            var FS5V = _regCont.RegMgr.GetRegisterItem(this, "FS5V[5:0]");
            var ADIM1H = _regCont.RegMgr.GetRegisterItem(this, "ADIM1H[4:0]");
            var ADIM1L = _regCont.RegMgr.GetRegisterItem(this, "ADIM1L[4:0]");
            var FSF = _regCont.RegMgr.GetRegisterItem(this, "FSF[3:0]");

            double VREF_Target = 5;
            double ADIMH_Target = 1.32;
            double ADIM_Min = 0, ADIM_Max = 0, ADIM_CUNT = 0;
            double ADIML_Target = 0.528;
            double FSF_Target = 10;

            uint val_fs5v = 0, val_ADIMH = 0, val_ADIML = 0, val_FSF = 0;
            int y_pos = actualNum;
            double VREF = 0, CS_Voltage = 0, GATE_ON = 0, OFF_TIME = 0;

            PowerSupply0.Write("OUTP OFF,(@2:3)");
            PowerSupply1.Write("OUTP OFF,(@2:3)");
            PowerSupply2.Write("OUTP OFF,(@1:3)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, true);
            gpio.SetGpioValue(8, false);
            gpio.SetGpioValue(9, false);

            while (!ct.IsCancellationRequested)
            {
                List<byte> SendBytes = new List<byte>();
                if (ShowMsg("새로운 칩을 넣고 확인을 눌러주세요.\r\n", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    y_pos++;
                else
                    return;

                PowerSupply0.Write("VOLT 13.0,(@2)");
                PowerSupply2.Write("VOLT 5.5,(@3)");
                PowerSupply0.Write("VOLT 0.2,(@3)");
                PowerSupply1.Write("VOLT 1.5,(@2)");
                PowerSupply1.Write("VOLT 3.3,(@3)");
                PowerSupply2.Write("VOLT 5.0,(@1)");
                await Task.Delay(10, ct);

                PowerSupply0.Write("OUTP ON,(@2:3)");
                PowerSupply1.Write("OUTP ON,(@2:3)");
                PowerSupply2.Write("OUTP ON,(@1:3)");
                await Task.Delay(100, ct);

                gpio.SetGpioValue(8, false);
                gpio.SetGpioValue(9, true);

                PowerSupply2.Write("VOLT 8.75,(@3)");
                await Task.Delay(500, ct);

                CommandEraseFlash();
                await Task.Delay(300, ct);

                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(500, ct);

                for (int Address = 0x30; Address <= 0x37; Address++)
                {
                    SendBytes.Add(0x00);
                    SendBytes.Add((byte)Address);
                    SendBytes.Add(0x00);
                    SendBytes.Add(Address == 0x37 ? (byte)0x80 : (byte)0x00);
                    I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                    SendBytes.Clear();
                    await Task.Delay(2, ct);
                }

                SendBytes.AddRange(new byte[] { 0x00, 0x12, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(110, ct);

                for (uint i = 0x01; i <= 0x08; i++)
                    ReadRegister(i);

                PowerSupply2.Write("VOLT 8.75,(@3)");
                await Task.Delay(500, ct);

                SendBytes.AddRange(new byte[] { 0x00, 0x10, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(500, ct);

                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(500, ct);

                byte[] flashDefaults = { 0x9F, 0x54, 0x8D, 0x82, 0x68, 0xC4, 0x24, 0xB0 };
                for (int Address = 0x30; Address <= 0x37; Address++)
                {
                    SendBytes.AddRange(new byte[] { 0x00, (byte)Address, 0x00, flashDefaults[Address - 0x30] });
                    I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                    SendBytes.Clear();
                    await Task.Delay(2, ct);
                }

                SendBytes.AddRange(new byte[] { 0x00, 0x12, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(110, ct);

                for (uint i = 0x01; i <= 0x08; i++)
                    ReadRegister(i);

                await Task.Delay(100, ct);

                val_fs5v = 31;
                FS5V.Value = val_fs5v;
                FS5V.Write();

                await Task.Delay(100, ct);
                VREF = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
                double VREF_Delta = VREF_Target - VREF;

                calSheet.Write(3, y_pos, VREF.ToString("F3"));

                for (int i = 5; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();

                    if (VREF_Delta < -0.05)
                        val_fs5v += 5;
                    else if (VREF_Delta < -0.04)
                        val_fs5v += 4;
                    else if (VREF_Delta < -0.03)
                        val_fs5v += 3;
                    else if (VREF_Delta < -0.02)
                        val_fs5v += 2;
                    else if (VREF_Delta < -0.01)
                        val_fs5v += 1;
                    else if (VREF_Delta < 0.01)
                        val_fs5v += 0;
                    else if (VREF_Delta < 0.02)
                        val_fs5v -= 1;
                    else if (VREF_Delta < 0.03)
                        val_fs5v -= 2;
                    else if (VREF_Delta < 0.04)
                        val_fs5v -= 3;
                    else if (VREF_Delta < 0.05)
                        val_fs5v -= 4;
                    else if (VREF_Delta >= 0.05)
                        val_fs5v -= 5;

                    FS5V.Value = val_fs5v;
                    FS5V.Write();
                    FS5V.Read();

                    await Task.Delay(100, ct);
                    VREF = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
                    VREF_Delta = VREF_Target - VREF;

                    if (VREF_Delta >= -0.01 && VREF_Delta < 0.01)
                        break;
                }

                calSheet.Write(1, y_pos, (y_pos - 1).ToString());
                calSheet.Write(2, y_pos, val_fs5v.ToString());
                calSheet.Write(4, y_pos, VREF.ToString("F4"));

                PowerSupply0.Write("VOLT 0.2,(@3)");
                PowerSupply1.Write("VOLT 3.3,(@3)");
                await Task.Delay(10, ct);
                PowerSupply0.Write("VOLT 0.0,(@3)");
                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(10, ct);

                OscilloScope0.Write(":TIM:SCAL 5E-3");
                OscilloScope0.Write(":RUN");
                await Task.Delay(100, ct);

                gpio.SetGpioValue(8, false);
                gpio.SetGpioValue(9, true);

                val_FSF = 2;
                FSF.Value = val_FSF;
                FSF.Write();
                PowerSupply2.Write("VOLT 1.8,(@3)");
                await Task.Delay(10, ct);

                OFF_TIME = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+3;
                double FSF_Delta = FSF_Target - OFF_TIME;

                calSheet.Write(12, y_pos, OFF_TIME.ToString("F3"));

                for (int i = 5; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();

                    if (FSF_Delta < -0.45)
                        val_FSF -= 2;
                    else if (FSF_Delta < -0.25)
                        val_FSF -= 1;
                    else if (FSF_Delta < 0.25)
                        val_FSF -= 0;
                    else if (FSF_Delta < 0.45)
                        val_FSF += 1;
                    else if (FSF_Delta >= 0.45)
                        val_FSF += 2;

                    gpio.SetGpioValue(8, false);
                    gpio.SetGpioValue(9, true);
                    PowerSupply2.Write("VOLT 5.5,(@3)");
                    await Task.Delay(10, ct);
                    FSF.Value = val_FSF;
                    FSF.Write();
                    PowerSupply2.Write("VOLT 1.8,(@3)");
                    await Task.Delay(10, ct);

                    OFF_TIME = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+3;
                    FSF_Delta = FSF_Target - OFF_TIME;

                    if (FSF_Delta >= -0.25 && FSF_Delta < 0.25)
                        break;
                }

                calSheet.Write(11, y_pos, val_FSF.ToString());
                calSheet.Write(13, y_pos, OFF_TIME.ToString("F4"));

                PowerSupply1.Write("VOLT 3.3,(@3)");
                PowerSupply0.Write("VOLT 0,(@3)");

                SignalGenerator0.Write("SOURce1:FUNC SQU");
                SignalGenerator0.Write("SOURce1:VOLTage 0.5");
                SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.25");
                SignalGenerator0.Write("SOURce1:FREQ 10000");
                SignalGenerator0.Write("OUTP:STAT ON");
                OscilloScope0.Write(":TIM:SCAL 2E-3");

                await Task.Delay(1000, ct);

                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(10, ct);
                gpio.SetGpioValue(8, false);
                gpio.SetGpioValue(9, true);
                await Task.Delay(10, ct);

                val_ADIMH = 20;
                val_ADIML = 13;

                ADIM1H.Value = val_ADIMH;
                ADIM1H.Write();
                ADIM1L.Value = val_ADIML;
                ADIM1L.Write();

                for (uint i = 0x01; i <= 0x08; i++)
                    ReadRegister(i);
                AppendLog("INFO", ADIM1H.Value.ToString() + " 1H\t1L " + ADIM1L.Value.ToString());

                gpio.SetGpioValue(8, true);
                gpio.SetGpioValue(9, false);
                await Task.Delay(100, ct);

                PowerSupply0.Write("VOLT 1.35,(@3)");
                PowerSupply1.Write("VOLT 3.3,(@3)");
                PowerSupply2.Write("VOLT 1.8,(@3)");
                await Task.Delay(10, ct);

                for (CS_Voltage = 1.4; CS_Voltage >= 1.1; CS_Voltage -= 0.002)
                {
                    ct.ThrowIfCancellationRequested();
                    PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                    await Task.Delay(100, ct);
                    ADIM_Min = 65535;
                    ADIM_Max = 400;

                    for (int j = 0; j < 10; j++)
                    {
                        OscilloScope0.Write(":SING");
                        await Task.Delay(300, ct);
                        GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                        if (ADIM_Min > GATE_ON)
                            ADIM_Min = GATE_ON;
                        if (ADIM_Max < GATE_ON)
                            ADIM_Max = GATE_ON;

                        if (GATE_ON > 600)
                        {
                            ADIM_Min = 0;
                            break;
                        }
                        if (GATE_ON > 10)
                            ADIM_CUNT += 1;
                        else if (GATE_ON < 10)
                            break;

                        if (ADIM_CUNT > 5)
                            break;
                    }

                    await Task.Delay(10, ct);
                    if ((ADIM_Max - ADIM_Min) < 4)
                        break;
                    if (GATE_ON > 10 && ADIM_CUNT > 5)
                        break;
                }

                double ADIMH_Delta = ADIMH_Target - CS_Voltage;
                ADIM_CUNT = 0;

                calSheet.Write(6, y_pos, CS_Voltage.ToString("F4"));
                calSheet.Write(14, y_pos, GATE_ON.ToString("F4"));

                for (int i = 5; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();
                    if (ADIMH_Delta < -0.0344)
                        val_ADIMH -= 5;
                    else if (ADIMH_Delta < -0.0274)
                        val_ADIMH -= 4;
                    else if (ADIMH_Delta < -0.0204)
                        val_ADIMH -= 3;
                    else if (ADIMH_Delta < -0.0134)
                        val_ADIMH -= 2;
                    else if (ADIMH_Delta < -0.0064)
                        val_ADIMH -= 1;
                    else if (ADIMH_Delta < 0.0064)
                        val_ADIMH -= 0;
                    else if (ADIMH_Delta < 0.0134)
                        val_ADIMH += 1;
                    else if (ADIMH_Delta < 0.0204)
                        val_ADIMH += 2;
                    else if (ADIMH_Delta < 0.0274)
                        val_ADIMH += 3;
                    else if (ADIMH_Delta < 0.0344)
                        val_ADIMH += 4;
                    else if (ADIMH_Delta >= 0.0344)
                        val_ADIMH += 5;
                    else if (ADIMH_Delta >= -0.0064 && ADIMH_Delta < 0.0064)
                        break;

                    PowerSupply2.Write("VOLT 5.5,(@3)");
                    await Task.Delay(10, ct);
                    PowerSupply1.Write("VOLT 3.3,(@3)");
                    PowerSupply0.Write("VOLT 0,(@3)");

                    gpio.SetGpioValue(8, false);
                    gpio.SetGpioValue(9, true);
                    await Task.Delay(10, ct);

                    ADIM1H.Value = val_ADIMH;
                    ADIM1H.Write();
                    for (uint addr = 0x01; addr <= 0x08; addr++)
                        ReadRegister(addr);

                    gpio.SetGpioValue(9, false);
                    gpio.SetGpioValue(8, true);
                    await Task.Delay(10, ct);

                    PowerSupply2.Write("VOLT 1.8,(@3)");
                    await Task.Delay(10, ct);
                    PowerSupply1.Write("VOLT 3.3,(@3)");
                    await Task.Delay(10, ct);

                    for (CS_Voltage = 1.4; CS_Voltage >= 1.1; CS_Voltage -= 0.002)
                    {
                        ct.ThrowIfCancellationRequested();
                        PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                        await Task.Delay(10, ct);

                        ADIM_Min = 65535;
                        ADIM_Max = 400;
                        for (int j = 0; j < 10; j++)
                        {
                            OscilloScope0.Write(":SING");
                            await Task.Delay(300, ct);
                            GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                            if (ADIM_Min > GATE_ON)
                                ADIM_Min = GATE_ON;
                            if (ADIM_Max < GATE_ON)
                                ADIM_Max = GATE_ON;

                            if (GATE_ON > 600)
                            {
                                ADIM_Min = 0;
                                break;
                            }
                            if (GATE_ON > 10)
                                ADIM_CUNT += 1;
                            else if (GATE_ON < 10)
                                break;

                            if (ADIM_CUNT > 5)
                                break;
                        }

                        calSheet.Write(15, y_pos, GATE_ON.ToString("F4"));
                        await Task.Delay(10, ct);
                        if ((ADIM_Max - ADIM_Min) < 4)
                            break;
                        if (GATE_ON > 10 && ADIM_CUNT > 5)
                            break;
                    }

                    ADIMH_Delta = ADIMH_Target - CS_Voltage;
                    ADIM_CUNT = 0;
                    if (ADIMH_Delta >= -0.0064 && ADIMH_Delta < 0.0064)
                        break;
                }

                calSheet.Write(5, y_pos, val_ADIMH.ToString());
                calSheet.Write(7, y_pos, CS_Voltage.ToString("F4"));
                calSheet.Write(16, y_pos, GATE_ON.ToString("F4"));

                OscilloScope0.Write(":RUN");
                PowerSupply0.Write("VOLT 0.6,(@3)");
                PowerSupply1.Write("VOLT 0.0,(@3)");
                PowerSupply2.Write("VOLT 1.8,(@3)");
                await Task.Delay(10, ct);

                gpio.SetGpioValue(8, true);
                gpio.SetGpioValue(9, false);
                await Task.Delay(10, ct);

                for (CS_Voltage = 0.6; CS_Voltage >= 0.4; CS_Voltage -= 0.002)
                {
                    ct.ThrowIfCancellationRequested();
                    PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                    await Task.Delay(10, ct);

                    ADIM_Min = 65535;
                    ADIM_Max = 400;
                    for (int j = 0; j < 20; j++)
                    {
                        OscilloScope0.Write(":SING");
                        await Task.Delay(500, ct);
                        GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                        if (ADIM_Min > GATE_ON)
                            ADIM_Min = GATE_ON;
                        if (ADIM_Max < GATE_ON)
                            ADIM_Max = GATE_ON;

                        if (GATE_ON > 600)
                        {
                            ADIM_Min = 0;
                            break;
                        }
                        if (GATE_ON > 10)
                            ADIM_CUNT += 1;
                        else if (GATE_ON < 10)
                            break;

                        if (ADIM_CUNT > 5)
                            break;
                    }

                    calSheet.Write(15, y_pos, GATE_ON.ToString("F4"));
                    await Task.Delay(10, ct);
                    if ((ADIM_Max - ADIM_Min) < 15)
                        break;
                    if (GATE_ON > 10 && ADIM_CUNT > 5)
                        break;
                }
                double ADIML_Delta = ADIML_Target - CS_Voltage;
                ADIM_CUNT = 0;

                calSheet.Write(9, y_pos, CS_Voltage.ToString("F4"));

                for (int i = 6; i >= 0; i--)
                {
                    ct.ThrowIfCancellationRequested();
                    if (ADIML_Delta < -0.0154)
                        val_ADIML += 4;
                    else if (ADIML_Delta < -0.0114)
                        val_ADIML += 3;
                    else if (ADIML_Delta < -0.0074)
                        val_ADIML += 2;
                    else if (ADIML_Delta < -0.0034)
                        val_ADIML += 1;
                    else if (ADIML_Delta < 0.0034)
                        val_ADIML += 0;
                    else if (ADIML_Delta < 0.0074)
                        val_ADIML -= 1;
                    else if (ADIML_Delta < 0.0114)
                        val_ADIML -= 2;
                    else if (ADIML_Delta < 0.0154)
                        val_ADIML -= 3;
                    else if (ADIML_Delta >= 0.0154)
                        val_ADIML -= 4;
                    else if (ADIML_Delta >= -0.0034 && ADIML_Delta < 0.0034)
                        break;

                    PowerSupply2.Write("VOLT 5.5,(@3)");
                    await Task.Delay(10, ct);

                    gpio.SetGpioValue(8, false);
                    gpio.SetGpioValue(9, true);

                    ADIM1L.Value = val_ADIML;
                    ADIM1L.Write();
                    for (uint addr = 0x01; addr <= 0x08; addr++)
                        ReadRegister(addr);

                    gpio.SetGpioValue(8, true);
                    gpio.SetGpioValue(9, false);

                    PowerSupply2.Write("VOLT 1.8,(@3)");
                    await Task.Delay(10, ct);

                    for (CS_Voltage = 0.6; CS_Voltage >= 0.4; CS_Voltage -= 0.002)
                    {
                        ct.ThrowIfCancellationRequested();
                        PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                        await Task.Delay(10, ct);
                        ADIM_Min = 65535;
                        ADIM_Max = 400;
                        for (int j = 0; j < 20; j++)
                        {
                            OscilloScope0.Write(":SING");
                            await Task.Delay(500, ct);
                            GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                            if (ADIM_Min > GATE_ON)
                                ADIM_Min = GATE_ON;
                            if (ADIM_Max < GATE_ON)
                                ADIM_Max = GATE_ON;

                            if (GATE_ON > 600)
                            {
                                ADIM_Min = 0;
                                break;
                            }
                            if (GATE_ON > 10)
                                ADIM_CUNT += 1;
                            else if (GATE_ON < 10)
                                break;

                            if (ADIM_CUNT > 5)
                                break;
                        }

                        calSheet.Write(15, y_pos, GATE_ON.ToString("F4"));
                        await Task.Delay(10, ct);
                        if ((ADIM_Max - ADIM_Min) < 15)
                            break;
                        if (GATE_ON > 10 && ADIM_CUNT > 5)
                            break;
                    }
                    ADIML_Delta = ADIML_Target - CS_Voltage;
                    ADIM_CUNT = 0;

                    if ((ADIML_Delta >= -0.0034 && ADIML_Delta < 0.0034) || val_ADIML >= 30)
                        break;
                }

                calSheet.Write(8, y_pos, val_ADIML.ToString());
                calSheet.Write(10, y_pos, CS_Voltage.ToString("F4"));

                gpio.SetGpioValue(8, false);
                gpio.SetGpioValue(9, true);

                PowerSupply2.Write("VOLT 8.75,(@3)");
                await Task.Delay(500, ct);

                SendBytes.AddRange(new byte[] { 0x00, 0x10, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(300, ct);

                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(500, ct);

                for (int Address = 0x30; Address <= 0x37; Address++)
                {
                    SendBytes.AddRange(new byte[] { 0x00, (byte)Address, 0x00, Address == 0x37 ? (byte)0x80 : (byte)0x00 });
                    I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                    SendBytes.Clear();
                    await Task.Delay(2, ct);
                }

                SendBytes.AddRange(new byte[] { 0x00, 0x12, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(110, ct);

                for (uint i = 0x01; i <= 0x08; i++)
                    ReadRegister(i);

                PowerSupply2.Write("VOLT 8.75,(@3)");
                await Task.Delay(500, ct);
                SendBytes.AddRange(new byte[] { 0x00, 0x10, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(500, ct);

                PowerSupply2.Write("VOLT 5.5,(@3)");
                await Task.Delay(500, ct);
                for (int Address = 0x30; Address <= 0x37; Address++)
                {
                    SendBytes.AddRange(new byte[] { 0x00, (byte)Address, 0x00 });
                    switch (Address)
                    {
                        case 0x30:
                            SendBytes.Add((byte)(0x80 | val_fs5v));
                            break;
                        case 0x31:
                            SendBytes.Add((byte)(0x40 | val_ADIMH));
                            break;
                        case 0x32:
                            SendBytes.Add((byte)(0x80 | val_ADIML));
                            break;
                        case 0x33:
                            SendBytes.Add((byte)(0x00 | val_FSF));
                            break;
                        case 0x34:
                            SendBytes.Add(0x59);
                            break;
                        case 0x35:
                            SendBytes.Add(0x84);
                            break;
                        case 0x36:
                            SendBytes.Add(0x20);
                            break;
                        case 0x37:
                            SendBytes.Add(0xA0);
                            break;
                    }
                    I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                    SendBytes.Clear();
                    await Task.Delay(2, ct);
                }

                SendBytes.AddRange(new byte[] { 0x00, 0x12, 0x00, 0x00 });
                I2cBus.Write(DeviceAddress, SendBytes.ToArray(), true);
                SendBytes.Clear();
                await Task.Delay(110, ct);

                for (uint i = 0x01; i <= 0x08; i++)
                    ReadRegister(i);

                PowerSupply0.Write("OUTP OFF,(@2:3)");
                PowerSupply1.Write("OUTP OFF,(@2:3)");
                PowerSupply2.Write("OUTP OFF,(@1:3)");

                gpio.SetGpioValue(8, false);
                gpio.SetGpioValue(9, false);
            }
        }

        private async Task RunVccCurrentTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null)
                throw new InvalidOperationException("All required instruments (PS0-2) must be connected.");

            if (_regCont?.RegMgr == null)
                throw new InvalidOperationException("Register Map is not loaded.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int actualNum;
            if (!num.HasValue)
            {
                string inputStr = "";
                Ui(() => { inputStr = Microsoft.VisualBasic.Interaction.InputBox("Enter the Start Num in Decimal.", "VCC Current Test", "1"); });
                if (!int.TryParse(inputStr, out actualNum))
                    throw new InvalidOperationException("Invalid start num.");
            }
            else
                actualNum = num.Value;

            int y_pos = actualNum;

            PowerSupply0.Write("SENS:CURR:RANG 10E-3,(@2)");
            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 6.4,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 1.5,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);
            await Task.Delay(100, ct);

            double IST = double.Parse(PowerSupply0.Query("MEAS:CURR? (@2)")) * 1000000;
            await Task.Delay(500, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply0.Write("SENS:CURR:RANG AUTO,(@2)");
            await Task.Delay(500, ct);
            double IOP11 = double.Parse(PowerSupply0.Query("MEAS:CURR? (@2)")) * 1000;

            PowerSupply0.Write("VOLT 9.0,(@2)");
            await Task.Delay(500, ct);
            double IOP12 = double.Parse(PowerSupply0.Query("MEAS:CURR? (@2)")) * 1000;

            PowerSupply0.Write("VOLT 20.0,(@2)");
            await Task.Delay(500, ct);
            double IOP13 = double.Parse(PowerSupply0.Query("MEAS:CURR? (@2)")) * 1000;

            singleSheet.Write(2, y_pos, (y_pos - 7).ToString());
            singleSheet.Write(4, y_pos, IST.ToString("F4"));
            singleSheet.Write(5, y_pos, IOP11.ToString("F4"));
            singleSheet.Write(6, y_pos, IOP12.ToString("F4"));
            singleSheet.Write(7, y_pos, IOP13.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunVrefTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || ElectronicLoad == null || DigitalMultimeter0 == null)
                throw new InvalidOperationException("Check Instrument (PS0-2, DMM0, ELOAD) must be connected.");

            if (_regCont?.RegMgr == null)
                throw new InvalidOperationException("Register Map is not loaded.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");
            if (I2cBus == null)
                throw new InvalidOperationException("I2cBus is not connected.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int actualNum;
            if (!num.HasValue)
            {
                string inputStr = "";
                Ui(() => { inputStr = Microsoft.VisualBasic.Interaction.InputBox("Enter the Start Num in Decimal.", "VREF Test", "1"); });
                if (!int.TryParse(inputStr, out actualNum))
                    throw new InvalidOperationException("Invalid start num.");
            }
            else
                actualNum = num.Value;

            int y_pos = actualNum;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            ElectronicLoad.Write(":OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            ElectronicLoad.Write("FUNC CC");
            ElectronicLoad.Write("CURR 0.001");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(100, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VREF = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
            await Task.Delay(100, ct);

            PowerSupply0.Write("VOLT 20.0,(@2)");
            await Task.Delay(10, ct);
            double VREF_A = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
            await Task.Delay(100, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            await Task.Delay(10, ct);
            ElectronicLoad.Write(":OUTP:STAT ON");
            await Task.Delay(10, ct);

            double VREF_LOAD = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
            await Task.Delay(100, ct);

            double VLOAD = Math.Abs(VREF - VREF_LOAD);
            double VLINE = Math.Abs(VREF - VREF_A);

            singleSheet.Write(9, y_pos, VREF.ToString("F4"));
            singleSheet.Write(10, y_pos, VREF_A.ToString("F4"));
            singleSheet.Write(11, y_pos, VLINE.ToString("F4"));
            singleSheet.Write(12, y_pos, VLOAD.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            ElectronicLoad.Write(":OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        private async Task RunUvloTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null)
                throw new InvalidOperationException("All required instruments (PS0-2, OSC0) must be connected.");

            if (_regCont?.RegMgr == null)
                throw new InvalidOperationException("Register Map is not loaded.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int actualNum;
            if (!num.HasValue)
            {
                string inputStr = "";
                Ui(() => { inputStr = Microsoft.VisualBasic.Interaction.InputBox("Enter the Start Num in Decimal.", "UVLO Test", "1"); });
                if (!int.TryParse(inputStr, out actualNum))
                    throw new InvalidOperationException("Invalid start num.");
            }
            else
                actualNum = num.Value;

            int y_pos = actualNum;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 7.3,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VCC_Voltage = 0;
            for (VCC_Voltage = 7.3; VCC_Voltage < 9.3; VCC_Voltage += 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {VCC_Voltage},(@2)");
                await Task.Delay(10, ct);
                double GATE_High = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_High > 0.4)
                    break;
            }
            double VSTH = VCC_Voltage;

            for (VCC_Voltage = VSTH; VCC_Voltage >= 6.5; VCC_Voltage -= 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {VCC_Voltage},(@2)");
                await Task.Delay(10, ct);
                double GATE_Low = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_Low < 0.1)
                    break;
            }
            double VSTL = VCC_Voltage;

            singleSheet.Write(13, y_pos, VSTH.ToString("F4"));
            singleSheet.Write(14, y_pos, VSTL.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunVpwmTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null)
                throw new InvalidOperationException("All required power supplies (PS0-2) must be connected.");
            if (OscilloScope0 == null)
                throw new InvalidOperationException("Oscilloscope must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int actualNum;
            if (!num.HasValue)
            {
                string inputStr = "";
                Ui(() => { inputStr = Microsoft.VisualBasic.Interaction.InputBox("Enter the Start Num in Decimal.", "VPWM TEST", "1"); });
                if (!int.TryParse(inputStr, out actualNum))
                    throw new InvalidOperationException("Invalid start num.");
            }
            else
                actualNum = num.Value;

            int y_pos = actualNum;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double pwmVoltage = 0, gateHigh = 0;
            for (pwmVoltage = 1.16; pwmVoltage < 1.84; pwmVoltage += 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply2.Write($"VOLT {pwmVoltage:F3},(@1)");
                await Task.Delay(10, ct);

                gateHigh = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (gateHigh > 1)
                    break;
            }

            double vGh = double.Parse(OscilloScope0.Query(":MEAS:VTOP? CHAN1"));
            double vPwm = pwmVoltage;

            double gateLow = 0;
            for (pwmVoltage = vPwm; pwmVoltage >= 0.6; pwmVoltage -= 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply2.Write($"VOLT {pwmVoltage:F3},(@1)");
                await Task.Delay(10, ct);

                gateLow = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (gateLow < 1)
                    break;
            }

            double vGl = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
            if (vGl < 1)
                vGl = 0;

            double vPwmHy = vPwm - pwmVoltage;

            singleSheet.Write(16, y_pos, vPwm.ToString("F4"));
            singleSheet.Write(17, y_pos, vPwmHy.ToString("F4"));
            singleSheet.Write(18, y_pos, vGh.ToString("F4"));
            singleSheet.Write(19, y_pos, vGl.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunScpThresholdTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");

            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF ");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:FREQ 10000");
            SignalGenerator0.Write("SOURce1:VOLTage 1.5");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.75");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double CS_Voltage = 0, OFF_TIME = 0;
            for (CS_Voltage = 1.5; CS_Voltage < 2.16; CS_Voltage += 0.01)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(10, ct);
                OFF_TIME = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1"));
                if (OFF_TIME > 2)
                    break;
            }

            singleSheet.Write(20, y_pos, CS_Voltage.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF ");
            await Task.Delay(1, ct);
        }

        private async Task RunCsShortProtTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF ");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double CS_Voltage = 0, ON_TIME = 0;
            for (CS_Voltage = 0.2; CS_Voltage >= 0; CS_Voltage -= 0.005)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(10, ct);
                ON_TIME = double.Parse(OscilloScope0.Query(":MEAS:PWIDth? CHAN1")) * 1E+6;
                if (ON_TIME < 15)
                    break;
            }

            double VCSRST = CS_Voltage;

            PowerSupply0.Write("VOLT 0,(@3)");
            await Task.Delay(10, ct);

            ON_TIME = double.Parse(OscilloScope0.Query(":MEAS:PWIDth? CHAN1")) * 1E+6;
            double TCSMNT = ON_TIME;

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioValue(8, false);
            gpio.SetGpioDirection(9, false);

            OscilloScope0.Write(":TIM:SCAL 5E-3");
            await Task.Delay(100, ct);

            double OFF_TIME = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+3;
            double TCSRST = OFF_TIME;

            singleSheet.Write(21, y_pos, VCSRST.ToString("F4"));
            singleSheet.Write(22, y_pos, TCSMNT.ToString("F4"));
            singleSheet.Write(23, y_pos, TCSRST.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF ");
            await Task.Delay(1, ct);
        }

        private async Task RunMaxOnOffTimeTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            SignalGenerator0.Write("SOURce1:FUNC DC");
            SignalGenerator0.Write("SOURce1:VOLTage 0");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            OscilloScope0.Write(":TIM:SCAL 2E-5");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double OFF_TIME = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+6;
            double ON_TIME = double.Parse(OscilloScope0.Query(":MEAS:PWIDth? CHAN1")) * 1E+6;

            singleSheet.Write(24, y_pos, ON_TIME.ToString("F4"));
            singleSheet.Write(25, y_pos, OFF_TIME.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        private async Task RunFetDsShortProtTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 0.2,(@3)");

            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.5");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.25");
            SignalGenerator0.Write("SOURce1:FREQ 10000");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            PowerSupply0.Write("OUTP ON,(@3)");
            SignalGenerator0.Write("OUTP:STAT ON");

            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            OscilloScope0.Write(":TRIG:SOUR CHAN2");
            OscilloScope0.Write(":TRIG:LEV 5");
            OscilloScope0.Write(":TRIG:EDGE:SLOP NEG");

            double CS_Voltage = 0, FAIL_low = 0;
            for (CS_Voltage = 0.2; CS_Voltage < 0.7; CS_Voltage += 0.005)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(50, ct);
                FAIL_low = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN2"));
                if (FAIL_low < 9)
                    break;
            }
            double VPDS = CS_Voltage;

            OscilloScope0.Write(":TRIG:SOUR CHAN1");
            OscilloScope0.Write(":TRIG:LEV 5");
            OscilloScope0.Write(":TRIG:EDGE:SLOP NEG");

            singleSheet.Write(28, y_pos, VPDS.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunCsPinOpenTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("SENS:CURR:RANG 10E-3,(@3)");
            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-6");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            PowerSupply0.Write("OUTP ON,(@3)");
            await Task.Delay(1000, ct);
            PowerSupply0.Write("OUTP OFF,(@3)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VPCSO = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN3"));

            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            await Task.Delay(100, ct);

            double ICS = double.Parse(PowerSupply0.Query("MEAS:CURR? (@3)")) * 1000000;

            singleSheet.Write(29, y_pos, VPCSO.ToString("F4"));
            singleSheet.Write(30, y_pos, ICS.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunDsThresholdTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 1.8,(@3)");

            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.05");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.0025");
            SignalGenerator0.Write("SOURce1:FREQ 100000");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            OscilloScope0.Write(":TIM:SCAL 2E-5");
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);
            await Task.Delay(1000, ct);

            double GATE_H = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+6;
            await Task.Delay(10, ct);

            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.15");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.075");
            await Task.Delay(10, ct);

            double GATE_L = double.Parse(OscilloScope0.Query(":MEAS:NWIDth? CHAN1")) * 1E+6;

            if (GATE_H > 15 && GATE_L < 15)
                singleSheet.Write(36, y_pos, "0.2");
            else
                singleSheet.Write(36, y_pos, "0");

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        private async Task RunVdrvfbOvpTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 1E-3");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VDRVFB_Voltage = 0, GATE_Low = 0, GATE_High = 0;
            for (VDRVFB_Voltage = 2.0; VDRVFB_Voltage < 2.4; VDRVFB_Voltage += 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply1.Write($"VOLT {VDRVFB_Voltage},(@2)");
                await Task.Delay(50, ct);
                GATE_Low = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_Low < 1)
                    break;
            }

            double VDRVFBOVP = VDRVFB_Voltage;

            for (VDRVFB_Voltage = VDRVFBOVP; VDRVFB_Voltage >= 1.9; VDRVFB_Voltage -= 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply1.Write($"VOLT {VDRVFB_Voltage},(@2)");
                await Task.Delay(50, ct);
                GATE_High = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_High > 1)
                    break;
            }

            double VDRVFBOVPR = VDRVFB_Voltage;
            double VDOVPHY = VDRVFBOVP - VDRVFBOVPR;

            singleSheet.Write(38, y_pos, VDRVFBOVP.ToString("F4"));
            singleSheet.Write(39, y_pos, VDRVFBOVPR.ToString("F4"));
            singleSheet.Write(40, y_pos, VDOVPHY.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunVdrvfbUvpTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VDRVFB_Voltage = 0, GATE_Low = 0, GATE_High = 0;
            for (VDRVFB_Voltage = 1.3; VDRVFB_Voltage >= 0.9; VDRVFB_Voltage -= 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply1.Write($"VOLT {VDRVFB_Voltage},(@2)");
                await Task.Delay(50, ct);
                GATE_Low = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_Low < 1)
                    break;
            }

            double VDRVFBUVP = VDRVFB_Voltage;

            for (VDRVFB_Voltage = VDRVFBUVP; VDRVFB_Voltage < 1.4; VDRVFB_Voltage += 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply1.Write($"VOLT {VDRVFB_Voltage},(@2)");
                await Task.Delay(50, ct);
                GATE_High = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN1"));
                if (GATE_High > 1)
                    break;
            }

            double VDRVFBUVPR = VDRVFB_Voltage;
            double VDUVPHY = VDRVFBUVP - VDRVFBUVPR;

            singleSheet.Write(41, y_pos, VDRVFBUVP.ToString("F4"));
            singleSheet.Write(42, y_pos, VDRVFBUVPR.ToString("F4"));
            singleSheet.Write(43, y_pos, VDUVPHY.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunVccOvpTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            OscilloScope0.Write(":TIM:SCAL 2E-5");
            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double VCC_Voltage = 0, FAIL_Low = 0, FAIL_High = 0;
            for (VCC_Voltage = 14.7; VCC_Voltage < 16.5; VCC_Voltage += 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {VCC_Voltage},(@2)");
                await Task.Delay(50, ct);
                FAIL_Low = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN2"));
                if (FAIL_Low < 5)
                    break;
            }

            double VCCOVP = VCC_Voltage;

            for (VCC_Voltage = 8.0; VCC_Voltage >= 6.1; VCC_Voltage -= 0.02)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {VCC_Voltage},(@2)");
                await Task.Delay(50, ct);
                FAIL_High = double.Parse(OscilloScope0.Query(":MEAS:VAV? CHAN2"));
                if (FAIL_High > 5)
                    break;
            }

            double VCCOVPR = VCC_Voltage;

            singleSheet.Write(44, y_pos, VCCOVP.ToString("F4"));
            singleSheet.Write(45, y_pos, VCCOVPR.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);
        }

        private async Task RunStandbyModeTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || DigitalMultimeter0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("SENS:CURR:RANG 10E-3,(@2)");
            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply0.Write("VOLT 0.2,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            PowerSupply2.Write("VOLT 0.0,(@1)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            await Task.Delay(1000, ct);

            double iSTBY = double.Parse(PowerSupply0.Query("MEAS:CURR? (@2)")) * 1000000;
            await Task.Delay(10, ct);
            double VREFst = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?"));
            await Task.Delay(10, ct);

            singleSheet.Write(47, y_pos, iSTBY.ToString("F4"));
            singleSheet.Write(49, y_pos, VREFst.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
        }

        private async Task RunV2drefTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 1.35,(@3)");

            OscilloScope0.Write(":TIM:SCAL 2E-3");
            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.5");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.25");
            SignalGenerator0.Write("SOURce1:FREQ 10000");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);
            await Task.Delay(1000, ct);

            double CS_Voltage = 0, GATE_ON = 0, ADIM_Min = 0, ADIM_Max = 0;
            for (CS_Voltage = 1.35; CS_Voltage >= 1.29; CS_Voltage -= 0.002)
            {
                ct.ThrowIfCancellationRequested();
                ADIM_Min = 65535;
                ADIM_Max = 0;

                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(100, ct);
                for (int j = 0; j < 10; j++)
                {
                    OscilloScope0.Write(":SING");
                    await Task.Delay(100, ct);
                    GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                    if (ADIM_Min > GATE_ON)
                        ADIM_Min = GATE_ON;
                    if (ADIM_Max < GATE_ON)
                        ADIM_Max = GATE_ON;

                    if (GATE_ON > 600)
                    {
                        ADIM_Min = 0;
                        break;
                    }
                }

                if ((ADIM_Max - ADIM_Min) < 4)
                    break;
            }
            double V2DREF = CS_Voltage;

            singleSheet.Write(50, y_pos, V2DREF.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        private async Task RunV2dMinMaxTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 3.3,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 1.35,(@3)");

            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.5");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.25");
            SignalGenerator0.Write("SOURce1:FREQ 10000");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            await Task.Delay(1000, ct);
            gpio.SetGpioDirection(8, true);
            gpio.SetGpioDirection(9, false);

            double CS_Voltage = 0, GATE_ON = 0, ADIM_Min = 0, ADIM_Max = 0;
            for (CS_Voltage = 1.45; CS_Voltage >= 1.1; CS_Voltage -= 0.002)
            {
                ct.ThrowIfCancellationRequested();
                ADIM_Min = 65535;
                ADIM_Max = 400;
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(100, ct);
                for (int j = 0; j < 10; j++)
                {
                    OscilloScope0.Write(":SING");
                    await Task.Delay(100, ct);
                    GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                    if (ADIM_Min > GATE_ON)
                        ADIM_Min = GATE_ON;
                    if (ADIM_Max < GATE_ON)
                        ADIM_Max = GATE_ON;

                    if (GATE_ON > 600)
                    {
                        ADIM_Min = 0;
                        break;
                    }
                }

                if ((ADIM_Max - ADIM_Min) < 4)
                    break;
            }
            double V2DMAX = CS_Voltage;

            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            await Task.Delay(10, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioValue(8, false); // Floating
            gpio.SetGpioDirection(9, false);

            for (CS_Voltage = 0.65; CS_Voltage >= 0.4; CS_Voltage -= 0.002)
            {
                ct.ThrowIfCancellationRequested();
                ADIM_Min = 65535;
                ADIM_Max = 400;
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(10, ct);
                for (int j = 0; j < 10; j++)
                {
                    OscilloScope0.Write(":SING");
                    await Task.Delay(100, ct);
                    GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                    if (ADIM_Min > GATE_ON)
                        ADIM_Min = GATE_ON;
                    if (ADIM_Max < GATE_ON)
                        ADIM_Max = GATE_ON;

                    if (GATE_ON > 600)
                    {
                        ADIM_Min = 0;
                        break;
                    }
                }

                if ((ADIM_Max - ADIM_Min) < 15)
                    break;
            }
            double V2DMIN = CS_Voltage;

            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            await Task.Delay(10, ct);

            gpio.SetGpioValue(8, false);
            gpio.SetGpioDirection(9, false);

            for (CS_Voltage = 1.372; CS_Voltage >= 1.1; CS_Voltage -= 0.002)
            {
                ct.ThrowIfCancellationRequested();
                ADIM_Min = 65535;
                ADIM_Max = 400;
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(100, ct);
                for (int j = 0; j < 10; j++)
                {
                    OscilloScope0.Write(":SING");
                    await Task.Delay(100, ct);
                    GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                    if (ADIM_Min > GATE_ON)
                        ADIM_Min = GATE_ON;
                    if (ADIM_Max < GATE_ON)
                        ADIM_Max = GATE_ON;

                    if (GATE_ON > 600)
                    {
                        ADIM_Min = 0;
                        break;
                    }
                }

                if ((ADIM_Max - ADIM_Min) < 4)
                    break;
            }
            double VADIMO = CS_Voltage;

            OscilloScope0.Write(":TIM:SCAL 50E-6");
            await Task.Delay(100, ct);
            double TRISE = 0;
            for (int i = 0; i < 10; i++)
            {
                ct.ThrowIfCancellationRequested();
                OscilloScope0.Write(":SING");
                TRISE = double.Parse(OscilloScope0.Query(":MEAS:RIS? CHAN1")) * 1E+9;
                await Task.Delay(100, ct);

                if (TRISE > 115)
                    break;
            }

            OscilloScope0.Write(":TIM:SCAL 5E-6");
            double TFALL = 0;
            for (int i = 0; i < 10; i++)
            {
                ct.ThrowIfCancellationRequested();
                OscilloScope0.Write(":SING");
                TFALL = double.Parse(OscilloScope0.Query(":MEAS:FALL? CHAN1")) * 1E+9;
                await Task.Delay(100, ct);
                if (TFALL > 25)
                    break;
            }

            singleSheet.Write(51, y_pos, V2DMAX.ToString("F4"));
            singleSheet.Write(52, y_pos, V2DMIN.ToString("F4"));
            singleSheet.Write(53, y_pos, VADIMO.ToString("F4"));
            singleSheet.Write(56, y_pos, TRISE.ToString("F4"));
            singleSheet.Write(57, y_pos, TFALL.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        private async Task RunVadimoTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (PowerSupply0 == null || PowerSupply1 == null || PowerSupply2 == null || OscilloScope0 == null || SignalGenerator0 == null)
                throw new InvalidOperationException("All required instruments must be connected.");
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            IReportSheet singleSheet;
            try
            {
                singleSheet = ctx.Report.SelectSheet("SINGLE_TEST");
            }
            catch { throw new InvalidOperationException("Sheet not found name of 'SINGLE_TEST'"); }

            int y_pos = num ?? 1;

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);

            PowerSupply0.Write("VOLT 0.0,(@2)");
            PowerSupply2.Write("VOLT 0.0,(@3)");
            PowerSupply0.Write("VOLT 0.0,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@2)");
            PowerSupply1.Write("VOLT 0.0,(@3)");
            PowerSupply2.Write("VOLT 0.0,(@1)");
            await Task.Delay(10, ct);

            PowerSupply0.Write("VOLT 13.0,(@2)");
            PowerSupply2.Write("VOLT 1.8,(@3)");
            PowerSupply1.Write("VOLT 1.5,(@2)");
            PowerSupply1.Write("VOLT 5.0,(@3)");
            PowerSupply2.Write("VOLT 5.0,(@1)");
            PowerSupply0.Write("VOLT 1.35,(@3)");
            PowerSupply1.Write("VOLT 0.0,(@3)");

            SignalGenerator0.Write("SOURce1:FUNC SQU");
            SignalGenerator0.Write("SOURce1:VOLTage 0.5");
            SignalGenerator0.Write("SOURce1:VOLTage:OFFSet 0.25");
            SignalGenerator0.Write("SOURce1:FREQ 10000");
            await Task.Delay(10, ct);

            PowerSupply0.Write("OUTP ON,(@2)");
            PowerSupply2.Write("OUTP ON,(@3)");
            PowerSupply0.Write("OUTP ON,(@3)");
            PowerSupply1.Write("OUTP ON,(@2)");
            PowerSupply1.Write("OUTP ON,(@3)");
            PowerSupply2.Write("OUTP ON,(@1)");
            SignalGenerator0.Write("OUTP:STAT ON");
            await Task.Delay(1000, ct);

            gpio.SetGpioDirection(8, true);
            gpio.SetGpioValue(8, false);
            gpio.SetGpioDirection(9, false);

            double CS_Voltage = 0, GATE_ON = 0;
            for (CS_Voltage = 1.372; CS_Voltage >= 1.28; CS_Voltage -= 0.002)
            {
                ct.ThrowIfCancellationRequested();
                PowerSupply0.Write($"VOLT {CS_Voltage},(@3)");
                await Task.Delay(50, ct);
                GATE_ON = double.Parse(OscilloScope0.Query(":MEAS:PPUL? CHAN1"));

                if (GATE_ON >= 24 && GATE_ON <= 25)
                    break;
            }
            double VADIMO = CS_Voltage;

            singleSheet.Write(53, y_pos, VADIMO.ToString("F4"));

            PowerSupply0.Write("OUTP OFF,(@2)");
            PowerSupply2.Write("OUTP OFF,(@3)");
            PowerSupply0.Write("OUTP OFF,(@3)");
            PowerSupply1.Write("OUTP OFF,(@2)");
            PowerSupply1.Write("OUTP OFF,(@3)");
            PowerSupply2.Write("OUTP OFF,(@1)");
            SignalGenerator0.Write("OUTP:STAT OFF");
            await Task.Delay(1, ct);
        }

        [ChipTest("AUTO", "Single Test Sequence", "Run full automated test sequence.")]
        private async Task RunSingleTest(RunTestContext ctx, CancellationToken ct, int? num = null)
        {
            if (!(_bus is IGpioController gpio))
                throw new InvalidOperationException("GPIO Control is not supported.");

            int y_pos = (num ?? 1) + 6;

            while (!ct.IsCancellationRequested)
            {
                if (ShowMsg("새로운 칩을 넣고 확인을 눌러주세요.\r\n[취소]를 누르면 종료합니다.",
                    Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    y_pos++;
                }
                else
                {
                    return;
                }

                gpio.SetGpioDirection(8, true);
                gpio.SetGpioValue(8, false);
                gpio.SetGpioDirection(9, false);

                gpio.SetGpioValue(8, true);

                await RunVccCurrentTest(ctx, ct, y_pos);
                await RunVrefTest(ctx, ct, y_pos);
                await RunUvloTest(ctx, ct, y_pos);
                await RunVpwmTest(ctx, ct, y_pos);
                await RunScpThresholdTest(ctx, ct, y_pos);
                await RunCsShortProtTest(ctx, ct, y_pos);
                await RunMaxOnOffTimeTest(ctx, ct, y_pos);
                await RunFetDsShortProtTest(ctx, ct, y_pos);
                await RunCsPinOpenTest(ctx, ct, y_pos);
                await RunDsThresholdTest(ctx, ct, y_pos);
                await RunVdrvfbOvpTest(ctx, ct, y_pos);
                await RunVdrvfbUvpTest(ctx, ct, y_pos);
                await RunVccOvpTest(ctx, ct, y_pos);
                await RunStandbyModeTest(ctx, ct, y_pos);
                await RunV2drefTest(ctx, ct, y_pos);
                await RunV2dMinMaxTest(ctx, ct, y_pos);
            }
        }
    }
}