using SKAIChips_Verification_Tool.Instrument;
using System.Globalization;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Chicago : ProjectBase
    {
        public override string Name => "Chicago";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Chicago",
            "SCH1711"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.SPI };

        public override uint ComFrequency => 1000;

        private IChicagoSpiBus ChicagoBus => SpiBus as IChicagoSpiBus
            ?? throw new InvalidOperationException("Bus is not IChicagoSpiBus");

        public Chicago()
        {
        }

        public Chicago(ISpiBus bus) : base(bus)
        {
            if (bus is not IChicagoSpiBus)
                throw new InvalidOperationException("Chicago requires UM232H device (IChicagoSpiBus).");
        }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return new[]
            {
                new TestSlotAction("WriteSRAM", async () => await WriteSramWithHexForm()),
                new TestSlotAction("ReadSRAM", async () => await ReadSramWithHexForm())
            };
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            var bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC0;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            bytes[4] = (byte)((address >> 8) & 0xFF);
            bytes[5] = (byte)(address & 0xFF);
            bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
            ChicagoBus.ChicagoWrite(bytes);

            bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC1;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            bytes[4] = 0x00;
            bytes[5] = 0x01;
            bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
            ChicagoBus.ChicagoWrite(bytes);

            bytes = new byte[6];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC2;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            bytes[4] = (byte)(data & 0xFF);
            bytes[5] = bytes[4];
            ChicagoBus.ChicagoWrite(bytes);
        }

        public override uint ReadRegister(uint address)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            uint data = 0xFF;
            byte[] bytes;
            Span<byte> rx = stackalloc byte[3];

            switch (address)
            {
                case 0x4FE:
                case 0x4FF:
                    bytes = new byte[4];
                    bytes[0] = 0x5A;
                    bytes[1] = 0xFF;
                    bytes[2] = 0x07;
                    bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);

                    rx = stackalloc byte[3];
                    ChicagoSpiBus.ChicagoWriteRead(bytes, rx);

                    if (address == 0x4FE)
                        data = (uint)(rx[0] & 0xFF);
                    else
                        data = (uint)(rx[1] & 0xFF);
                    break;

                default:
                    bytes = new byte[7];
                    bytes[0] = 0x5A;
                    bytes[1] = 0xFF;
                    bytes[2] = 0xC0;
                    bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
                    bytes[4] = (byte)((address >> 8) & 0xFF);
                    bytes[5] = (byte)(address & 0xFF);
                    bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
                    ChicagoSpiBus.ChicagoWrite(bytes);

                    bytes = new byte[7];
                    bytes[0] = 0x5A;
                    bytes[1] = 0xFF;
                    bytes[2] = 0xC1;
                    bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
                    bytes[4] = 0x00;
                    bytes[5] = 0x01;
                    bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
                    ChicagoBus.ChicagoWrite(bytes);

                    bytes = new byte[4];
                    bytes[0] = 0x5A;
                    bytes[1] = 0xFF;
                    bytes[2] = 0xC3;
                    bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);

                    rx = stackalloc byte[3];
                    ChicagoSpiBus.ChicagoWriteRead(bytes, rx);

                    data = (uint)(rx[0] & 0xFF);
                    break;
            }

            return data;
        }

        private void WriteCommand(uint cmd)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            byte[] bytes = { 0x5A, 0xFF, (byte)(cmd & 0xFF), 0x00 };
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            ChicagoBus.ChicagoWrite(bytes);
        }

        private void WriteSRAM(uint Address, uint length, byte[] hexData)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            if (hexData == null || hexData.Length < length)
                return;

            byte[] bytes;
            bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC0;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            bytes[4] = (byte)((Address >> 8) & 0xFF);
            bytes[5] = (byte)((Address >> 0) & 0xFF);
            bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
            ChicagoSpiBus.ChicagoWrite(bytes);

            bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC1;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            bytes[4] = (byte)((length >> 8) & 0xFF);
            bytes[5] = (byte)((length >> 0) & 0xFF);
            bytes[6] = (byte)((bytes[4] + bytes[5]) & 0xFF);
            ChicagoSpiBus.ChicagoWrite(bytes);

            bytes = new byte[4];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC2;
            bytes[3] = (byte)((bytes[0] + bytes[1] + bytes[2]) & 0xFF);
            ChicagoSpiBus.ChicagoWrite(bytes);

            bytes = new byte[length + 1];
            Array.Copy(hexData, 0, bytes, 0, length);
            byte cksum = 0;
            for (int i = 0; i < length; i++)
                cksum += bytes[i];
            bytes[bytes.Length - 1] = cksum;
            ChicagoSpiBus.ChicagoWrite(bytes);
        }

        private byte[]? ReadSRAM(uint Address, uint length)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            if (length == 0 || length > 320)
                return null;

            byte[] bytes;
            uint cksum;
            bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC0;
            cksum = (uint)(bytes[0] + bytes[1] + bytes[2]);
            bytes[3] = (byte)(cksum & 0xFF);
            bytes[4] = (byte)((Address >> 8) & 0xFF);
            bytes[5] = (byte)(Address & 0xFF);
            cksum = (uint)(bytes[4] + bytes[5]);
            bytes[6] = (byte)(cksum & 0xFF);
            ChicagoSpiBus.ChicagoWrite(bytes);

            bytes = new byte[7];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC1;
            cksum = (uint)(bytes[0] + bytes[1] + bytes[2]);
            bytes[3] = (byte)(cksum & 0xFF);
            bytes[4] = (byte)((length >> 8) & 0xFF);
            bytes[5] = (byte)(length & 0xFF);
            cksum = (uint)(bytes[4] + bytes[5]);
            bytes[6] = (byte)(cksum & 0xFF);
            ChicagoSpiBus.ChicagoWrite(bytes);

            bytes = new byte[4];
            bytes[0] = 0x5A;
            bytes[1] = 0xFF;
            bytes[2] = 0xC3;
            cksum = (uint)(bytes[0] + bytes[1] + bytes[2]);
            bytes[3] = (byte)(cksum & 0xFF);
            Span<byte> rx = stackalloc byte[(int)length + 1];
            ChicagoSpiBus.ChicagoWriteRead(bytes, rx);
            if (rx == null || rx.Length < length)
                return null;

            byte[] payload = new byte[length];
            rx.Slice(0, (int)length).CopyTo(payload.AsSpan());
            return payload;
        }

        private void WriteLEDData(uint length, byte[] hexData)
        {
            if (ChicagoSpiBus == null)
                throw new InvalidOperationException("ChicagoSpiBus is not initialized.");

            if (length == 0)
                return;
            if (length > 320)
                length = 320;
            if (hexData == null || hexData.Length < length)
                return;

            byte[] header = new byte[4];
            header[0] = 0x5A;
            header[1] = 0xFF;
            header[2] = 0x02;
            header[3] = (byte)((header[0] + header[1] + header[2]) & 0xFF);
            SpiBus!.Write(header);

            byte[] bytes = new byte[length + 1];
            Array.Copy(hexData, 0, bytes, 0, length);

            byte cksum = 0;
            for (int i = 0; i < length; i++)
                cksum += bytes[i];
            bytes[bytes.Length - 1] = cksum;

            ChicagoSpiBus.ChicagoWrite(bytes);
        }

        private async Task WriteLedDataWithHexForm()
        {
            var dims = GetGridDimensions();
            if (dims.Item1 == 0)
            {
                MessageBox.Show("The calculated number of LEDs is zero. (SEG_MODE = 0)", "WriteLedData", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Ui(() =>
            {
                using var hexForm = new HexWriteForm(dims.Item1, dims.Item2);
                if (hexForm.ShowDialog(_regCont) == DialogResult.OK)
                    WriteLEDData((uint)dims.Item1, hexForm.ResultData);
            });
            await Task.CompletedTask;
        }

        private async Task WriteSramWithHexForm()
        {
            const int sramTotalBytes = 320;
            const int sramSegmentsPerGrid = 32;

            uint startAddress = uint.Parse(RegisterControlForm.Prompt.ShowDialog("Enter the Start Address in Decimal.", "Write SRAM"));
            if (startAddress is < 0x000 or > 0x3FF)
            {
                MessageBox.Show($"Address must be in 0x000 ~ 0x3FF.", "Write SRAM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Ui(() =>
            {
                using (var hexForm = new HexWriteForm(sramTotalBytes, sramSegmentsPerGrid))
                {
                    hexForm.Text = $"SRAM Write (Addr: 0x{startAddress:X4}, Len: {sramTotalBytes})";
                    if (hexForm.ShowDialog(_regCont) == DialogResult.OK)
                        WriteSRAM(startAddress, sramTotalBytes, hexForm.ResultData);
                }
            });
            await Task.CompletedTask;
        }

        private async Task ReadSramWithHexForm()
        {
            const int sramTotalBytes = 320;
            const int sramSegmentsPerGrid = 32;

            uint startAddress = uint.Parse(RegisterControlForm.Prompt.ShowDialog("Enter the Start Address in Decimal.", "Read SRAM"));
            if (startAddress is < 0x000 or > 0x400)
            {
                MessageBox.Show($"Address must be in 0x000 ~ 0x400.", "Read SRAM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[]? data = ReadSRAM(startAddress, sramTotalBytes);
            if (data == null)
            {
                MessageBox.Show("Data read failed.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Ui(() =>
            {
                using (var hexForm = new HexWriteForm(sramTotalBytes, sramSegmentsPerGrid, data))
                {
                    hexForm.Text = $"SRAM Read Result (Addr: 0x{startAddress:X4}, Len: {sramTotalBytes})";
                    hexForm.SetReadOnlyMode();
                    hexForm.ShowDialog(_regCont);
                }
            });
            await Task.CompletedTask;
        }

        #region MANUAL TEST ITMES
        [ChipTest("MANUAL", "Update Display", "SendCommand to Update Display")]
        public async Task UpdateDisplay(Func<string, string, Task> log) => WriteCommand(0x04);

        [ChipTest("MANUAL", "Write Display", "Write LED Data from Hex")]
        public async Task WriteDisplay(Func<string, string, Task> log) => await WriteLedDataWithHexForm();

        [ChipTest("MANUAL", "Turn Off Display", "SendCommand to Turn off Display")]
        public async Task TurnOffDisplay(Func<string, string, Task> log) => WriteCommand(0x08);

        [ChipTest("MANUAL", "Wake Up", "SendCommand to Wake up")]
        public async Task WakeUp(Func<string, string, Task> log) => WriteCommand(0x0D);

        [ChipTest("MANUAL", "Sleep", "SendCommand to Sleep")]
        public async Task Sleep(Func<string, string, Task> log) => WriteCommand(0x0E);

        [ChipTest("MANUAL", "Reset", "SendCommand to Reset")]
        public async Task Reset(Func<string, string, Task> log) => WriteCommand(0xCE);
        #endregion MANUAL TEST ITMES

        #region AUTO TEST ITEMS
        private async Task AutoSegCurrentCycle(Func<string, string, Task> log, CancellationToken ct, RunTestContext ctx)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                string? sPins = Ui(() => _regCont.PromptText("SEG MEASURE", "총 Segment 개수(32/24/16)를 입력:", "16"));
                if (string.IsNullOrWhiteSpace(sPins))
                    return;
                if (!int.TryParse(sPins, out int pinCount))
                    return;

                bool measureAll = Ui(() => MessageBox.Show($"Measure all segments?\n\nYes: {sPins} Pins All\nNo: SEG{sPins} Only", "SEG MEASURE", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                int singleSegNo = pinCount;

                string? sV = Ui(() => _regCont.PromptText("SEG MEASURE", "Start Volt[V] Segment Sweep:", "5.0"));
                if (string.IsNullOrWhiteSpace(sV))
                    return;
                if (!double.TryParse(sV, NumberStyles.Float, CultureInfo.InvariantCulture, out double startVolt))
                    return;

                string? sCycle = Ui(() => _regCont.PromptText("SEG MEASURE", "Cycle Repeats:", "3"));
                if (string.IsNullOrWhiteSpace(sCycle))
                    return;
                if (!int.TryParse(sCycle, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cycleCount))
                    return;

                const int avgTime = 1;

                double ReadMilliAmpAvg()
                {
                    double sum = 0;
                    for (int i = 0; i < avgTime; i++)
                    {
                        var s = DigitalMultimeter0?.Query("READ?");
                        var a = double.Parse(s, CultureInfo.InvariantCulture);
                        sum += Math.Round(a * 1000.0, 5);
                    }
                    return Math.Round(sum / avgTime, 5);
                }

                bool ConfirmSegmentReady(string segLabel)
                {
                    return Ui(() => MessageBox.Show($"{segLabel} Probing 후 OK를 누르세요.\n(취소 시 테스트 종료)", "SEG MEASURE", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK);
                }

                bool ConfirmContinueNext(int nextChipNo)
                {
                    return Ui(() => MessageBox.Show($"Chip #{nextChipNo - 1} 완료.\nChip 교체 후 OK를 누르면 이어서 진행합니다.\nNext Chip #{nextChipNo}.", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
                }

                async Task MeasureOneSegmentAsync(IReportSheet shInput, IReportSheet shVdd, IReportSheet shI, int currentRow, int chipNoLocal, int cycleLocal, string segLabel, bool isFirstWrite, bool measureAllMode)
                {
                    ct.ThrowIfCancellationRequested();

                    if (measureAllMode)
                    {
                        if (!ConfirmSegmentReady(segLabel))
                            throw new OperationCanceledException();
                    }

                    string rowLabel = $"Chip{chipNoLocal}_Cyc{cycleLocal + 1}_{segLabel}";

                    PowerSupply0?.Write("VOLT 5, (@1)");
                    PowerSupply0?.Write("VOLT 0.6, (@2)");
                    DigitalMultimeter0?.Write("CONF:CURR:DC 0.1A");
                    PowerSupply0?.Write("OUTP ON, (@1:2)");
                    await Task.Delay(250, ct);

                    WriteRegister(0x400, 0x3F);
                    WriteRegister(0x40D, 0x02);
                    await Task.Delay(250, ct);

                    // ==========================================
                    // 1. Input Voltage Sweep (Input Sheet)
                    // ==========================================

                    ctx.Report.SelectSheet(shInput.Name);
                    shInput.Write(currentRow, 1, rowLabel);

                    double baseCurrent = ReadMilliAmpAvg();

                    if (isFirstWrite)
                        shInput.Write(2, 2, "0.6");
                    shInput.Write(currentRow, 2, baseCurrent);

                    int start = (int)Math.Round(startVolt * 100.0);
                    int colIndex = 3;

                    for (int iv = start; iv >= 0; iv--)
                    {
                        ct.ThrowIfCancellationRequested();
                        double v = iv / 100.0;

                        PowerSupply0?.Write($"VOLT {v.ToString("F2", CultureInfo.InvariantCulture)}, (@2)");
                        await Task.Delay(500, ct);

                        double cur = ReadMilliAmpAvg();

                        if (isFirstWrite)
                            shInput.Write(2, colIndex, v.ToString("F2", CultureInfo.InvariantCulture));

                        shInput.Write(currentRow, colIndex, cur.ToString("F5", CultureInfo.InvariantCulture));

                        colIndex++;
                    }
                    ctx.Report.Save();

                    // ==========================================
                    // 2. VDD Sweep (VDD Sheet)
                    // ==========================================
                    PowerSupply0?.Write("VOLT 0.6, (@2)");
                    await Task.Delay(500, ct);

                    ctx.Report.SelectSheet(shVdd.Name);
                    shVdd.Write(currentRow, 1, rowLabel);

                    colIndex = 2;
                    for (double vdd = 5.0; vdd >= 2.7; vdd -= 0.01)
                    {
                        ct.ThrowIfCancellationRequested();

                        PowerSupply0?.Write($"VOLT {vdd.ToString("F2", CultureInfo.InvariantCulture)}, (@1)");
                        await Task.Delay(500, ct);

                        double cur = ReadMilliAmpAvg();

                        if (isFirstWrite)
                            shVdd.Write(2, colIndex, vdd.ToString("F2", CultureInfo.InvariantCulture));

                        shVdd.Write(currentRow, colIndex, cur.ToString("F5", CultureInfo.InvariantCulture));

                        colIndex++;
                    }
                    ctx.Report.Save();

                    // ==========================================
                    // 3. I[5:0] Sweep (VDD Sheet)
                    // ==========================================
                    PowerSupply0?.Write("VOLT 5, (@1)");
                    await Task.Delay(500, ct);

                    ctx.Report.SelectSheet(shI.Name);
                    shI.Write(currentRow, 1, rowLabel);

                    colIndex = 2;
                    for (uint regI = 0; regI < 64; regI++)
                    {
                        ct.ThrowIfCancellationRequested();

                        WriteRegister(0x400, regI);
                        await Task.Delay(500, ct);

                        double cur = ReadMilliAmpAvg();

                        if (isFirstWrite)
                            shVdd.Write(2, colIndex, regI.ToString("", CultureInfo.InvariantCulture));

                        shVdd.Write(currentRow, colIndex, cur.ToString("F5", CultureInfo.InvariantCulture));

                        colIndex++;
                    }
                    ctx.Report.Save();
                }

                IReportSheet? fixedSheetInput = null;
                IReportSheet? fixedSheetVdd = null;
                IReportSheet? fixedSheetI = null;

                bool singleModeFirstRun = true;

                if (!measureAll)
                {
                    string t = DateTime.Now.ToString("HHmmss");
                    fixedSheetInput = ctx.Report.CreateSheet($"SingleSeg_Input_{t}");
                    fixedSheetInput.SetSheetFont("Consolas", 11);
                    fixedSheetVdd = ctx.Report.CreateSheet($"SingleSeg_VDD_{t}");
                    fixedSheetVdd.SetSheetFont("Consolas", 11);
                    fixedSheetI = ctx.Report.CreateSheet($"SingleSeg_I_{t}");
                    fixedSheetI.SetSheetFont("Consolas", 11);

                    fixedSheetInput.Write(1, 1, "Input Sweep Data");
                    fixedSheetInput.AutoFit();
                    fixedSheetVdd.Write(1, 1, "VDD Sweep Data");
                    fixedSheetVdd.AutoFit();
                    fixedSheetI.Write(1, 1, "Current Data");
                    fixedSheetI.AutoFit();
                }

                int chipNo = 1;
                int globalRowIndex = 3;

                while (true)
                {
                    IReportSheet? shInputAll = null;
                    IReportSheet? shVddAll = null;
                    IReportSheet? shIAll = null;
                    bool allModeFirstRun = true;
                    int allModeRowIndex = 3;

                    if (measureAll)
                    {
                        string t = DateTime.Now.ToString("HHmmss");
                        shInputAll = ctx.Report.CreateSheet($"Chip{chipNo}_Input_{t}");
                        shInputAll.SetSheetFont("Consolas", 11);
                        shVddAll = ctx.Report.CreateSheet($"Chip{chipNo}_VDD_{t}");
                        shVddAll.SetSheetFont("Consolas", 11);
                        shIAll = ctx.Report.CreateSheet($"Chip{chipNo}_I_{t}");
                        shIAll.SetSheetFont("Consolas", 11);

                        shInputAll.Write(1, 1, $"Chip #{chipNo} Input Sweep");
                        shInputAll.AutoFit();
                        shVddAll.Write(1, 1, $"Chip #{chipNo} VDD Sweep");
                        shVddAll.AutoFit();
                        shIAll.Write(1, 1, $"Chip #{chipNo} Current Data");
                        shIAll.AutoFit();
                    }

                    for (int cycle = 0; cycle < cycleCount; cycle++)
                    {
                        ct.ThrowIfCancellationRequested();

                        if (measureAll)
                        {
                            for (int seg = pinCount; seg >= 1; seg--)
                            {
                                ct.ThrowIfCancellationRequested();

                                await MeasureOneSegmentAsync(shInputAll!, shVddAll!, shIAll!, allModeRowIndex, chipNo, cycle, $"SEG{seg}", allModeFirstRun, measureAll);

                                allModeFirstRun = false;
                                allModeRowIndex++;
                            }
                            await log("CYCLE", $"Completed Chip #{chipNo} Cycle {cycle + 1} (All Segments)");
                        }
                        else
                        {
                            await MeasureOneSegmentAsync(fixedSheetInput!, fixedSheetVdd!, fixedSheetI, globalRowIndex, chipNo, cycle, $"SEG{singleSegNo}", singleModeFirstRun, measureAll);

                            singleModeFirstRun = false;
                            globalRowIndex++;

                            await log("CYCLE", $"Chip #{chipNo} - Cycle {cycle + 1} Completed");
                        }

                        ctx.Report.Save();
                    }

                    PowerSupply0?.Write("OUTP OFF, (@1:2)");

                    int nextChip = chipNo + 1;
                    if (!ConfirmContinueNext(nextChip))
                    {
                        await log("STOP", "Stopped by user after cycle completion.");
                        break;
                    }
                    chipNo = nextChip;
                }
            }
            catch (OperationCanceledException)
            {
                await log("STOP", "Stopped by user.");
                throw;
            }
            catch (Exception ex)
            {
                await log("ERROR", $"Error in Auto Segment Current Measure : {ex}");
                throw;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@1:2)");
                }
                catch { }
            }
        }

        [ChipTest("AUTO", "Segment Current", "Cycle Test for Segment Current")]
        private async Task SegmentCurrentSweep(Func<string, string, Task> log, CancellationToken ct, RunTestContext ctx)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                const string segLabel = "SEG16";
                const int avgTime = 1;

                string? sStartV = Ui(() => _regCont?.PromptText("Segment Current Sweep", "Start Voltage [V]:", "3.0"));
                if (string.IsNullOrWhiteSpace(sStartV) || !double.TryParse(sStartV, NumberStyles.Float, CultureInfo.InvariantCulture, out double startVolt))
                    return;

                string? sEndV = Ui(() => _regCont?.PromptText("Segment Current Sweep", "End Voltage [V]:", "0.0"));
                if (string.IsNullOrWhiteSpace(sEndV) || !double.TryParse(sEndV, NumberStyles.Float, CultureInfo.InvariantCulture, out double endVolt))
                    return;

                string? sCycle = Ui(() => _regCont?.PromptText("Segment Current Sweep", "Cycle Count:", "1"));
                if (string.IsNullOrWhiteSpace(sCycle) || !int.TryParse(sCycle, NumberStyles.Integer, CultureInfo.InvariantCulture, out int totalCycles))
                    return;

                if (PowerSupply0 == null || DigitalMultimeter0 == null)
                {
                    await log("ERROR", "Instruments not connected.");
                    return;
                }

                double ReadMilliAmpAvg()
                {
                    double sum = 0;
                    for (int i = 0; i < avgTime; i++)
                    {
                        var s = DigitalMultimeter0.Query("READ?");
                        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                            sum += val;
                    }
                    return Math.Round((sum / avgTime) * 1000.0, 5);
                }

                async Task<double> SetAndCalibrateVoltage(double targetV)
                {
                    double setV = targetV;
                    double measuredV = 0;

                    ct.ThrowIfCancellationRequested();

                    PowerSupply0.Write($"VOLT {setV.ToString("F4", CultureInfo.InvariantCulture)}, (@2)");
                    await Task.Delay(500, ct);

                    string resp = PowerSupply0.Query("MEAS:VOLT? (@2)");
                    if (!double.TryParse(resp, NumberStyles.Float, CultureInfo.InvariantCulture, out measuredV))
                    {
                        await log("WARN", $"Voltage Measure Failed: {resp}");
                    }

                    double error = targetV - measuredV;

                    setV += error;
                    PowerSupply0.Write($"VOLT {setV.ToString("F4", CultureInfo.InvariantCulture)}, (@2)");
                    await Task.Delay(500, ct);

                    return measuredV;
                }

                if (Ui(() => MessageBox.Show($"{segLabel} Probing 후 OK를 누르세요.\n(총 {totalCycles}회 반복)\n*전압 정밀 보정 기능이 활성화됩니다.", "Ready", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)) != DialogResult.OK)
                    return;

                PowerSupply0.Write("VOLT 5, (@1)");
                PowerSupply0.Write("VOLT 0.6, (@2)");
                DigitalMultimeter0.Write("CONF:CURR:DC 0.1A");
                PowerSupply0.Write("OUTP ON, (@1:2)");
                await Task.Delay(500, ct);
                WriteRegister(0x40D, 0x02);

                string t = DateTime.Now.ToString("HHmmss");
                var sheet = ctx.Report.CreateSheet($"SegSweep_{t}");
                sheet.SetSheetFont("Consolas", 10);

                sheet.Write(1, 1, "Cycle");
                sheet.Write(1, 2, "Set Volt"); // 목표 전압
                sheet.Write(1, 3, "Real Volt"); // 보정 후 실제 전압
                for (int r = 0; r <= 63; r++)
                    sheet.Write(1, 4 + r, $"{r}");

                int currentRow = 2;

                for (int cycle = 1; cycle <= totalCycles; cycle++)
                {
                    ct.ThrowIfCancellationRequested();
                    await log("CYCLE", $"Start Cycle {cycle} / {totalCycles}");

                    int startStep = (int)Math.Round(startVolt * 100);
                    int endStep = (int)Math.Round(endVolt * 100);
                    int stepDir = (startStep >= endStep) ? -1 : 1;

                    for (int vStep = startStep; (stepDir == 1 ? vStep <= endStep : vStep >= endStep); vStep += stepDir)
                    {
                        ct.ThrowIfCancellationRequested();

                        double targetVol = vStep / 100.0;

                        double finalRealVol = await SetAndCalibrateVoltage(targetVol);

                        //sheet.Focus(currentRow, 1);
                        sheet.Write(currentRow, 1, cycle.ToString());
                        //sheet.Focus(currentRow, 2);
                        sheet.Write(currentRow, 2, targetVol.ToString("F2"));   // 목표값
                        //sheet.Focus(currentRow, 3);
                        sheet.Write(currentRow, 3, finalRealVol.ToString("F4")); // 실제값

                        for (uint regVal = 0; regVal <= 63; regVal++)
                        {
                            WriteRegister(0x400, regVal);
                            await Task.Delay(100, ct);

                            double currentMa = ReadMilliAmpAvg();
                            //sheet.Focus(currentRow, 4 + (int)regVal);
                            sheet.Write(currentRow, 4 + (int)regVal, currentMa.ToString("F5"));
                        }

                        currentRow++;
                    }

                    ctx.Report.Save();
                    await log("CYCLE", $"Completed Cycle {cycle}");

                    if (cycle < totalCycles)
                        await Task.Delay(500, ct);
                }

                sheet.AutoFit();
                await log("DONE", $"All {totalCycles} Cycles Completed.");
            }
            catch (OperationCanceledException)
            {
                await log("STOP", "Test Stopped by User.");
            }
            catch (Exception ex)
            {
                await log("ERROR", $"Segment Sweep Failed: {ex.Message}");
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@1:2)");
                }
                catch { }
            }
        }

        private const int LsdRefTrim = 0;
        private const int LodRefTrim = 7;
        private const int BgrFeOutTrim = 0;
        private const int RcOscCapTrim = 0;
        private const int Crcin = 0b1110;
        private const int IMax = 0b1111;
        private const int GridMode = 1;
        private const int ChgEn = 1;
        private const int DchgEn = 1;
        private const int Res = 0;
        private const int ProgramFlag = 1;
        private const int IReg = 0;
        private int _segMode;

        private void SetTestAna125Ref()
        {
            try
            {
                uint reg40D = this.ReadRegister(0x40D);
                this.WriteRegister(0x40C, 0x08);
                this.WriteRegister(0x40D, reg40D | (1 << 3));
            }
            catch { }
        }

        private void SetTestAna150Ref()
        {
            try
            {
                uint reg40D = this.ReadRegister(0x40D);
                this.WriteRegister(0x40C, 0x10);
                this.WriteRegister(0x40D, reg40D | (1 << 3));
            }
            catch { }
        }

        private void SetTestAnaPtatOut()
        {
            try
            {
                uint reg40D = this.ReadRegister(0x40D);
                this.WriteRegister(0x40C, 0x04);
                this.WriteRegister(0x40D, reg40D | (1 << 3));
            }
            catch { }
        }

        private void SetTestAnaOscFreq()
        {
            try
            {
                uint reg40D = this.ReadRegister(0x40D);
                this.WriteRegister(0x40C, 0x01);
                this.WriteRegister(0x40D, reg40D | (1 << 3));
            }
            catch { }
        }

        private void DisableTestAna()
        {
            try
            {
                uint reg40D = this.ReadRegister(0x40D);
                this.WriteRegister(0x40C, 0x00);
                this.WriteRegister(0x40D, reg40D & ~(1u << 3));
            }
            catch { }
        }

        private async Task<double[]?> RunCal125Ref(CancellationToken ct, double target = 0.9365)
        {
            RegisterFieldManager.RegisterField? TSP_125_TRIM = null;

            try
            {
                TSP_125_TRIM = _regCont?.RegMgr.GetRegisterItem(this, "TSP_125_TRIM");
                if (DigitalMultimeter0 == null)
                    throw new Exception("Fail to connect Instruments: DigitalMultimeter0.");

                int left = 0, right = 31, bestCode = -1;
                double bestDiff = double.MaxValue, dmmVolt = 0.0;

                SetTestAna125Ref();
                DigitalMultimeter0.Write("CONF:VOLT:DC AUTO");

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();

                    int mid = left + (right - left) / 2;

                    if (TSP_125_TRIM != null)
                    {
                        TSP_125_TRIM.Value = (uint)mid;
                        TSP_125_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    var measurements = new List<double>(3);
                    for (int i = 0; i < 3; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        string? response = DigitalMultimeter0.Query("READ?");
                        if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                            throw new Exception($"DMM returned invalid measurement: '{response}'");
                        measurements.Add(mv);
                    }

                    dmmVolt = Math.Round(measurements.Average(), 4);
                    double diff = Math.Round(Math.Abs(target - dmmVolt), 4);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestCode = mid;
                    }

                    if (dmmVolt > target)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                if (bestCode >= 0 && TSP_125_TRIM != null)
                {
                    TSP_125_TRIM.Value = (uint)bestCode;
                    TSP_125_TRIM.Write();
                    await Task.Delay(100, ct);
                }

                var finalMeasurements = new List<double>(3);
                for (int i = 0; i < 3; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    string? response = DigitalMultimeter0.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"DMM returned invalid measurement: '{response}'");
                    finalMeasurements.Add(mv);
                }

                dmmVolt = Math.Round(finalMeasurements.Average(), 4);
                return new[] { dmmVolt, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCal125Ref failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    DisableTestAna();
                }
                catch { }
                try
                {
                    DigitalMultimeter0?.Write("CONF:VOLT:AC AUTO");
                }
                catch { }
            }
        }

        private async Task<double[]?> RunCal150Ref(CancellationToken ct, double target = 1.009)
        {
            RegisterFieldManager.RegisterField? TSP_150_TRIM = null;

            try
            {
                TSP_150_TRIM = _regCont?.RegMgr.GetRegisterItem(this, "TSP_150_TRIM");
                if (DigitalMultimeter0 == null)
                    throw new Exception("Fail to connect Instruments: DigitalMultimeter0.");

                int left = 0, right = 31, bestCode = -1;
                double bestDiff = double.MaxValue, dmmVolt = 0.0;

                SetTestAna150Ref();
                DigitalMultimeter0.Write("CONF:VOLT:DC AUTO");

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();
                    int mid = left + (right - left) / 2;

                    if (TSP_150_TRIM != null)
                    {
                        TSP_150_TRIM.Value = (uint)mid;
                        TSP_150_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    var measurements = new List<double>(3);
                    for (int i = 0; i < 3; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        string? response = DigitalMultimeter0.Query("READ?");
                        if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                            throw new Exception($"DMM returned invalid measurement: '{response}'");
                        measurements.Add(mv);
                    }

                    dmmVolt = Math.Round(measurements.Average(), 4);
                    double diff = Math.Round(Math.Abs(target - dmmVolt), 4);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestCode = mid;
                    }

                    if (dmmVolt > target)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                if (bestCode >= 0 && TSP_150_TRIM != null)
                {
                    TSP_150_TRIM.Value = (uint)bestCode;
                    TSP_150_TRIM.Write();
                    await Task.Delay(100, ct);
                }

                var finalMeasurements = new List<double>(3);
                for (int i = 0; i < 3; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    string? response = DigitalMultimeter0.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"DMM returned invalid measurement: '{response}'");
                    finalMeasurements.Add(mv);
                }

                dmmVolt = Math.Round(finalMeasurements.Average(), 4);
                return new[] { dmmVolt, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCal150Ref failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    DisableTestAna();
                }
                catch { }
                try
                {
                    DigitalMultimeter0?.Write("CONF:VOLT:AC AUTO");
                }
                catch { }
            }
        }

        private async Task<double[]?> RunCalPtatOut(CancellationToken ct, double target = 0.727)
        {
            RegisterFieldManager.RegisterField? VPTAT_OUT_TRIM = null;

            try
            {
                VPTAT_OUT_TRIM = _regCont?.RegMgr.GetRegisterItem(this, "VPTAT_OUT_TRIM");
                if (DigitalMultimeter0 == null)
                    throw new Exception("Fail to connect Instruments: DigitalMultimeter0.");

                SetTestAnaPtatOut();
                DigitalMultimeter0.Write("CONF:VOLT:DC AUTO");

                int left = 0, right = 31, bestOrd = -1;
                double bestDiff = double.MaxValue;

                // Check slope direction
                if (VPTAT_OUT_TRIM != null)
                {
                    VPTAT_OUT_TRIM.Value = (uint)((left + 16) & 31);
                    VPTAT_OUT_TRIM.Write();
                }
                await Task.Delay(100, ct);
                double vL = await ReadAvgVolt(DigitalMultimeter0, ct);

                if (VPTAT_OUT_TRIM != null)
                {
                    VPTAT_OUT_TRIM.Value = (uint)((right + 16) & 31);
                    VPTAT_OUT_TRIM.Write();
                }
                await Task.Delay(100, ct);
                double vR = await ReadAvgVolt(DigitalMultimeter0, ct);

                bool isDescending = vR < vL;

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();
                    int mid = left + (right - left) / 2;
                    int code = (mid + 16) & 31;

                    if (VPTAT_OUT_TRIM != null)
                    {
                        VPTAT_OUT_TRIM.Value = (uint)code;
                        VPTAT_OUT_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    double dmmVolt = await ReadAvgVolt(DigitalMultimeter0, ct);
                    double diff = Math.Round(Math.Abs(target - dmmVolt), 4);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestOrd = mid;
                    }

                    if (isDescending)
                    {
                        if (dmmVolt > target)
                            left = mid + 1;
                        else
                            right = mid - 1;
                    }
                    else
                    {
                        if (dmmVolt < target)
                            left = mid + 1;
                        else
                            right = mid - 1;
                    }
                }

                int bestCode = -1;
                if (bestOrd >= 0 && VPTAT_OUT_TRIM != null)
                {
                    bestCode = (bestOrd + 16) & 31;
                    VPTAT_OUT_TRIM.Value = (uint)bestCode;
                    VPTAT_OUT_TRIM.Write();
                    await Task.Delay(100, ct);
                }

                double finalVolt = await ReadAvgVolt(DigitalMultimeter0, ct);
                return new[] { finalVolt, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCalPtatOut failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    DisableTestAna();
                }
                catch { }
                try
                {
                    DigitalMultimeter0?.Write("CONF:VOLT:AC AUTO");
                }
                catch { }
            }
        }

        private async Task<double> ReadAvgVolt(IScpiClient dmm, CancellationToken ct)
        {
            var measurements = new List<double>(3);
            for (int i = 0; i < 3; i++)
            {
                ct.ThrowIfCancellationRequested();
                string? r = dmm.Query("READ?");
                if (!double.TryParse(r, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                    throw new FormatException($"DMM returned invalid measurement: '{r}'");
                measurements.Add(mv);
            }
            return Math.Round(measurements.Average(), 4);
        }

        private async Task<double[]?> RunCalOscFreq(CancellationToken ct, double target = 1.9985)
        {
            RegisterFieldManager.RegisterField? RCOSC_BIAS_TRIM = null;

            try
            {
                RCOSC_BIAS_TRIM = _regCont?.RegMgr.GetRegisterItem(this, "RC_OSC_BIAS_TRIM");
                if (OscilloScope0 == null)
                    throw new Exception("Fail to connect Instruments: OscilloScope0.");

                int left = 0, right = 31, bestCode = -1;
                double bestDiff = double.MaxValue, oscMHz = 0.0;

                SetTestAnaOscFreq();
                OscilloScope0.Write(":CHAN1:SCAL 2");
                OscilloScope0.Write(":CHAN1:OFFS 0");
                OscilloScope0.Write(":TIM:SCAL 2.00E-7");
                OscilloScope0.Write(":TIM:POS 0");

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();
                    int mid = left + (right - left) / 2;

                    if (RCOSC_BIAS_TRIM != null)
                    {
                        RCOSC_BIAS_TRIM.Value = (uint)mid;
                        RCOSC_BIAS_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    var measurements = new List<double>(3);
                    for (int i = 0; i < 3; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        OscilloScope0.Write(":MEAS:FREQ CHAN1");
                        string? response = OscilloScope0.Query(":MEAS:FREQ?");
                        if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                            throw new Exception($"OscilloScope returned invalid measurement: '{response}'");
                        measurements.Add(mv);
                    }

                    oscMHz = Math.Round(measurements.Average() / 1_000_000, 4);
                    double diff = Math.Round(Math.Abs(target - oscMHz), 4);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestCode = mid;
                    }

                    if (oscMHz > target)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                if (bestCode >= 0 && RCOSC_BIAS_TRIM != null)
                {
                    RCOSC_BIAS_TRIM.Value = (uint)bestCode;
                    RCOSC_BIAS_TRIM.Write();
                    await Task.Delay(100, ct);
                }

                var finalMeasurements = new List<double>(3);
                for (int i = 0; i < 3; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    OscilloScope0.Write(":MEAS:FREQ CHAN1");
                    string? response = OscilloScope0.Query(":MEAS:FREQ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"OscilloScope returned invalid measurement: '{response}'");
                    finalMeasurements.Add(mv);
                }

                oscMHz = Math.Round(finalMeasurements.Average() / 1_000_000, 4);
                return new[] { oscMHz, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCalOscFreq failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    DisableTestAna();
                }
                catch { }
            }
        }

        private async Task<double[]?> RunCalRextCurrent(CancellationToken ct, double target = 30)
        {
            RegisterFieldManager.RegisterField? BGR_OUT_TRIM = null, SEG_CURR = null, SEG_TEST = null, RES = null;

            try
            {
                if (_regCont != null)
                {
                    BGR_OUT_TRIM = _regCont.RegMgr.GetRegisterItem(this, "BGR_OUT_TRIM");
                    SEG_CURR = _regCont.RegMgr.GetRegisterItem(this, "I[5:0]");
                    SEG_TEST = _regCont.RegMgr.GetRegisterItem(this, "SEG_TEST");
                    RES = _regCont.RegMgr.GetRegisterItem(this, "RES");
                }

                if (PowerSupply0 == null || DigitalMultimeter1 == null)
                    throw new Exception("Fail to connect Instruments: PowerSupply0 / DigitalMultimeter1.");

                int left = 0, right = 31, bestCode = -1;
                double bestDiff = double.MaxValue, currentmA = 0.0;

                DigitalMultimeter1.Write("CONF:CURR:DC 0.1");

                if (SEG_CURR != null)
                {
                    SEG_CURR.Value = 63;
                    SEG_CURR.Write();
                }
                if (RES != null)
                {
                    RES.Value = 1;
                    RES.Write();
                }
                if (SEG_TEST != null)
                {
                    SEG_TEST.Value = 1;
                    SEG_TEST.Write();
                }
                await Task.Delay(100, ct);

                PowerSupply0.Write("VOLT 0.6, (@2)");
                PowerSupply0.Write("CURR 1, (@2)");
                PowerSupply0.Write("OUTP ON, (@2)");

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();
                    int mid = left + (right - left) / 2;

                    if (BGR_OUT_TRIM != null)
                    {
                        BGR_OUT_TRIM.Value = (uint)mid;
                        BGR_OUT_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    double avgCurrent = await ReadAvgCurrentMA(DigitalMultimeter1, ct);
                    currentmA = avgCurrent;
                    double diff = Math.Round(Math.Abs(target - currentmA), 2);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestCode = mid;
                    }

                    if (currentmA > target)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                if (BGR_OUT_TRIM != null)
                {
                    BGR_OUT_TRIM.Value = (uint)bestCode;
                    BGR_OUT_TRIM.Write();
                }
                await Task.Delay(100, ct);

                currentmA = await ReadAvgCurrentMA(DigitalMultimeter1, ct);
                return new[] { currentmA, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCalRextCurrent failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@2)");
                }
                catch { }
                try
                {
                    if (SEG_TEST != null)
                    {
                        SEG_TEST.Value = 0;
                        SEG_TEST.Write();
                    }
                    if (RES != null)
                    {
                        RES.Value = 0;
                        RES.Write();
                    }
                }
                catch { }
            }
        }

        private async Task<double[]?> RunCalIbiasOfDrv(CancellationToken ct, double target = 30)
        {
            RegisterFieldManager.RegisterField? DRV_IBIAS_TRIM = null, SEG_CURR = null, SEG_TEST = null;

            try
            {
                if (_regCont != null)
                {
                    DRV_IBIAS_TRIM = _regCont.RegMgr.GetRegisterItem(this, "DRV_IBIAS_TRIM");
                    SEG_CURR = _regCont.RegMgr.GetRegisterItem(this, "I[5:0]");
                    SEG_TEST = _regCont.RegMgr.GetRegisterItem(this, "SEG_TEST");
                }

                if (PowerSupply0 == null || DigitalMultimeter1 == null)
                    throw new Exception("Fail to connect Instruments: PowerSupply0 / DigitalMultimeter1.");

                int left = 0, right = 31, bestCode = -1;
                double bestDiff = double.MaxValue, currentmA = 0.0;

                DigitalMultimeter1.Write("CONF:CURR:DC 0.1");

                if (SEG_CURR != null)
                {
                    SEG_CURR.Value = 63;
                    SEG_CURR.Write();
                }
                if (SEG_TEST != null)
                {
                    SEG_TEST.Value = 1;
                    SEG_TEST.Write();
                }
                await Task.Delay(100, ct);

                PowerSupply0.Write("VOLT 0.6, (@2)");
                PowerSupply0.Write("CURR 1, (@2)");
                PowerSupply0.Write("OUTP ON, (@2)");

                while (left <= right)
                {
                    ct.ThrowIfCancellationRequested();
                    int mid = left + (right - left) / 2;

                    if (DRV_IBIAS_TRIM != null)
                    {
                        DRV_IBIAS_TRIM.Value = (uint)mid;
                        DRV_IBIAS_TRIM.Write();
                    }
                    await Task.Delay(100, ct);

                    double avgCurrent = await ReadAvgCurrentMA(DigitalMultimeter1, ct);
                    currentmA = avgCurrent;
                    double diff = Math.Round(Math.Abs(target - currentmA), 2);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestCode = mid;
                    }

                    if (currentmA < target)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }

                if (DRV_IBIAS_TRIM != null)
                {
                    DRV_IBIAS_TRIM.Value = (uint)bestCode;
                    DRV_IBIAS_TRIM.Write();
                }
                await Task.Delay(100, ct);

                currentmA = await ReadAvgCurrentMA(DigitalMultimeter1, ct);
                return new[] { currentmA, bestCode };
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunCalIbiasOfDrv failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@2)");
                }
                catch { }
                try
                {
                    if (SEG_TEST != null)
                    {
                        SEG_TEST.Value = 0;
                        SEG_TEST.Write();
                    }
                }
                catch { }
            }
        }

        private async Task<double> ReadAvgCurrentMA(IScpiClient dmm, CancellationToken ct)
        {
            var measurements = new List<double>(3);
            for (int i = 0; i < 3; i++)
            {
                ct.ThrowIfCancellationRequested();
                string? response = dmm.Query("READ?");
                if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                    throw new Exception($"DMM returned invalid measurement: '{response}'");
                measurements.Add(mv);
            }
            return Math.Round(measurements.Average() * 1000, 2);
        }

        private int GetBit(int value, int bitPosition) => (value >> bitPosition) & 1;

        private bool SelectPkgType()
        {
            // Run on UI Thread
            bool result = false;
            Ui(() =>
            {
                using (var form = new Form())
                {
                    form.Text = "Initial_Test";
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.Width = 250;
                    form.Height = 200;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var label = new Label() { Left = 20, Top = 20, Text = "Select PKG Type:" };
                    var rb48 = new RadioButton() { Text = "48QFN", Left = 40, Top = 50, Checked = true };
                    var rb40 = new RadioButton() { Text = "40QFN", Left = 40, Top = 75 };
                    var rb32 = new RadioButton() { Text = "32QFN", Left = 40, Top = 100 };

                    var okButton = new System.Windows.Forms.Button() { Text = "확인", Left = 30, Width = 80, Top = 130, DialogResult = DialogResult.OK };
                    var cancelButton = new System.Windows.Forms.Button() { Text = "취소", Left = 120, Width = 80, Top = 130, DialogResult = DialogResult.Cancel };

                    form.Controls.Add(label);
                    form.Controls.Add(rb48);
                    form.Controls.Add(rb40);
                    form.Controls.Add(rb32);
                    form.Controls.Add(okButton);
                    form.Controls.Add(cancelButton);
                    form.AcceptButton = okButton;
                    form.CancelButton = cancelButton;

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (rb48.Checked)
                            _segMode = 0b11;
                        else if (rb40.Checked)
                            _segMode = 0b10;
                        else if (rb32.Checked)
                            _segMode = 0b01;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            });
            return result;
        }

        private int CalculateCrc(Dictionary<string, int> trimResults)
        {
            int tsp125Trim = trimResults.TryGetValue("TSP_125_TRIM", out var v0) ? v0 : 0;
            int tsp150Trim = trimResults.TryGetValue("TSP_150_TRIM", out var v1) ? v1 : 0;
            int vptatOutTrim = trimResults.TryGetValue("VPTAT_OUT_TRIM", out var v2) ? v2 : 0;
            int bgrOutTrim = trimResults.TryGetValue("BGR_OUT_TRIM", out var v3) ? v3 : 0;
            int drvIbiasTrim = trimResults.TryGetValue("DRV_IBIAS_TRIM", out var v4) ? v4 : 0;
            int rcOscBiasTrim = trimResults.TryGetValue("RC_OSC_BIAS_TRIM", out var v5) ? v5 : 0;

            int crc3 = 0;
            crc3 ^= GetBit(Crcin, 0) ^ GetBit(Crcin, 1) ^ GetBit(Crcin, 3);
            crc3 ^= GetBit(rcOscBiasTrim, 0) ^ GetBit(rcOscBiasTrim, 1) ^ GetBit(rcOscBiasTrim, 3);
            crc3 ^= GetBit(drvIbiasTrim, 0) ^ GetBit(drvIbiasTrim, 1) ^ GetBit(drvIbiasTrim, 3);
            crc3 ^= GetBit(IReg, 2) ^ GetBit(IReg, 3) ^ GetBit(IReg, 5);
            crc3 ^= GetBit(BgrFeOutTrim, 2) ^ GetBit(BgrFeOutTrim, 3);
            crc3 ^= GetBit(bgrOutTrim, 0) ^ GetBit(bgrOutTrim, 4);
            crc3 ^= GetBit(vptatOutTrim, 0) ^ GetBit(vptatOutTrim, 2);
            crc3 ^= GetBit(tsp150Trim, 1) ^ GetBit(tsp150Trim, 2) ^ GetBit(tsp150Trim, 4);
            crc3 ^= GetBit(tsp125Trim, 3) ^ GetBit(tsp125Trim, 4);
            crc3 ^= GetBit(LodRefTrim, 1) ^ GetBit(LsdRefTrim, 2);
            crc3 ^= GridMode ^ GetBit(IMax, 1) ^ ChgEn ^ GetBit(_segMode, 0) ^ ProgramFlag;

            int crc2 = 0;
            crc2 ^= GetBit(Crcin, 1) ^ GetBit(Crcin, 2) ^ GetBit(Crcin, 3);
            crc2 ^= GetBit(rcOscBiasTrim, 1) ^ GetBit(rcOscBiasTrim, 2) ^ GetBit(rcOscBiasTrim, 3) ^ GetBit(RcOscCapTrim, 1);
            crc2 ^= GetBit(drvIbiasTrim, 1) ^ GetBit(drvIbiasTrim, 2) ^ GetBit(drvIbiasTrim, 3);
            crc2 ^= GetBit(IReg, 1) ^ GetBit(IReg, 3) ^ GetBit(IReg, 4) ^ GetBit(IReg, 5);
            crc2 ^= GetBit(BgrFeOutTrim, 1) ^ GetBit(BgrFeOutTrim, 3) ^ GetBit(BgrFeOutTrim, 4);
            crc2 ^= GetBit(bgrOutTrim, 0) ^ GetBit(bgrOutTrim, 3);
            crc2 ^= GetBit(vptatOutTrim, 0) ^ GetBit(vptatOutTrim, 1) ^ GetBit(vptatOutTrim, 2);
            crc2 ^= GetBit(tsp150Trim, 0) ^ GetBit(tsp150Trim, 2) ^ GetBit(tsp150Trim, 3) ^ GetBit(tsp150Trim, 4);
            crc2 ^= GetBit(tsp125Trim, 2) ^ GetBit(tsp125Trim, 4);
            crc2 ^= GetBit(LodRefTrim, 0) ^ GetBit(LodRefTrim, 1) ^ GetBit(LsdRefTrim, 1);
            crc2 ^= GridMode ^ GetBit(IMax, 0) ^ GetBit(IMax, 1) ^ DchgEn ^ GetBit(_segMode, 0) ^ GetBit(_segMode, 1) ^ ProgramFlag;

            int crc1 = 0;
            crc1 ^= GetBit(Crcin, 2) ^ GetBit(Crcin, 3);
            crc1 ^= GetBit(rcOscBiasTrim, 2) ^ GetBit(rcOscBiasTrim, 3) ^ GetBit(RcOscCapTrim, 0);
            crc1 ^= GetBit(drvIbiasTrim, 2) ^ GetBit(drvIbiasTrim, 3);
            crc1 ^= GetBit(IReg, 0) ^ GetBit(IReg, 4) ^ GetBit(IReg, 5);
            crc1 ^= GetBit(BgrFeOutTrim, 0) ^ GetBit(BgrFeOutTrim, 4);
            crc1 ^= GetBit(bgrOutTrim, 0) ^ GetBit(bgrOutTrim, 2);
            crc1 ^= GetBit(vptatOutTrim, 1) ^ GetBit(vptatOutTrim, 2) ^ GetBit(vptatOutTrim, 4);
            crc1 ^= GetBit(tsp150Trim, 3) ^ GetBit(tsp150Trim, 4);
            crc1 ^= GetBit(tsp125Trim, 1);
            crc1 ^= GetBit(LodRefTrim, 0) ^ GetBit(LodRefTrim, 1) ^ GetBit(LsdRefTrim, 0);
            crc1 ^= GetBit(IMax, 0) ^ GetBit(IMax, 1) ^ GetBit(IMax, 3);
            crc1 ^= GetBit(_segMode, 1) ^ ProgramFlag;

            int crc0 = 0;
            crc0 ^= GetBit(Crcin, 1) ^ GetBit(Crcin, 2);
            crc0 ^= GetBit(rcOscBiasTrim, 1) ^ GetBit(rcOscBiasTrim, 2) ^ GetBit(rcOscBiasTrim, 4);
            crc0 ^= GetBit(drvIbiasTrim, 1) ^ GetBit(drvIbiasTrim, 2) ^ GetBit(drvIbiasTrim, 4);
            crc0 ^= GetBit(IReg, 3) ^ GetBit(IReg, 4) ^ Res;
            crc0 ^= GetBit(BgrFeOutTrim, 3) ^ GetBit(BgrFeOutTrim, 4);
            crc0 ^= GetBit(bgrOutTrim, 1);
            crc0 ^= GetBit(vptatOutTrim, 0) ^ GetBit(vptatOutTrim, 1) ^ GetBit(vptatOutTrim, 3);
            crc0 ^= GetBit(tsp150Trim, 2) ^ GetBit(tsp150Trim, 3);
            crc0 ^= GetBit(tsp125Trim, 0) ^ GetBit(tsp125Trim, 4);
            crc0 ^= GetBit(LodRefTrim, 0) ^ GetBit(LodRefTrim, 2);
            crc0 ^= GridMode ^ GetBit(IMax, 0) ^ GetBit(IMax, 2);
            crc0 ^= GetBit(_segMode, 0) ^ GetBit(_segMode, 1);

            return (crc3 << 3) | (crc2 << 2) | (crc1 << 1) | crc0;
        }

        private byte[] BuildEfuseData(Dictionary<string, int> trimResults)
        {
            int tsp125Trim = trimResults.TryGetValue("TSP_125_TRIM", out var v0) ? v0 : 0;
            int tsp150Trim = trimResults.TryGetValue("TSP_150_TRIM", out var v1) ? v1 : 0;
            int vptatOutTrim = trimResults.TryGetValue("VPTAT_OUT_TRIM", out var v2) ? v2 : 0;
            int bgrOutTrim = trimResults.TryGetValue("BGR_OUT_TRIM", out var v3) ? v3 : 0;
            int drvIbiasTrim = trimResults.TryGetValue("DRV_IBIAS_TRIM", out var v4) ? v4 : 0;
            int rcOscBiasTrim = trimResults.TryGetValue("RC_OSC_BIAS_TRIM", out var v5) ? v5 : 0;

            int crcValue = CalculateCrc(trimResults);

            byte[] wdata = new byte[8];

            wdata[0] = (byte)(
                (GetBit(rcOscBiasTrim, 0) << 0) |
                (GetBit(rcOscBiasTrim, 1) << 1) |
                (GetBit(rcOscBiasTrim, 2) << 2) |
                (GetBit(rcOscBiasTrim, 3) << 3) |
                (GetBit(rcOscBiasTrim, 4) << 4) |
                (GetBit(RcOscCapTrim, 0) << 5) |
                (GetBit(RcOscCapTrim, 1) << 6) |
                (GetBit(drvIbiasTrim, 0) << 7)
            );

            wdata[1] = (byte)(
                (GetBit(drvIbiasTrim, 1) << 0) |
                (GetBit(drvIbiasTrim, 2) << 1) |
                (GetBit(drvIbiasTrim, 3) << 2) |
                (GetBit(drvIbiasTrim, 4) << 3) |
                (GetBit(IReg, 0) << 4) |
                (GetBit(IReg, 1) << 5) |
                (GetBit(IReg, 2) << 6) |
                (GetBit(IReg, 3) << 7)
            );

            wdata[2] = (byte)(
                (GetBit(IReg, 4) << 0) |
                (GetBit(IReg, 5) << 1) |
                (GetBit(Res, 0) << 2) |
                (GetBit(BgrFeOutTrim, 0) << 3) |
                (GetBit(BgrFeOutTrim, 1) << 4) |
                (GetBit(BgrFeOutTrim, 2) << 5) |
                (GetBit(BgrFeOutTrim, 3) << 6) |
                (GetBit(BgrFeOutTrim, 4) << 7)
            );

            wdata[3] = (byte)(
                (GetBit(bgrOutTrim, 0) << 0) |
                (GetBit(bgrOutTrim, 1) << 1) |
                (GetBit(bgrOutTrim, 2) << 2) |
                (GetBit(bgrOutTrim, 3) << 3) |
                (GetBit(bgrOutTrim, 4) << 4) |
                (GetBit(vptatOutTrim, 0) << 5) |
                (GetBit(vptatOutTrim, 1) << 6) |
                (GetBit(vptatOutTrim, 2) << 7)
            );

            wdata[4] = (byte)(
                (GetBit(vptatOutTrim, 3) << 0) |
                (GetBit(vptatOutTrim, 4) << 1) |
                (GetBit(tsp150Trim, 0) << 2) |
                (GetBit(tsp150Trim, 1) << 3) |
                (GetBit(tsp150Trim, 2) << 4) |
                (GetBit(tsp150Trim, 3) << 5) |
                (GetBit(tsp150Trim, 4) << 6) |
                (GetBit(tsp125Trim, 0) << 7)
            );

            wdata[5] = (byte)(
                (GetBit(tsp125Trim, 1) << 0) |
                (GetBit(tsp125Trim, 2) << 1) |
                (GetBit(tsp125Trim, 3) << 2) |
                (GetBit(tsp125Trim, 4) << 3) |
                (GetBit(LodRefTrim, 0) << 4) |
                (GetBit(LodRefTrim, 1) << 5) |
                (GetBit(LodRefTrim, 2) << 6) |
                (GetBit(LsdRefTrim, 0) << 7)
            );

            wdata[6] = (byte)(
                (GetBit(LsdRefTrim, 1) << 0) |
                (GetBit(LsdRefTrim, 2) << 1) |
                (GridMode << 2) |
                (GetBit(IMax, 0) << 3) |
                (GetBit(IMax, 1) << 4) |
                (GetBit(IMax, 2) << 5) |
                (GetBit(IMax, 3) << 6) |
                (DchgEn << 7)
            );

            wdata[7] = (byte)(
                (ChgEn << 0) |
                (GetBit(_segMode, 0) << 1) |
                (GetBit(_segMode, 1) << 2) |
                (ProgramFlag << 3) |
                (GetBit(crcValue, 0) << 4) |
                (GetBit(crcValue, 1) << 5) |
                (GetBit(crcValue, 2) << 6) |
                (GetBit(crcValue, 3) << 7)
            );

            return wdata;
        }

        private async Task<double[]?> RunTestPowerOnReset(CancellationToken ct)
        {
            try
            {
                if (PowerSupply0 == null || DigitalMultimeter2 == null)
                    throw new Exception("Fail to connect Instruments: PowerSupply0 / DigitalMultimeter2");

                double[] porDetect = new double[2];

                DigitalMultimeter2.Write("SENS:FUNC 'CURR:DC'");
                DigitalMultimeter2.Write("SENS:CURR:DC:RANG 0.1");

                PowerSupply0.Write("VOLT 0, (@1)");
                PowerSupply0.Write("CURR 1, (@1)");
                PowerSupply0.Write("OUTP ON, (@1)");
                await Task.Delay(1000, ct);

                for (double volt = 2; volt < 2.6; volt += 0.05)
                {
                    ct.ThrowIfCancellationRequested();
                    PowerSupply0.Write($"VOLT {volt}, (@1)");
                    await Task.Delay(1500, ct);

                    string response = DigitalMultimeter2.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"Invalid response: '{response}'");

                    double temp = Math.Round(mv * 1_000, 2);
                    if (temp >= 0.18)
                    {
                        porDetect[0] = volt;
                        break;
                    }
                }

                for (double volt = 2.2; volt > 1.65; volt -= 0.05)
                {
                    ct.ThrowIfCancellationRequested();
                    PowerSupply0.Write($"VOLT {volt}, (@1)");
                    await Task.Delay(1500, ct);

                    string response = DigitalMultimeter2.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"Invalid response: '{response}'");

                    double temp = Math.Round(mv * 1_000, 2);
                    if (temp <= 0.18)
                    {
                        porDetect[1] = volt;
                        break;
                    }
                }

                return porDetect;
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunTestPowerOnReset failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("VOLT 5, (@1)");
                }
                catch { }
                try
                {
                    DigitalMultimeter2?.Write("SENS:CURR:DC:RANG 0.1");
                }
                catch { }
            }
        }

        private async Task<double[]?> RunTestStandbyCurrent(CancellationToken ct)
        {
            try
            {
                if (PowerSupply0 == null || DigitalMultimeter2 == null)
                    throw new Exception("Fail to connect Instruments: PowerSupply0 / DigitalMultimeter2");

                double[] standbyCurrent = new double[2];
                var sleep_en = _regCont?.RegMgr.GetRegisterItem(this, "SLEEP");

                DigitalMultimeter2.Write("SENS:FUNC 'CURR:DC'");
                DigitalMultimeter2.Write("SENS:CURR:DC:RANGE 1E-3");

                PowerSupply0.Write("VOLT 5, (@1)");
                PowerSupply0.Write("CURR 1, (@1)");
                PowerSupply0.Write("OUTP ON, (@1)");
                await Task.Delay(1000, ct);

                double temp = 0;
                for (int i = 0; i < 5; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    string response = DigitalMultimeter2.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"Invalid response: '{response}'");
                    temp += Math.Round(mv * 1_000, 2);
                }
                standbyCurrent[0] = Math.Round(temp / 5, 2);

                if (sleep_en != null)
                {
                    sleep_en.Value = 1;
                    sleep_en.Write();
                }
                await Task.Delay(100, ct);
                this.WriteCommand(0x0E);
                await Task.Delay(300, ct);

                DigitalMultimeter2.Write("SENS:CURR:DC:RANGE 1E-4");

                temp = 0;
                for (int i = 0; i < 5; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    string response = DigitalMultimeter2.Query("READ?");
                    if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv))
                        throw new FormatException($"Invalid response: '{response}'");
                    temp += Math.Round(mv * 1_000_000, 2);
                }
                standbyCurrent[1] = Math.Round(temp / 5, 2);

                return standbyCurrent;
            }
            catch (Exception e)
            {
                AppendLog("ERROR", $"RunTestStandbyCurrent failed: {e.Message}");
                throw;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@1)");
                }
                catch { }
            }
        }

        private async Task<double[]?> RunMeasSegCurrent(IReportSheet segSheet, IReportSheet initSheet, int xOffset, CancellationToken ct)
        {
            if (DigitalMultimeter1 == null)
                throw new Exception("Fail to connect Instruments: DigitalMultimeter1");

            var SEG_CURR = _regCont?.RegMgr.GetRegisterItem(this, "I[5:0]");
            var SEG_TEST = _regCont?.RegMgr.GetRegisterItem(this, "SEG_TEST");

            try
            {
                string message = $"Run Measurement of each Segments Current.\nSEG1 ~ 16 / DigitalMultimeter1 / Current";

                if (ShowMsg(message, "SCH1711 Initial Test", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return null;

                if (SEG_CURR != null)
                {
                    SEG_CURR.Value = 63;
                    SEG_CURR.Write();
                }
                if (SEG_TEST != null)
                {
                    SEG_TEST.Value = 1;
                    SEG_TEST.Write();
                }

                DigitalMultimeter1.Write("CONF:CURR:DC 0.1");

                PowerSupply0?.Write("VOLT 0.6, (@2)");
                PowerSupply0?.Write("CURR 1, (@2)");
                PowerSupply0?.Write("OUTP ON, (@2)");

                int[] segCount = { 16, 24, 32 };
                // Safety check for _segMode index
                int countIndex = (_segMode >= 1 && _segMode <= 3) ? _segMode - 1 : 2;
                double[] segCurrents = new double[segCount[countIndex]];

                segSheet.Write(3, 3 + xOffset, (xOffset + 1).ToString());

                for (int i = 0; i < segCurrents.Length; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    double avgBuff = 0;
                    bool checkConnection = true;
                    int retryCount = 0;

                    if (ShowMsg($"SEG{i + 1} Probing 후 Yes", "Probing", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return null;

                    while (checkConnection)
                    {
                        string response = DigitalMultimeter1.Query("READ?");
                        if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var measuredValue))
                            throw new FormatException($"Invalid response: '{response}'");

                        double temp = Math.Round(measuredValue * 1000, 2);

                        if (temp > 0.1)
                        {
                            checkConnection = false;
                            avgBuff += temp;
                        }
                        else
                        {
                            retryCount++;
                            if (retryCount >= 10)
                            {
                                var result = ShowMsg(
                                    $"SEG{i + 1} 전류가 감지되지 않습니다. (현재값: {temp}mA)\n\n재시도하시겠습니까?\n(OK: 계속 시도, Cancel: 측정 중단)",
                                    "연결 확인 실패",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.OK)
                                    retryCount = 0;
                                else
                                    return null;
                            }
                        }
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        string response = DigitalMultimeter1.Query("READ?");
                        if (!double.TryParse(response, NumberStyles.Float, CultureInfo.InvariantCulture, out var measuredValue))
                            throw new FormatException($"Invalid response: '{response}'");

                        avgBuff += Math.Round(measuredValue * 1000, 2);
                    }

                    segCurrents[i] = Math.Round(avgBuff / 5, 2);
                    segSheet.Write(4 + i, 3 + xOffset, segCurrents[i].ToString(CultureInfo.InvariantCulture));
                }

                double[] minmax = new double[2];
                minmax[0] = segCurrents.Max();
                minmax[1] = segCurrents.Min();

                segSheet.Write(36, 3 + xOffset, minmax[0].ToString(CultureInfo.InvariantCulture));
                segSheet.Write(37, 3 + xOffset, minmax[1].ToString(CultureInfo.InvariantCulture));

                for (int i = 0; i < 2; i++)
                {
                    double mismatchPercentage = Math.Round((minmax[i] - 30) / 30 * 100, 1);
                    string mismatch = $"{mismatchPercentage} ({minmax[i]})";
                    initSheet.Write(7 + xOffset, 19 + i, mismatch);
                }

                return minmax;
            }
            finally
            {
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@2)");
                    if (SEG_TEST != null)
                    {
                        SEG_TEST.Value = 0;
                        SEG_TEST.Write();
                    }
                    segSheet.AutoFit();
                }
                catch { }
            }
        }

        [ChipTest("AUTO", "Initial Test", "Start Initial Test Sequence")]
        private async Task RunInitialTestSeq(RunTestContext ctx, CancellationToken ct)
        {
            string step = "INIT";

            try
            {
                step = "CHECK_INSTRUMENTS";
                if (PowerSupply0 == null || DigitalMultimeter0 == null || DigitalMultimeter1 == null || DigitalMultimeter2 == null)
                    throw new Exception("Fail to connect Instruments: PowerSupply0 / DigitalMultimeter0~2");

                string[] pkgType = { "32QFN", "40QFN", "48QFN" };

                step = "SELECT_PKG";
                if (!SelectPkgType())
                    throw new OperationCanceledException("사용자에 의해 테스트가 중단되었습니다.");

                string initSheetName = "Initial_Test";
                string segSheetName = "Seg_Current";
                string[,] initialSpec =
                {
                    { "Item",           "Min",          "Typ",          "Max",          "Unit" },
                    { "POR Rising",     "2.15",         "2.32",         "2.52",         "V"  },
                    { "POR Falling",    "1.74",         "1.90",         "2.05",         "V" },
                    { "125_Ref",        "0.9300",       "0.9365",       "0.9430",       "V" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "150_Ref",        "1.004",        "1.009",        "1.014",        "V" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "PTAT_Out",       "0.7240",       "0.7277",       "0.7310",       "V" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "OSC_Freq",       "1.9630",       "1.9985",       "2.0400",       "MHz" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "REXT Current",   "29.10",        "29.99",        "31.00",        "mA" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "IBIAS of DRV",   "29.50",        "29.99",        "30.00",        "mA" },
                    { "RegValue",       "0",            "-",            "31",           "Decimal" },
                    { "Seg Mismatch",   "-",            "-0.0 (30)",    "+6.5 (32)",    "% (mA)"},
                    { "Seg Mismatch",   "-6.5 (28)",    "-0.0 (30)",    "-",            "% (mA)"},
                    { "EFUSE",          "-",            "PASS/FAIL",    "-",            "-"},
                    { "Active Current", "-",            "-",            "2.00",         "mA"},
                    { "Sleep Current",  "-",            "2.50",         "-",            "uA"}
                };

                IReportSheet initSheet;
                IReportSheet segSheet;
                int xOffset = 0;

                try
                {
                    initSheet = ctx.Report.SelectSheet(initSheetName);
                    while (true)
                    {
                        var val = initSheet.Read(7 + xOffset, 2);
                        if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                            break;
                        xOffset++;
                    }
                }
                catch
                {
                    initSheet = ctx.Report.CreateSheet(initSheetName);
                    initSheet.SetSheetFont("Consolas", 11);

                    initSheet.Write(2, 2, "Chip #");
                    initSheet.Merge(2, 2, 6, 2);

                    initSheet.Write(2, 3, "PKG");
                    initSheet.Merge(2, 3, 6, 3);

                    for (int i = 0; i < initialSpec.GetLength(0); i++)
                    {
                        for (int j = 0; j < initialSpec.GetLength(1); j++)
                        {
                            initSheet.Write(2 + j, 4 + i, initialSpec[i, j]);
                        }
                    }
                    initSheet.SetAlignmentCenterAll();
                    initSheet.SetBorderAll(2, 2, 6, 23);
                    initSheet.AutoFit();
                    xOffset = 0;
                }

                try
                {
                    segSheet = ctx.Report.SelectSheet(segSheetName);
                }
                catch
                {
                    segSheet = ctx.Report.CreateSheet(segSheetName);
                    segSheet.SetSheetFont("Consolas", 11);
                    segSheet.Write(2, 2, "Current (mA)");
                    segSheet.Write(3, 2, "Seg #");
                    for (int k = 1; k <= 32; k++)
                        segSheet.Write(3 + k, 2, $"SEG {k}");
                    segSheet.Write(36, 2, "Max");
                    segSheet.Write(37, 2, "Min");
                    segSheet.SetAlignmentCenterAll();
                    segSheet.SetBorderAll(2, 2, 37, 2);
                    segSheet.AutoFit();
                }

                if (xOffset > 0)
                {
                    AppendLog("INFO", $"Start with Chip #{xOffset + 1}.");
                }

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    var bestCodes = new Dictionary<string, int>();

                    ctx.Report.SelectSheet(initSheetName);
                    initSheet.Write(7 + xOffset, 2, (xOffset + 1).ToString());
                    int idx = (_segMode >= 1 && _segMode <= 3) ? _segMode - 1 : 0;
                    initSheet.Write(7 + xOffset, 3, pkgType[idx]);

                    step = $"CHIP#{xOffset + 1} POR_CONFIRM";
                    if (ShowMsg($"Chip #{xOffset + 1} 테스트 준비.\nRun POR Rising/Falling ?", "SCH1711 Initial Test",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                        throw new OperationCanceledException("사용자에 의해 테스트가 중단되었습니다.");

                    step = $"CHIP#{xOffset + 1} POR";
                    var por = await RunTestPowerOnReset(ct);
                    if (por != null)
                    {
                        initSheet.Write(7 + xOffset, 5, por[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 6, por[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} TSP_125_TRIM";
                    var r125 = await RunCal125Ref(ct);
                    if (r125 != null)
                    {
                        bestCodes["TSP_125_TRIM"] = (int)r125[1];
                        initSheet.Write(7 + xOffset, 7, r125[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 8, r125[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} TSP_150_TRIM";
                    var r150 = await RunCal150Ref(ct);
                    if (r150 != null)
                    {
                        bestCodes["TSP_150_TRIM"] = (int)r150[1];
                        initSheet.Write(7 + xOffset, 9, r150[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 10, r150[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} VPTAT_OUT_TRIM";
                    var rptat = await RunCalPtatOut(ct);
                    if (rptat != null)
                    {
                        bestCodes["VPTAT_OUT_TRIM"] = (int)rptat[1];
                        initSheet.Write(7 + xOffset, 11, rptat[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 12, rptat[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} OSC_CONFIRM";
                    if (ShowMsg("Run OSC Trim ?", "SCH1711 Initial Test",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                        throw new OperationCanceledException("사용자에 의해 테스트가 중단되었습니다.");

                    step = $"CHIP#{xOffset + 1} RC_OSC_BIAS_TRIM";
                    var rosc = await RunCalOscFreq(ct);
                    if (rosc != null)
                    {
                        bestCodes["RC_OSC_BIAS_TRIM"] = (int)rosc[1];
                        initSheet.Write(7 + xOffset, 13, rosc[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 14, rosc[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} CURRENT_CONFIRM";
                    if (ShowMsg("Run REXT/IBIAS/SEG Current ?", "SCH1711 Initial Test",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                        throw new OperationCanceledException("사용자에 의해 테스트가 중단되었습니다.");

                    step = $"CHIP#{xOffset + 1} BGR_OUT_TRIM";
                    var rrext = await RunCalRextCurrent(ct);
                    if (rrext != null)
                    {
                        bestCodes["BGR_OUT_TRIM"] = (int)rrext[1];
                        initSheet.Write(7 + xOffset, 15, rrext[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 16, rrext[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} DRV_IBIAS_TRIM";
                    var ribias = await RunCalIbiasOfDrv(ct);
                    if (ribias != null)
                    {
                        bestCodes["DRV_IBIAS_TRIM"] = (int)ribias[1];
                        initSheet.Write(7 + xOffset, 17, ribias[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 18, ribias[1].ToString(CultureInfo.InvariantCulture));
                    }

                    step = $"CHIP#{xOffset + 1} SEG_CURRENT";
                    ctx.Report.SelectSheet(segSheetName);
                    await RunMeasSegCurrent(segSheet, initSheet, xOffset, ct);
                    await Task.Delay(1000, ct);

                    step = $"CHIP#{xOffset + 1} EFUSE_BUILD";
                    ctx.Report.SelectSheet(initSheetName);
                    byte[] wdata = BuildEfuseData(bestCodes);

                    string confirmationMessage = "Check eFuse Parameters.\n\n";
                    confirmationMessage += "■ Trim Codes\n" + string.Join("\n", bestCodes.Select(kvp => $"  {kvp.Key,-20}: {kvp.Value}")) + "\n\n";
                    confirmationMessage += "■ Fixed Parameters\n";
                    confirmationMessage += $"  {"LsdRefTrim",-20}: {LsdRefTrim} (0b{Convert.ToString(LsdRefTrim, 2)})\n";
                    confirmationMessage += $"  {"LodRefTrim",-20}: {LodRefTrim} (0b{Convert.ToString(LodRefTrim, 2)})\n";
                    confirmationMessage += $"  {"BgrFeOutTrim",-20}: {BgrFeOutTrim} (0b{Convert.ToString(BgrFeOutTrim, 2)})\n";
                    confirmationMessage += $"  {"RcOscCapTrim",-20}: {RcOscCapTrim} (0b{Convert.ToString(RcOscCapTrim, 2)})\n";
                    confirmationMessage += $"  {"Crcin",-20}: {Crcin} (0b{Convert.ToString(Crcin, 2)})\n";
                    confirmationMessage += $"  {"IMax",-20}: {IMax} (0b{Convert.ToString(IMax, 2)})\n";
                    confirmationMessage += $"  {"_segMode",-20}: {_segMode} (0b{Convert.ToString(_segMode, 2)})\n";
                    confirmationMessage += $"  {"GridMode",-20}: {GridMode} (0b{Convert.ToString(GridMode, 2)})\n";
                    confirmationMessage += $"  {"ChgEn",-20}: {ChgEn} (0b{Convert.ToString(ChgEn, 2)})\n";
                    confirmationMessage += $"  {"DchgEn",-20}: {DchgEn} (0b{Convert.ToString(DchgEn, 2)})\n";
                    confirmationMessage += $"  {"Res",-20}: {Res} (0b{Convert.ToString(Res, 2)})\n";
                    confirmationMessage += $"  {"ProgramFlag",-20}: {ProgramFlag} (0b{Convert.ToString(ProgramFlag, 2)})\n";
                    confirmationMessage += $"  {"IReg",-20}: {IReg} (0b{Convert.ToString(IReg, 2)})\n";

                    step = $"CHIP#{xOffset + 1} EFUSE_CONFIRM";
                    if (ShowMsg(confirmationMessage, "eFuse Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        throw new OperationCanceledException("사용자에 의해 테스트가 중단되었습니다.");

                    step = $"CHIP#{xOffset + 1} EFUSE_WRITE";
                    for (uint i = 0; i < wdata.Length; i++)
                    {
                        if (_regCont != null)
                        {
                            var EF_WDATA = _regCont.RegMgr.GetRegisterItem(this, $"EF_WDATA{i}[7:0]");
                            if (EF_WDATA != null)
                            {
                                EF_WDATA.Value = wdata[i];
                                EF_WDATA.Write();
                            }
                        }
                        await Task.Delay(10, ct);
                    }

                    if (_regCont != null)
                    {
                        var EF_PASSCODE = _regCont.RegMgr.GetRegisterItem(this, "EF_PASSCODE[7:0]");
                        if (EF_PASSCODE != null)
                        {
                            EF_PASSCODE.Value = 0x38;
                            EF_PASSCODE.Write();
                        }
                    }
                    await Task.Delay(100, ct);

                    PowerSupply0.Write("VOLT 5.5, (@1)");
                    await Task.Delay(500, ct);

                    if (_regCont != null)
                    {
                        var EF_PGM = _regCont.RegMgr.GetRegisterItem(this, "pgm");
                        if (EF_PGM != null)
                        {
                            EF_PGM.Value = 1;
                            EF_PGM.Write();
                            await Task.Delay(100, ct);
                            EF_PGM.Value = 0;
                            EF_PGM.Write();
                        }
                    }
                    await Task.Delay(100, ct);

                    PowerSupply0.Write("VOLT 5, (@1)");
                    await Task.Delay(500, ct);

                    if (_regCont != null)
                    {
                        var EF_READ = _regCont.RegMgr.GetRegisterItem(this, "read");
                        if (EF_READ != null)
                        {
                            EF_READ.Value = 1;
                            EF_READ.Write();
                            await Task.Delay(100, ct);
                            EF_READ.Value = 0;
                            EF_READ.Write();
                        }
                    }
                    await Task.Delay(100, ct);

                    step = $"CHIP#{xOffset + 1} EFUSE_VERIFY";
                    byte[] readData = new byte[8];
                    bool isMatch = true;

                    for (uint i = 0; i < readData.Length; i++)
                    {
                        if (_regCont != null)
                        {
                            var EF_RDATA = _regCont.RegMgr.GetRegisterItem(this, $"EF_RDATA{i}[7:0]");
                            if (EF_RDATA != null)
                            {
                                EF_RDATA.Read();
                                readData[i] = (byte)EF_RDATA.Value;
                                if (wdata[i] != readData[i])
                                    isMatch = false;
                            }
                        }
                        await Task.Delay(100, ct);
                    }

                    if (isMatch)
                        initSheet.Write(7 + xOffset, 21, "PASS");
                    else
                    {
                        initSheet.Write(7 + xOffset, 21, "FAIL");
                        string writeStr = string.Join(" ", wdata.Select(b => $"0x{b:X2}"));
                        string readStr = string.Join(" ", readData.Select(b => $"0x{b:X2}"));
                        throw new Exception($"eFuse Verification FAILED.\n\nWritten: {writeStr}\nRead:    {readStr}");
                    }

                    step = $"CHIP#{xOffset + 1} STANDBY";
                    var standby = await RunTestStandbyCurrent(ct);
                    if (standby != null)
                    {
                        initSheet.Write(7 + xOffset, 22, standby[0].ToString(CultureInfo.InvariantCulture));
                        initSheet.Write(7 + xOffset, 23, standby[1].ToString(CultureInfo.InvariantCulture));
                    }

                    initSheet.AutoFit();
                    ctx.Report.Save();

                    try
                    {
                        PowerSupply0.Write("OUTP OFF, (@1:2)");
                    }
                    catch { }

                    step = $"CHIP#{xOffset + 1} NEXT?";
                    if (ShowMsg($"Chip #{xOffset + 1} Complete.\nContinue next Chip ({xOffset + 2})?", "Continue?",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        break;

                    xOffset++;
                }
            }
            catch (OperationCanceledException oce)
            {
                AppendLog("CANCEL", $"[{step}] {oce.Message}");
                Ui(() => MessageBox.Show($"테스트가 취소되었습니다.\n\nSTEP: {step}\nMSG: {oce.Message}",
                    "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information));
                throw;
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"[{step}] {ex.Message}");
                Ui(() => MessageBox.Show($"테스트 중 오류로 중단되었습니다.\n\nSTEP: {step}\nMSG: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                throw;
            }
            finally
            {
                try
                {
                    DisableTestAna();
                }
                catch { }
                try
                {
                    PowerSupply0?.Write("OUTP OFF, (@1:2)");
                }
                catch { }
                try
                {
                    DigitalMultimeter0?.Write("CONF:VOLT:AC AUTO");
                }
                catch { }
            }
        }

        #endregion AUTO TEST ITEMS

        #region LED EFFECT ITEMS
        private Tuple<int, int> GetGridDimensions()
        {
            uint reg408 = ReadRegister(0x408);
            uint reg401 = ReadRegister(0x401);
            bool is9Grid = ((reg408 >> 2) & 0x01) == 1;
            uint g_n = (reg401 >> 2) & 0x0F;

            int numGrids = is9Grid
                ? (g_n <= 7 ? (int)g_n + 1 : 9)
                : (g_n <= 8 ? (int)g_n + 1 : 10);

            int segmentsPerGrid = (reg408 & 0x03) switch
            {
                1 => 16,
                2 => 24,
                3 => 32,
                _ => 0
            };
            return new Tuple<int, int>(numGrids * segmentsPerGrid, segmentsPerGrid);
        }

        private bool CheckLEDEffectPrerequisites()
        {
            uint don = (ReadRegister(0x402) >> 3) & 0x01;

            if (don == 0)
            {
                MessageBox.Show("LED 효과를 시작할 수 없습니다.\n\n→ 0x402: DON = 0.",
                                "Fail to Start LED Effect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private readonly Dictionary<char, byte[]> Font = new Dictionary<char, byte[]>()
        {
            {'A', new byte[]{0x7E, 0x11, 0x11, 0x11, 0x7E}},
            {'B', new byte[]{0x7F, 0x49, 0x49, 0x49, 0x36}},
            {'C', new byte[]{0x3E, 0x41, 0x41, 0x41, 0x22}},
            {'D', new byte[]{0x7F, 0x41, 0x41, 0x22, 0x1C}},
            {'E', new byte[]{0x7F, 0x49, 0x49, 0x49, 0x41}},
            {'F', new byte[]{0x7F, 0x09, 0x09, 0x09, 0x01}},
            {'G', new byte[]{0x3E, 0x41, 0x49, 0x49, 0x7A}},
            {'H', new byte[]{0x7F, 0x08, 0x08, 0x08, 0x7F}},
            {'I', new byte[]{0x00, 0x41, 0x7F, 0x41, 0x00}},
            {'J', new byte[]{0x20, 0x40, 0x41, 0x3F, 0x01}},
            {'K', new byte[]{0x7F, 0x08, 0x14, 0x22, 0x41}},
            {'L', new byte[]{0x7F, 0x40, 0x40, 0x40, 0x40}},
            {'M', new byte[]{0x7F, 0x02, 0x04, 0x02, 0x7F}},
            {'N', new byte[]{0x7F, 0x04, 0x08, 0x10, 0x7F}},
            {'O', new byte[]{0x3E, 0x41, 0x41, 0x41, 0x3E}},
            {'P', new byte[]{0x7F, 0x09, 0x09, 0x09, 0x06}},
            {'Q', new byte[]{0x3E, 0x41, 0x51, 0x21, 0x5E}},
            {'R', new byte[]{0x7F, 0x09, 0x19, 0x29, 0x46}},
            {'S', new byte[]{0x46, 0x49, 0x49, 0x49, 0x31}},
            {'T', new byte[]{0x01, 0x01, 0x7F, 0x01, 0x01}},
            {'U', new byte[]{0x3F, 0x40, 0x40, 0x40, 0x3F}},
            {'V', new byte[]{0x1F, 0x20, 0x40, 0x20, 0x1F}},
            {'W', new byte[]{0x3F, 0x40, 0x38, 0x40, 0x3F}},
            {'X', new byte[]{0x63, 0x14, 0x08, 0x14, 0x63}},
            {'Y', new byte[]{0x07, 0x08, 0x70, 0x08, 0x07}},
            {'Z', new byte[]{0x61, 0x51, 0x49, 0x45, 0x43}},
            {'0', new byte[]{0x3E, 0x51, 0x49, 0x45, 0x3E}},
            {'1', new byte[]{0x00, 0x42, 0x7F, 0x40, 0x00}},
            {'2', new byte[]{0x42, 0x61, 0x51, 0x49, 0x46}},
            {'3', new byte[]{0x21, 0x41, 0x45, 0x4B, 0x31}},
            {'4', new byte[]{0x18, 0x14, 0x12, 0x7F, 0x10}},
            {'5', new byte[]{0x27, 0x45, 0x45, 0x45, 0x39}},
            {'6', new byte[]{0x3C, 0x4A, 0x49, 0x49, 0x30}},
            {'7', new byte[]{0x01, 0x71, 0x09, 0x05, 0x03}},
            {'8', new byte[]{0x36, 0x49, 0x49, 0x49, 0x36}},
            {'9', new byte[]{0x06, 0x49, 0x49, 0x29, 0x1E}},
            {':', new byte[]{0x00, 0x24, 0x24, 0x00, 0x00}},
            {' ', new byte[]{0x00, 0x00, 0x00, 0x00, 0x00}},
        };

        [ChipTest("LED", "Wave Effect", "Run Led Matrix Effect Wave")]
        private async Task RunLedEffectWave(Func<string, string, Task> log, CancellationToken ct)
        {
            try
            {
                if (!CheckLEDEffectPrerequisites())
                    return;

                var dimensions = GetGridDimensions();
                int totalBytes = dimensions.Item1;
                int segmentsPerGrid = dimensions.Item2;

                if (totalBytes == 0 || segmentsPerGrid == 0)
                {
                    MessageBox.Show("계산된 LED 개수가 0입니다. 칩 설정을 확인하세요.", "RunLedEffectWave", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                uint frame = 0;
                double WAVE_FREQUENCY = 0.06;
                byte[] ledData = new byte[totalBytes];
                bool UP = (ReadRegister(0x401) & 0x01) == 1;

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    for (int i = 0; i < totalBytes; i++)
                    {
                        int r = i / segmentsPerGrid;
                        int c = i % segmentsPerGrid;

                        double sinInput = (c + r + frame) * WAVE_FREQUENCY;
                        double sinOutput = Math.Sin(sinInput);
                        byte brightness = (byte)(255 - (((sinOutput + 1.0) / 2.0) * 255));
                        if (brightness < 0)
                            brightness = 0;

                        int led_index = r * segmentsPerGrid + ((segmentsPerGrid - 1) - c);

                        if (led_index < totalBytes)
                        {
                            ledData[led_index] = brightness;
                        }
                    }

                    WriteLEDData((uint)totalBytes, ledData);
                    if (!UP)
                        WriteCommand(0x04);

                    frame++;

                    await Task.Delay(10, ct);
                }
            }
            catch { }
        }

        [ChipTest("LED", "Breathing Effect", "Run Led Matrix Effect Breathing")]
        private async Task RunLedEffectBreathing(Func<string, string, Task> log, CancellationToken ct)
        {
            try
            {
                if (!CheckLEDEffectPrerequisites())
                    return;

                var dimensions = GetGridDimensions();
                int totalBytes = dimensions.Item1;
                int segmentsPerGrid = dimensions.Item2;
                if (totalBytes == 0 || segmentsPerGrid == 0)
                {
                    MessageBox.Show("계산된 LED 개수가 0입니다. 칩 설정을 확인하세요.", "RunLedEffectBreathing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                uint frame = 0;
                byte[] ledData = new byte[totalBytes];
                bool UP = (ReadRegister(0x401) & 0x01) == 1;

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    double sinInput = frame * 0.05;
                    double sinOutput = Math.Sin(sinInput);
                    byte brightness = (byte)(((sinOutput + 1.0) / 2.0) * 255);

                    for (int i = 0; i < totalBytes; i++)
                        ledData[i] = brightness;

                    WriteLEDData((uint)totalBytes, ledData);
                    if (!UP)
                        WriteCommand(0x04);

                    await Task.Delay(10, ct);
                    frame++;
                }
            }
            catch { }
        }

        [ChipTest("LED", "Scroll Text Effect", "Run Led Matrix Effect Scroll Text")]
        private async Task RunLedEffectScrollText(Func<string, string, Task> log, CancellationToken ct)
        {
            if (!CheckLEDEffectPrerequisites())
                return;

            DialogResult result = MessageBox.Show("배경을 켜시겠습니까?\n(아니오: 글자 켜기)", "RunLedEffectScrollText", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            bool invertDisplay = (result == DialogResult.Yes);

            string inputText = RegisterControlForm.Prompt.ShowDialog("스크롤할 문자를 입력하세요 (최대 20글자):", "RunLedEffectScrollText");
            if (string.IsNullOrWhiteSpace(inputText))
                return;

            if (inputText.Length > 20)
            {
                MessageBox.Show("최대 20글자까지만 입력할 수 있습니다.", "RunLedEffectScrollText", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            inputText = inputText.ToUpper();

            List<byte> scrollBuffer = new List<byte>();
            foreach (char ch in inputText)
            {
                if (Font.ContainsKey(ch))
                {
                    scrollBuffer.AddRange(Font[ch]);

                    if (ch != ' ')
                    {
                        scrollBuffer.Add(0x00);
                    }
                }
            }
            if (scrollBuffer.Count == 0)
                return;

            for (int i = 0; i < 32; i++)
            {
                scrollBuffer.Add(0x00);
            }

            var dimensions = GetGridDimensions();
            int totalBytes = dimensions.Item1;
            if (totalBytes != 320)
            {
                MessageBox.Show("해당 Effect는 G1~10 & SEG1~32를 모두 사용합니다. 칩 설정을 확인하세요.", "RunLedEffectScrollText", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int frame = 0;
            byte[] ledData = new byte[totalBytes];
            bool UP = (ReadRegister(0x401) & 0x01) == 1;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                for (int grid_col = 0; grid_col < 32; grid_col++)
                {
                    int buffer_index = (grid_col + frame) % scrollBuffer.Count;
                    byte columnData = scrollBuffer[buffer_index];

                    for (int grid_row = 0; grid_row < 10; grid_row++)
                    {
                        int led_index = grid_row * 32 + (31 - grid_col);

                        if (grid_row > 0 && grid_row < 9)
                        {
                            bool isPixelOn = ((columnData >> (grid_row - 1)) & 0x01) == 1;

                            if (invertDisplay)
                            {
                                ledData[led_index] = isPixelOn ? (byte)0 : (byte)255;
                            }
                            else
                            {
                                ledData[led_index] = isPixelOn ? (byte)255 : (byte)0;
                            }
                        }
                        else
                        {
                            ledData[led_index] = invertDisplay ? (byte)255 : (byte)0;
                        }
                    }
                }

                WriteLEDData((uint)totalBytes, ledData);
                if (!UP)
                    WriteCommand(0x04);

                await Task.Delay(100, ct);
                frame++;
            }
        }

        [ChipTest("LED", "Digital Clock Effect", "Run Led Matrix Effect Digital Clock")]
        private async Task RunLedEffectDigitalClock(Func<string, string, Task> log, CancellationToken ct)
        {
            if (!CheckLEDEffectPrerequisites())
                return;

            var dimensions = GetGridDimensions();
            int totalBytes = dimensions.Item1;
            if (totalBytes != 320)
            {
                MessageBox.Show("해당 Effect는 G1~10 & SEG 1~32를 모두 사용합니다. 칩 설정을 확인하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte[] ledData = new byte[totalBytes];
            bool UP = (ReadRegister(0x401) & 0x01) == 1;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                string timeString = DateTime.Now.ToString("HH:mm");

                bool showColon = (DateTime.Now.Millisecond >= 500);

                List<byte> textBuffer = new List<byte>();
                foreach (char ch in timeString)
                {
                    char charToRender = ch;
                    if (ch == ':' && !showColon)
                    {
                        charToRender = ' ';
                    }

                    if (Font.ContainsKey(charToRender))
                    {
                        textBuffer.AddRange(Font[charToRender]);
                        textBuffer.Add(0x00);
                    }
                }

                int textWidth = textBuffer.Count;
                int startCol = (32 - textWidth) / 2;
                Array.Clear(ledData, 0, ledData.Length);

                for (int c = 0; c < textWidth; c++)
                {
                    int physical_col = startCol + c;
                    if (physical_col < 0 || physical_col >= 32)
                        continue;

                    byte columnData = textBuffer[c];

                    for (int grid_row = 0; grid_row < 10; grid_row++)
                    {
                        int led_index = grid_row * 32 + (31 - physical_col);

                        if (grid_row > 0 && grid_row < 9)
                        {
                            if (((columnData >> (grid_row - 1)) & 0x01) == 1)
                            {
                                ledData[led_index] = 255;
                            }
                        }
                    }
                }

                WriteLEDData((uint)totalBytes, ledData);
                if (!UP)
                    WriteCommand(0x04);

                int currentMillisecond = DateTime.Now.Millisecond;
                int delay = (currentMillisecond < 500) ? (500 - currentMillisecond) : (1000 - currentMillisecond);
                await Task.Delay(delay, ct);
            }
        }
        #endregion LED EFFECT ITEMS
    }
}