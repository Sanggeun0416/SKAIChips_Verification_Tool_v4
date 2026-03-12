using Microsoft.VisualBasic.Logging;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class IRIS2 : ProjectBase
    {
        public override string Name => "IRIS2";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "IRIS2",
            "SCP1502"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.SPI, ProtocolRegLogType.I2C };

        public override uint ComFrequency => 400;
        public override byte DeviceAddress => 0x3B;

        public IRIS2()
        {
        }

        public IRIS2(ISpiBus bus) : base(bus) { }

        public IRIS2(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return null;
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (SpiBus != null && SpiBus.IsConnected)
            {
                byte[] Bytes = new byte[3];
                uint rwFlag = 1;

                Bytes[0] = (byte)((rwFlag << 7) | (address & 0x7F));
                Bytes[1] = (byte)((data >> 8) & 0xff);
                Bytes[2] = (byte)((data >> 0) & 0xff);

                SpiBus.Write(Bytes, true);
            }
            else if (I2cBus != null && I2cBus.IsConnected)
            {
                List<byte> sendData = new List<byte>();

                sendData.Add((byte)(address & 0xff));
                sendData.Add((byte)(data & 0xff));
                I2cBus.Write(DeviceAddress, sendData.ToArray());
                sendData.Clear();
            }
            else
            {
                throw new InvalidOperationException("Bus (SPI or I2C) is not connected.");
            }
        }

        public override uint ReadRegister(uint address)
        {
            if (SpiBus != null && SpiBus.IsConnected)
            {
                uint rwFlag = 0;
                uint Data = 0xFFFF;
                byte[] Bytes = new byte[1];
                byte[] Buff = new byte[2];

                Bytes[0] = (byte)((rwFlag << 7) | (address & 0x7F));

                SpiBus.Transfer(Bytes, Buff, true);

                Data = (uint)((Buff[0] << 8) | (Buff[1] << 0));

                return Data;
            }
            else if (I2cBus != null && I2cBus.IsConnected)
            {
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
            else
            {
                throw new InvalidOperationException("Bus (SPI or I2C) is not connected.");
            }
        }

        [ChipTest("AUTO", "BGR SWEEP", "Auto Sweep BGR Trim Bits.")]
        private async Task AutoSweepBgr(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            DialogResult dr = ShowMsg(
                "TC Trim까지 모두 Sweep 하시겠습니까?\n\n'예'를 누르면 bgr_tc_trim<3:0>, bgr_trim<5:0> 전체 Sweep을 진행하며," +
                "\n'아니요'를 누르면 현재 설정된 bgr_tc_trim<3:0> 값에서 bgr_trim<5:0>만 Sweep 합니다.",
                "Sweep 범위 설정",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (dr == DialogResult.Cancel)
            {
                AppendLog("STOP", "테스트가 사용자에 의해 취소되었습니다.");
                return;
            }

            bool sweepTcAll = (dr == DialogResult.Yes);

            CheckInstruments("DigitalMultimeter0");

            IReportSheet bgrSheet;
            string time = DateTime.Now.ToString("HHmmss");
            bgrSheet = ctx.Report.CreateSheet($"{time}_BGR_SWEEP");
            bgrSheet.SetSheetFont("Consolas", 11);

            bgrSheet.Write(1, 1, $"bgr_tc_trim<3:0>");
            bgrSheet.Write(1, 2, $"bgr_trim<5:0>");
            bgrSheet.Write(1, 3, $"bgr_out[mV]");
            bgrSheet.SetAlignmentCenter(1, 1, 1, 3);
            bgrSheet.AutoFit();

            uint startTc = 0;
            uint endTc = 16;

            if (!sweepTcAll)
            {
                startTc = 0;
                endTc = startTc + 1;
            }

            int rowOffset = 2;

            WriteRegister(0x02, 0x30);

            for (uint tc = startTc; tc < endTc; tc++)
            {
                uint tc_3_2 = (tc >> 2) & 0x03;
                uint tc_1_0 = tc & 0x03;

                for (uint trim = 0; trim < 64; trim++)
                {
                    ct.ThrowIfCancellationRequested();

                    WriteRegister(0x04, tc_3_2);
                    WriteRegister(0x05, 0x00 | (tc_1_0 << 6) | trim);
                    await Task.Delay(100, ct);

                    double voltage = double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000;

                    bgrSheet.Write(rowOffset, 1, $"{tc}");
                    bgrSheet.Write(rowOffset, 2, $"{trim}");
                    bgrSheet.Write(rowOffset, 3, $"{Math.Round(voltage, 5)}");

                    rowOffset++;
                }
            }
        }

        [ChipTest("AUTO", "VREF ALDO SWEEP", "Auto Sweep VREF ALDO Trim Bits.")]
        private async Task AutoSweepVrefAldo(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            CheckInstruments("DigitalMultimeter0");

            string time = DateTime.Now.ToString("HHmmss");
            IReportSheet aldoSheet = ctx.Report.CreateSheet($"{time}_ALDO_REFGEN");
            aldoSheet.SetSheetFont("Consolas", 11);

            var VREF_ALDO_TRIM = _regCont.RegMgr.GetRegisterItem(this, "vref_aldo_trim<3:0>");

            aldoSheet.Write(1, 1, $"vref_aldo_trim<3:0>");
            aldoSheet.Write(1, 2, $"aldo_out[mV]");
            aldoSheet.SetAlignmentCenter(1, 1, 1, 2);
            aldoSheet.AutoFit();

            for (uint trim = 0; trim < 16; trim++)
            {
                ct.ThrowIfCancellationRequested();

                VREF_ALDO_TRIM.Write(trim);
                await Task.Delay(100, ct);

                double voltage = double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000;

                int rowOffset = 2 + (int)trim;
                aldoSheet.Write(rowOffset, 1, $"{trim}");
                aldoSheet.Write(rowOffset, 2, $"{Math.Round(voltage, 5)}");
            }
        }

        [ChipTest("AUTO", "VREF DLDO SWEEP", "Auto Sweep VREF DLDO Trim Bits.")]
        private async Task AutoSweepVrefDldo(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            CheckInstruments("DigitalMultimeter0");

            string time = DateTime.Now.ToString("HHmmss");
            IReportSheet dldoSheet = ctx.Report.CreateSheet($"{time}_DLDO_REFGEN");
            dldoSheet.SetSheetFont("Consolas", 11);

            var vref_dldo_trim_3 = _regCont.RegMgr.GetRegisterItem(this, "vref_dldo_trim<3>");
            var vref_dldo_trim_2_0 = _regCont.RegMgr.GetRegisterItem(this, "vref_dldo_trim<2:0>");

            dldoSheet.Write(1, 1, $"vref_dldo_trim<3:0>");
            dldoSheet.Write(1, 2, $"dldo_out[mV]");
            dldoSheet.SetAlignmentCenter(1, 1, 1, 2);
            dldoSheet.AutoFit();

            for (uint trim = 0; trim < 16; trim++)
            {
                ct.ThrowIfCancellationRequested();

                uint fine_3 = (trim >> 3) & 0x01;
                uint fine_2_0 = trim & 0x07;

                vref_dldo_trim_3.Write(fine_3);
                vref_dldo_trim_2_0.Write(fine_2_0);
                await Task.Delay(100, ct);

                double voltage = double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000;

                int rowOffset = 2 + (int)trim;
                dldoSheet.Write(rowOffset, 1, $"{trim}");
                dldoSheet.Write(rowOffset, 2, $"{Math.Round(voltage, 5)}");
            }
        }

        [ChipTest("AUTO", "ALDO SWEEP", "Auto Sweep ALDO Trim Bits.")]
        private async Task AutoSweepAldo(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            CheckInstruments("DigitalMultimeter0");

            IReportSheet bgrSheet;
            string time = DateTime.Now.ToString("HHmmss");
            bgrSheet = ctx.Report.CreateSheet($"{time}_ALDO_SWEEP");
            bgrSheet.SetSheetFont("Consolas", 11);

            bgrSheet.Write(1, 1, $"aldo_lv_trim<1:0>");
            bgrSheet.Write(1, 2, $"aldo_trim<3:0>");
            bgrSheet.Write(1, 3, $"aldo_out[mV]");
            bgrSheet.SetAlignmentCenter(1, 1, 1, 3);
            bgrSheet.AutoFit();

            int rowOffset = 2;

            for (uint coarse = 0; coarse < 4; coarse++)
            {
                for (uint fine = 0; fine < 16; fine++)
                {
                    ct.ThrowIfCancellationRequested();

                    uint fine_3_2 = (fine >> 2) & 0x03;
                    uint fine_1_0 = fine & 0x03;

                    WriteRegister(0x0A, fine_3_2);
                    WriteRegister(0x0B, 0x00 | (fine_1_0 << 6) | (coarse << 4));
                    await Task.Delay(100, ct);

                    double voltage = double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000;

                    bgrSheet.Write(rowOffset, 1, $"{coarse}");
                    bgrSheet.Write(rowOffset, 2, $"{fine}");
                    bgrSheet.Write(rowOffset, 3, $"{Math.Round(voltage, 5)}");

                    rowOffset++;
                }
            }
        }

        [ChipTest("AUTO", "Chamber Test", "Start Chamber Test.")]
        private async Task AutoChamberTest(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            CheckInstruments("TempChamber0", "PowerSupply0", "DigitalMultimeter0");

            string time = DateTime.Now.ToString("HHmmss");
            IReportSheet dldoSheet = ctx.Report.CreateSheet($"{time}_ChamberTest");
            dldoSheet.SetSheetFont("Consolas", 11);

            var vref_dldo_trim_3 = _regCont.RegMgr.GetRegisterItem(this, "vref_dldo_trim<3>");
            var vref_dldo_trim_2_0 = _regCont.RegMgr.GetRegisterItem(this, "vref_dldo_trim<2:0>");

            dldoSheet.Write(1, 1, $"vref_dldo_trim<3:0>");
            dldoSheet.Write(1, 2, $"dldo_out[mV]");
            dldoSheet.SetAlignmentCenter(1, 1, 1, 2);
            dldoSheet.AutoFit();

            double targetVoltage = 1200.0;
            uint bestTrim = 0;
            double minDifference = double.MaxValue;

            for (uint trim = 0; trim < 16; trim++)
            {
                ct.ThrowIfCancellationRequested();

                uint fine_3 = (trim >> 3) & 0x01;
                uint fine_2_0 = trim & 0x07;

                vref_dldo_trim_3.Write(fine_3);
                vref_dldo_trim_2_0.Write(fine_2_0);
                await Task.Delay(100, ct);

                double voltage = Math.Round(double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000, 5);

                int rowOffset = 2 + (int)trim;
                dldoSheet.Write(rowOffset, 1, $"{trim}");
                dldoSheet.Write(rowOffset, 2, $"{voltage}");

                double diff = Math.Abs(voltage - targetVoltage);
                if (diff < minDifference)
                {
                    minDifference = diff;
                    bestTrim = trim;
                }
            }

            uint best_fine_3 = (bestTrim >> 3) & 0x01;
            uint best_fine_2_0 = bestTrim & 0x07;

            vref_dldo_trim_3.Write(best_fine_3);
            vref_dldo_trim_2_0.Write(best_fine_2_0);
            await Task.Delay(100, ct);

            dldoSheet.Write(19, 1, "Best Trim:");
            dldoSheet.Write(19, 2, $"{bestTrim}");

            double[] tempsToTest = [85, 25, -40];
            for (int cycle = 0; cycle < tempsToTest.Length; cycle++)
            {
                ct.ThrowIfCancellationRequested();

                await SetlingChamber(tempsToTest[cycle], ct);

                double voltage = double.Parse(Inst("DigitalMultimeter0").Query(":MEAS:VOLT:DC?")) * 1000;
            }
        }

        private async Task SetlingChamber(double targetTemp, CancellationToken ct, double margin = 0.1, int stableTimeSecond = 60)
        {
            ct.ThrowIfCancellationRequested();

            int stableInterval = 1000;

            CheckInstruments("TempChamber0");

            AppendLog("INFO", $"Setting chamber to {targetTemp} °C. Waiting for stabilization...");
            Inst("TempChamber0").Write($"01,TEMP,S{targetTemp}");
            await Task.Delay(1000, ct);

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                string response = Inst("TempChamber0").Query(":TEMP?");
                string[] arrBuf = response.Split(new char[] { ',' });

                if (arrBuf.Length < 4)
                {
                    AppendLog($"ERROR", $"Unexpected response from TempChamber: {response}");
                    continue;
                }

                double[] dVal = new double[5];
                for (int split = 0; split < 4; split++)
                    dVal[split] = double.Parse(arrBuf[split]);

                AppendLog("INFO", $"RealTemp : {dVal[0].ToString()} | TargetTemp : {dVal[1].ToString()}");

                if (Math.Round(Math.Abs(dVal[0] - targetTemp), 1) <= margin)
                {
                    AppendLog("INFO", "Temperature reached. Soaking for 60 seconds to confirm stability...");
                    await Task.Delay(60000, ct);

                    response = Inst("TempChamber0").Query(":TEMP?");
                    arrBuf = response.Split(new char[] { ',' });

                    if (arrBuf.Length < 4)
                    {
                        AppendLog($"ERROR", $"Unexpected response from TempChamber: {response}");
                        continue;
                    }

                    dVal = new double[5];
                    for (int split = 0; split < 4; split++)
                        dVal[split] = double.Parse(arrBuf[split]);

                    if (Math.Round(Math.Abs(dVal[0] - targetTemp), 1) <= margin)
                    {
                        AppendLog("INFO", "Done SetTemp! Temperature is stable. Final setling for 1 minuite.");
                        await Task.Delay(60000, ct);
                        break;
                    }
                    else
                    {
                        AppendLog("INFO", $"Temperature drifted (Real: {dVal[0]}). Resuming polling...");
                        await Task.Delay(stableInterval, ct);
                    }
                }
                else
                {
                    await Task.Delay(stableInterval, ct);
                }

            }
        }
    }
}