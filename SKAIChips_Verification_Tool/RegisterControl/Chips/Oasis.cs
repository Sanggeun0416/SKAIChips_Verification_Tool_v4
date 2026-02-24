namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Oasis : ProjectBase
    {
        public override string Name => "Oasis";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Oasis",
            "SCP1701"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.I2C };

        public override uint ComFrequency => 400;
        public override byte DeviceAddress => 0x52;

        public Oasis()
        {
        }

        public Oasis(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return null;/*new[]
            {
                new TestSlotAction("Oasis Init", () => MessageBox.Show("Running Oasis Initialization..."))
            };*/
        }

        public bool PrepareTest(string testId, ITestUiContext uiContext)
        {
            if (testId == "FW.FLASH_WRITE")
            {
                string? filePath = uiContext.OpenFileDialog("FW File (*.bin;*.hex)|*.bin;*.hex|All files (*.*)|*.*", "Select Firmware File");
                if (string.IsNullOrEmpty(filePath))
                    return false;

                SetFirmwareFilePath(filePath);
            }
            else if (testId == "FW.FLASH_VERIFY" || testId == "FW.FLASH_READ")
            {
                string? sizeStr = uiContext.PromptInput("FLASH FUNCTION", "Enter the Flash Size[Byte]:", "524288");

                if (string.IsNullOrEmpty(sizeStr) || !uint.TryParse(sizeStr, out uint flashSize) || flashSize == 0)
                    return false;

                SetFlashSize(flashSize);
            }

            return true;
        }

        private const uint RegI2cId = 0x5000_0000;
        private const uint RegFlashCmd = 0x5009_0008;
        private const uint RegFlashStatus = 0x5009_0020;
        private const uint RegFlashStatusAlt = 0x5009_000C;
        private const uint RegFlashTxBase = 0x5009_1000;

        private string _firmwareFilePath = string.Empty;
        private uint _flashSizeBytes = 0;

        public override void WriteRegister(uint address, uint data)
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            Span<byte> cmd = stackalloc byte[8];
            cmd[0] = 0xA1;
            cmd[1] = 0x2C;
            cmd[2] = 0x12;
            cmd[3] = 0x34;
            cmd[4] = (byte)((address >> 24) & 0xFF);
            cmd[5] = (byte)((address >> 16) & 0xFF);
            cmd[6] = (byte)((address >> 8) & 0xFF);
            cmd[7] = (byte)(address & 0xFF);
            I2cBus.Write(DeviceAddress, cmd, stop: true);

            Span<byte> dataBytes = stackalloc byte[4];
            dataBytes[0] = (byte)(data & 0xFF);
            dataBytes[1] = (byte)((data >> 8) & 0xFF);
            dataBytes[2] = (byte)((data >> 16) & 0xFF);
            dataBytes[3] = (byte)((data >> 24) & 0xFF);
            I2cBus.Write(DeviceAddress, dataBytes, stop: true);
        }

        public override uint ReadRegister(uint address)
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            Span<byte> cmd = stackalloc byte[8];
            cmd[0] = 0xA1;
            cmd[1] = 0x2C;
            cmd[2] = 0x12;
            cmd[3] = 0x34;
            cmd[4] = (byte)((address >> 24) & 0xFF);
            cmd[5] = (byte)((address >> 16) & 0xFF);
            cmd[6] = (byte)((address >> 8) & 0xFF);
            cmd[7] = (byte)(address & 0xFF);
            I2cBus.Write(DeviceAddress, cmd, stop: true);

            Span<byte> rcv = stackalloc byte[4];
            I2cBus.Read(DeviceAddress, rcv, timeoutMs: 200);
            return (uint)((rcv[3] << 24) | (rcv[2] << 16) | (rcv[1] << 8) | rcv[0]);
        }

        internal void HaltMcu()
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            Span<byte> cmd = [0xA1, 0x2C, 0x56, 0x78];
            I2cBus.Write(DeviceAddress, cmd, stop: true);
            Thread.Sleep(50);
        }

        internal void ResetMcu()
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            Span<byte> cmd = [0xA1, 0x2C, 0xAB, 0xCD];
            I2cBus.Write(DeviceAddress, cmd, stop: true);
            Thread.Sleep(50);
        }

        #region MANUAL TEST ITEMS
        [ChipTest("MANUAL", "GPIO_DISABLE", "GPIO Disable")]
        public async Task ManualGpioDisable()
        {
            try
            {
                uint reg50110014 = ReadRegister(0x5011_0014);     // OSEL_16
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN
                uint regDC34041C = ReadRegister(0xDC34_041C);     // w_PWR_SRAM_CRET_Static[4]
                uint regDC340434 = ReadRegister(0xDC34_0434);     // w_PWR_SRAM_MD[4]
                uint regDC340080 = ReadRegister(0xDC34_0080);     // O_GPADC_PEN
                uint regDC340098 = ReadRegister(0xDC34_0098);     // O_TS_PEN
                uint regDC3400A0 = ReadRegister(0xDC34_00A0);     // w_XO_RFC_CLK_EN

                WriteRegister(0x5011_0014, reg50110014 & (0xFFFF_FF16u));
                WriteRegister(0xDC34_0050, regDC340050 & ~(1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 & ~(15u));
                WriteRegister(0xDC34_006C, regDC34006C & ~(1u << 15));
                WriteRegister(0xDC34_041C, regDC34041C & ~(1u << 14));
                WriteRegister(0xDC34_0434, regDC340434 & ~(1u << 15));
                WriteRegister(0xDC34_0080, regDC340080 & ~(1u << 9));
                WriteRegister(0xDC34_0098, regDC340098 & ~(1u));
                WriteRegister(0xDC34_00A0, regDC3400A0 & ~(1u << 15));
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO Disable: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_04_ABGR", "GPIO4 to ABGR")]
        public async Task ManualGpio4Abgr()
        {
            try
            {
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN

                WriteRegister(0xDC34_0050, regDC340050 | (1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 | 15u);
                WriteRegister(0xDC34_006C, regDC34006C | (1u << 15));
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO4 to ABGR: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_04_RETLDO", "GPIO4 to RETLDO")]
        public async Task ManualGpio4RetLdo()
        {
            try
            {
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN
                uint regDC34041C = ReadRegister(0xDC34_041C);     // w_PWR_SRAM_CRET_Static[4]
                uint regDC340434 = ReadRegister(0xDC34_0434);     // w_PWR_SRAM_MD[4]

                WriteRegister(0xDC34_041C, regDC34041C | 1u << 14);
                WriteRegister(0xDC34_0434, regDC340434 | 1u << 15);
                WriteRegister(0xDC34_0050, regDC340050 | (1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 | (11u));
                WriteRegister(0xDC34_006C, regDC34006C | (1u << 15));
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO4 to RETLDO: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_04_MBGR", "GPIO4 to MBGR")]
        public async Task ManualGpio4Mbgr()
        {
            try
            {
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN
                uint regDC340080 = ReadRegister(0xDC34_0080);     // O_GPADC_PEN

                WriteRegister(0xDC34_0050, regDC340050 | (1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 | (9u));
                WriteRegister(0xDC34_006C, regDC34006C | (1u << 15));
                WriteRegister(0xDC34_0080, regDC340080 | (1u << 9));
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO4 to MBGR: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_04_DALDO", "GPIO4 to DALDO")]
        public async Task ManualGpio4DaLdo()
        {
            try
            {
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN
                uint regDC340080 = ReadRegister(0xDC34_0080);     // O_GPADC_PEN

                WriteRegister(0xDC34_0050, regDC340050 | (1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 | (7u));
                WriteRegister(0xDC34_006C, regDC34006C | (1u << 15));
                WriteRegister(0xDC34_0080, regDC340080 | (1u << 9));

            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO4 to DALDO: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_04_TEMPSENSOR", "GPIO4 to TempSensor")]
        public async Task ManualGpio4TempSensor()
        {
            try
            {
                uint regDC340050 = ReadRegister(0xDC34_0050);     // O_AN_TEST_EN
                uint regDC340054 = ReadRegister(0xDC34_0054);     // O_AN_TEST_MUX[2:0] & O_GPIO4_AN_EN
                uint regDC34006C = ReadRegister(0xDC34_006C);     // O_GPIO_TEST_BUF_EN
                uint regDC340098 = ReadRegister(0xDC34_0098);     // O_TS_PEN

                WriteRegister(0xDC34_0050, regDC340050 | (1u << 15));
                WriteRegister(0xDC34_0054, regDC340054 | (13u));
                WriteRegister(0xDC34_006C, regDC34006C | (1u << 15));
                WriteRegister(0xDC34_0098, regDC340098 | (1u));
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO4 to DALDO: {ex.Message}");
                throw;
            }
        }

        [ChipTest("MANUAL", "GPIO_16_32KOSC", "GPIO16 to 32KOSC")]
        public async Task ManualGpio432kOsc()
        {
            try
            {
                uint reg50110014 = ReadRegister(0x5011_0014);     // OSEL_16
                uint regDC3400A0 = ReadRegister(0xDC34_00A0);     // w_XO_RFC_CLK_EN

                WriteRegister(0x5011_0014, (reg50110014 & 0xFFFF_FF00u) | 69u);
                WriteRegister(0xDC34_00A0, regDC3400A0 | 1u << 15);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"Error in GPIO16 to 32KOSC: {ex.Message}");
                throw;
            }
        }
        #endregion MANUAL TEST ITEMS

        #region AUTO TEST ITEMS
        private async Task TogglePllPen()
        {
            var regDC340030 = ReadRegister(0xDC34_0030);     // w_PLL_PEN

            WriteRegister(0xDC34_0030, regDC340030 & ~(1u << 14));
            await Task.Delay(100);

            WriteRegister(0xDC34_0030, regDC340030 | (1u << 14));
            await Task.Delay(100);
        }

        private async Task<double[]?> AutoTrimAbgr(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (DigitalMultimeter2 == null)
                throw new InvalidOperationException("DigitalMultimeter2 is not connected.");

            var ABGR_CONT = _regCont.RegMgr.GetRegisterItem(this, "O_ABGR_CONT[3:0]");

            uint[] bgr_cont = { 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7 };
            double dmm_volt_mv = 0, bgr_target_mv = 300;
            int left = 0, mid = 0, right = bgr_cont.Length - 1;
            double[] trim_value = { 0, 0 };

            await ManualGpio4Abgr();

            while (left <= right)
            {
                ct.ThrowIfCancellationRequested();

                mid = (left + right) / 2;
                ABGR_CONT.Read();
                ABGR_CONT.Value = bgr_cont[mid];
                ABGR_CONT.Write();
                await Task.Delay(10);

                dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

                if (Math.Abs(dmm_volt_mv - bgr_target_mv) <= 2)
                {
                    break;
                }
                if (dmm_volt_mv >= bgr_target_mv)
                {
                    right = mid - 1;
                }
                else if (dmm_volt_mv <= bgr_target_mv)
                {
                    left = mid + 1;
                }
            }
            ABGR_CONT.Value = bgr_cont[mid];
            ABGR_CONT.Write();
            await Task.Delay(10);

            dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

            trim_value[0] = bgr_cont[mid];
            trim_value[1] = dmm_volt_mv;

            await ManualGpioDisable();

            return trim_value;
        }

        private async Task<double[]?> AutoTrimMldo(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (DigitalMultimeter1 == null)
                throw new InvalidOperationException("DigitalMultimeter1 is not connected.");

            var MLDO_CONT = _regCont.RegMgr.GetRegisterItem(this, "O_MLDO_CONT[5:0]");

            uint[] mldo_cont = {
                8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7,
                24, 25, 26, 27, 28, 29, 30, 31, 16, 17, 18, 19, 20, 21,
                22, 23, 40, 41, 42, 43, 44, 45, 46, 47, 32, 33, 34, 35,
                36, 37, 38, 39, 56, 57, 58, 59, 60, 61, 62, 63, 48, 49,
                50, 51, 52, 53, 54, 55
            };
            double dmm_volt_mv = 0, mldo_target_mv = 950;
            int left = 0, mid = 0, right = mldo_cont.Length - 1;
            double[] trim_value = { 0, 0 };

            while (left <= right)
            {
                mid = (left + right) / 2;
                MLDO_CONT.Read();
                MLDO_CONT.Value = mldo_cont[mid];
                MLDO_CONT.Write();
                await Task.Delay(10);

                dmm_volt_mv = double.Parse(DigitalMultimeter1.Query(":MEAS:VOLT:DC?")) * 1000;

                if (Math.Abs(dmm_volt_mv - mldo_target_mv) <= 4.5)
                {
                    break;
                }
                if (dmm_volt_mv >= mldo_target_mv)
                {
                    right = mid - 1;
                }
                else if (dmm_volt_mv <= mldo_target_mv)
                {
                    left = mid + 1;
                }
            }

            MLDO_CONT.Value = mldo_cont[mid];
            MLDO_CONT.Write();
            await Task.Delay(10);

            dmm_volt_mv = double.Parse(DigitalMultimeter1.Query(":MEAS:VOLT:DC?")) * 1000;

            trim_value[0] = mldo_cont[mid];
            trim_value[1] = dmm_volt_mv;

            return trim_value;
        }

        private async Task<double[]?> AutoTrimAldo(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (DigitalMultimeter0 == null)
                throw new InvalidOperationException("DigitalMultimeter0 is not connected.");

            var ALDO_CONT = _regCont.RegMgr.GetRegisterItem(this, "O_ALDO_CONT[5:0]");

            uint[] aldo_cont = {
                24, 25, 26, 27, 28, 29, 30, 31,
                16, 17, 18, 19, 20, 21, 22, 23,
                56, 57, 58, 59, 60, 61, 62, 63,
                48, 49, 50, 51, 52, 53, 54, 55,
                8, 9, 10, 11, 12, 13, 14, 15,
                0, 1, 2, 3, 4, 5, 6, 7,
                40, 41, 42, 43, 44, 45, 46, 47,
                32, 33, 34, 35, 36, 37, 38, 39
            };
            double dmm_volt_mv = 0, aldo_target_mv = 900;
            int left = 0, mid = 0, right = aldo_cont.Length - 1;
            double[] trim_value = { 0, 0 };

            while (left <= right)
            {
                mid = (left + right) / 2;
                ALDO_CONT.Read();
                ALDO_CONT.Value = aldo_cont[mid];
                ALDO_CONT.Write();
                await Task.Delay(10);

                dmm_volt_mv = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?")) * 1000;

                if (Math.Abs(dmm_volt_mv - aldo_target_mv) <= 4.5)
                {
                    break;
                }
                if (dmm_volt_mv >= aldo_target_mv)
                {
                    right = mid - 1;
                }
                else if (dmm_volt_mv <= aldo_target_mv)
                {
                    left = mid + 1;
                }
            }

            ALDO_CONT.Value = aldo_cont[mid];
            ALDO_CONT.Write();
            await Task.Delay(10);

            dmm_volt_mv = double.Parse(DigitalMultimeter0.Query(":MEAS:VOLT:DC?")) * 1000;

            trim_value[0] = aldo_cont[mid];
            trim_value[1] = dmm_volt_mv;

            return trim_value;
        }

        private async Task<double[]?> AutoTrimFldo(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (DigitalMultimeter2 == null)
                throw new InvalidOperationException("DigitalMultimeter2 is not connected.");

            var FLDO_CONT = _regCont.RegMgr.GetRegisterItem(this, "O_FLDO_CONT[3:0]");

            uint[] fldo_cont = { 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7 };
            double dmm_volt_mv = 0, fldo_target_mv = 1.800;
            int left = 0, mid = 0, right = fldo_cont.Length - 1;
            double[] trim_value = { 0, 0 };

            while (left <= right)
            {
                mid = (left + right) / 2;
                FLDO_CONT.Read();
                FLDO_CONT.Value = fldo_cont[mid];
                FLDO_CONT.Write();
                await Task.Delay(10);

                dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

                if (Math.Abs(dmm_volt_mv - fldo_target_mv) <= 2)
                {
                    break;
                }
                if (dmm_volt_mv >= fldo_target_mv)
                {
                    right = mid - 1;
                }
                else if (dmm_volt_mv <= fldo_target_mv)
                {
                    left = mid + 1;
                }
            }
            FLDO_CONT.Value = fldo_cont[mid];
            FLDO_CONT.Write();
            await Task.Delay(10);

            dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

            trim_value[0] = fldo_cont[mid];
            trim_value[1] = dmm_volt_mv;

            return trim_value;
        }

        private async Task<double[]?> AutoTrimDaldo(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (DigitalMultimeter2 == null)
                throw new InvalidOperationException("DigitalMultimeter2 is not connected.");

            var DALDO_CONT = _regCont.RegMgr.GetRegisterItem(this, "O_DA_LDO_CONT[5:0]");

            uint[] daldo_cont = {
                8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7,
                24, 25, 26, 27, 28, 29, 30, 31, 16, 17, 18, 19, 20, 21,
                22, 23, 40, 41, 42, 43, 44, 45, 46, 47, 32, 33, 34, 35,
                36, 37, 38, 39, 56, 57, 58, 59, 60, 61, 62, 63, 48, 49,
                50, 51, 52, 53, 54, 55
            };
            double dmm_volt_mv = 0, daldo_target_mv = 450;
            int left = 0, mid = 0, right = daldo_cont.Length - 1;
            double[] trim_value = { 0, 0 };

            while (left <= right)
            {
                mid = (left + right) / 2;
                DALDO_CONT.Read();
                DALDO_CONT.Value = daldo_cont[mid];
                DALDO_CONT.Write();
                System.Threading.Thread.Sleep(10);

                dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

                if (Math.Abs(dmm_volt_mv - daldo_target_mv) <= 2)
                {
                    break;
                }
                if (dmm_volt_mv >= daldo_target_mv)
                {
                    right = mid - 1;
                }
                else if (dmm_volt_mv <= daldo_target_mv)
                {
                    left = mid + 1;
                }
            }

            DALDO_CONT.Value = daldo_cont[mid];
            DALDO_CONT.Write();
            System.Threading.Thread.Sleep(10);

            dmm_volt_mv = double.Parse(DigitalMultimeter2.Query(":MEAS:VOLT:DC?")) * 1000;

            trim_value[0] = daldo_cont[mid];
            trim_value[1] = dmm_volt_mv;

            return trim_value;
        }

        private async Task AutoSetGpadc(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            //GPIO01_AN_EN.Value = 1;
            uint reg0xDC34_0050 = ReadRegister(0xDC34_0050);
            WriteRegister(0xDC34_0050, reg0xDC34_0050 | (1 << 13));

            //DIV_FACTOR.Value = 5;
            //GPADC_ATT_VSEL.Value = 1;
            uint reg0xDC34_0094 = ReadRegister(0xDC34_0094);
            WriteRegister(0xDC34_0094, reg0xDC34_0050 | (5 << 12) | (1 << 7));

            //DIV_EN.Value = 1;
            uint reg0xDC34_0090 = ReadRegister(0xDC34_0090);
            WriteRegister(0xDC34_0090, reg0xDC34_0050 | (1 << 15));

            //GPADC_ATT_BYP_EN.Value = 0;
            //GPADC_BUF_EN.Value = 1;
            //GPADC_MUXSEL.Value = 0;
            uint reg0xDC34_008C = ReadRegister(0xDC34_008C);

            //GAIN_SEL.Value = 1;
            uint reg0xDC34_0084 = ReadRegister(0xDC34_0084);
            WriteRegister(0xDC34_0084, reg0xDC34_0050 | (1 << 12));

            //GPADC_CLK_SEL.Value = 1;
            //MODE.Value = 15;
            //RES_SEL.Value = 3;
            //GPADC_PEN.Value = 1;
            //GPADC_EN.Value = 1;
            //ADC_SOC.Value = 1;
            uint reg0xDC34_0080 = ReadRegister(0xDC34_0080);
            WriteRegister(0xDC34_0080, reg0xDC34_0050 | (1 << 11) | (15 << 4) | (3 << 2) | (1 << 1) | 1);
        }

        private async Task AutoMeasureVcoRange(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (SpectrumAnalyzer == null)
                throw new InvalidOperationException("SpectrumAnalyzer is not connected.");

            IReportSheet vcoSheet;

            string time = DateTime.Now.ToString("HHmmss");
            vcoSheet = ctx.Report.CreateSheet($"{time}_VCO");
            vcoSheet.SetSheetFont("Consolas", 10);

            vcoSheet.Write(1, 1, $"w_EXT_CAPS[9:0]");
            vcoSheet.Write(2, 1, $"Freq[MHz]");

            uint reg0xDC34_0020 = ReadRegister(0xDC34_0020);    // AFE_REG 0x08
            uint reg0xDC34_00A4 = ReadRegister(0xDC34_00A4);    // AFE_REG 0x29
            uint reg0xDC34_00B4 = ReadRegister(0xDC34_00B4);    // AFE_REG 0x2D
            uint reg0xDC34_00C4 = ReadRegister(0xDC34_00C4);    // AFE_REG 0x31

            //O_SPI_RSV_CORE[7:0].Value = 0x01;
            //xo_cl_n[5:0].Value = 0x20;
            WriteRegister(0xDC34_00A4, (reg0xDC34_00A4 & 0x00C0) | 0x0120);

            //w_BUF_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_PRE_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_DA_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_TRX_SEL_SRC_SEL[1:0].Value = 0x2;
            //r_TX_DA_PEN.Value = 0x1;
            //r_TX_PRE_PEN.Value = 0x1;
            //r_TX_BUF_PEN.Value = 0x1;
            //w_TX_DA_PEN_MODE.Value = 0x1;
            //w_TX_PRE_PEN_MODE.Value = 0x1;
            WriteRegister(0xDC34_00B4, (reg0xDC34_00B4 & 0xE000) | 0x155F);

            //w_TX_DA_EN.Value = 0x1;
            //w_TX_PRE_EN.Value = 0x1;
            //w_TX_BUF_EN.Value = 0x1;
            //w_TX_EN.Value = 0x1;
            //w_PLL_DIV2_OUT_EN.Value = 0x1;
            //w_PLL_OUT_EN.Value = 0x1;
            //w_PLL_EN.Value = 0x1;
            WriteRegister(0xDC34_00C4, (reg0xDC34_00C4 & 0xE1F1) | 0x1E0E);

            //w_EXT_CAPS[9:0].Value = 0;
            WriteRegister(0xDC34_0020, (reg0xDC34_0020 & 0xFE00) | 0);
            await Task.Delay(100, ct);

            SpectrumAnalyzer.Write("DISP:TRAC:Y:RLEV 10dBm");
            SpectrumAnalyzer.Write("FREQ:CENT 2.5 GHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 1.0 GHz");

            await TogglePllPen();
            await Task.Delay(1000, ct);

            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000, ct);

            double freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            vcoSheet.Write(1, 2, $"0");
            vcoSheet.Write(2, 2, $"{freqMHz}");

            //w_EXT_CAPS[9:0].Value = 512;
            reg0xDC34_0020 = ReadRegister(0xDC34_0020);
            WriteRegister(0xDC34_0020, (reg0xDC34_0020 & 0xFE00) | 512);

            await TogglePllPen();
            await Task.Delay(1000, ct);

            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000, ct);

            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            vcoSheet.Write(1, 3, $"512");
            vcoSheet.Write(2, 3, $"{freqMHz}");

            //w_EXT_CAPS[9:0].Value = 1023;
            reg0xDC34_0020 = ReadRegister(0xDC34_0020);
            WriteRegister(0xDC34_0020, (reg0xDC34_0020 & 0xFE00) | 1023);

            await TogglePllPen();
            await Task.Delay(1000, ct);

            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000, ct);

            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            vcoSheet.Write(1, 4, $"1023");
            vcoSheet.Write(2, 4, $"{freqMHz}");

            reg0xDC34_0020 = ReadRegister(0xDC34_0020);
            WriteRegister(0xDC34_0020, reg0xDC34_0020 & 0xBE00);
        }

        private async Task AutoMeasureTxPower(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (SpectrumAnalyzer == null)
                throw new InvalidOperationException("SpectrumAnalyzer is not connected.");

            IReportSheet txSheet;

            string time = DateTime.Now.ToString("HHmmss");
            txSheet = ctx.Report.CreateSheet($"{time}_TX");
            txSheet.SetSheetFont("Consolas", 10);

            txSheet.Write(1, 1, $"TX_Max_Power");
            txSheet.Write(2, 1, $"Freq [MHz]");
            txSheet.Write(2, 2, $"Power [dBm]");

            uint reg0xDC34_001C = ReadRegister(0xDC34_001C);    // AFE_REG 0x07
            uint reg0xDC34_0020 = ReadRegister(0xDC34_0020);    // AFE_REG 0x28
            uint reg0xDC34_0030 = ReadRegister(0xDC34_0030);    // AFE_REG 0x0C
            uint reg0xDC34_0038 = ReadRegister(0xDC34_0038);    // AFE_REG 0x0E
            uint reg0xDC34_00A4 = ReadRegister(0xDC34_00A4);    // AFE_REG 0x29
            uint reg0xDC34_00B4 = ReadRegister(0xDC34_00B4);    // AFE_REG 0x2D
            uint reg0xDC34_00C4 = ReadRegister(0xDC34_00C4);    // AFE_REG 0x31
            uint reg0xDC34_00D0 = ReadRegister(0xDC34_00D0);    // AFE_REG 0x34

            //w_EXT_CAL[7:0].Value = 0x42;
            //w_MF_TEST_MODE[1:0].Value = 0x1;
            //w_DSM_PEN.Value = 0x1;
            //w_CT_VCOLEV.Value = 0x1;
            WriteRegister(0xDC34001C, (reg0xDC34_001C & 0x00EC) | 0x4213);

            //w_PM_EXT_MODE.Value = 0x1;
            //w_REF_DIV_SEL[2:0].Value = 0x3;
            //w_EXT_CAPS[9:0].Value = 0x100;
            WriteRegister(0xDC340020, (reg0xDC34_0020 & 0x6000) | 0x8D00);

            //w_CT_RESET.Value = 0x1;
            //w_PLL_PEN.Value = 0x1;
            //w_PM_TIME.Value = 0x2;
            //w_PLL_2PM_CAL_HOLD.Value = 0x1;
            //w_PM_GAIN_IN[9:0].Value = 0x100;
            WriteRegister(0xDC340030, 0xD500);

            //w_SPI_CH_SEL_MODE.Value = 0x1;
            //w_PLL_DIV2_OUT_PEN.Value = 0x1;
            //w_PM_CAL_GAIN[9:0].Value = 0x100;
            WriteRegister(0xDC340038, 0x8900);

            //O_SPI_RSV_CORE[7:0].Value = 0x01;
            //xo_cl_n[5:0].Value = 0x20;
            WriteRegister(0xDC34_00A4, (reg0xDC34_00A4 & 0x00C0) | 0x0120);

            //w_BUF_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_PRE_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_DA_PEN_SRC_SEL[1:0].Value = 0x2;
            //w_TRX_SEL_SRC_SEL[1:0].Value = 0x2;
            //r_TX_DA_PEN.Value = 0x1;
            //r_TX_PRE_PEN.Value = 0x1;
            //r_TX_BUF_PEN.Value = 0x1;
            //w_TX_DA_PEN_MODE.Value = 0x1;
            //w_TX_PRE_PEN_MODE.Value = 0x1;
            WriteRegister(0xDC34_00B4, (reg0xDC34_00B4 & 0xE000) | 0x155F);

            //w_TX_DA_EN.Value = 0x1;
            //w_TX_PRE_EN.Value = 0x1;
            //w_TX_BUF_EN.Value = 0x1;
            //w_TX_EN.Value = 0x1;
            //w_PLL_DIV2_OUT_EN.Value = 0x1;
            //w_PLL_OUT_EN.Value = 0x1;
            //w_PLL_EN.Value = 0x1;
            WriteRegister(0xDC34_00C4, (reg0xDC34_00C4 & 0xE1F1) | 0x1E0E);

            //w_PLL_CT_CNT_MODE.Value = 0x1;
            WriteRegister(0xDC34_00D0, (reg0xDC34_00D0 & 0xFFFE) | 0x0001);

            // 2402MHz
            WriteRegister(0xDC34_003C, 2);
            await TogglePllPen();

            SpectrumAnalyzer.Write("FREQ:CENT 2402 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            SpectrumAnalyzer.Write("DISP:TRAC:Y:RLEV 10dBm");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            double freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            double TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 3, $"2402MHz");
            txSheet.Write(2, 3, $"{freqMHz}");
            txSheet.Write(2, 4, $"{TXpower}");

            SpectrumAnalyzer.Write("FREQ:CENT 4804 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 4, $"4804MHz");
            txSheet.Write(2, 4, $"{freqMHz}");
            txSheet.Write(2, 5, $"{TXpower}");

            SpectrumAnalyzer.Write("FREQ:CENT 7206 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 5, $"7206MHz");
            txSheet.Write(2, 5, $"{freqMHz}");
            txSheet.Write(2, 6, $"{TXpower}");

            // 2440MHz
            WriteRegister(0xDC34_003C, 40);
            await TogglePllPen();

            SpectrumAnalyzer.Write("FREQ:CENT 2440 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 6, $"2440MHz");
            txSheet.Write(2, 6, $"{freqMHz}");
            txSheet.Write(3, 6, $"{TXpower}");

            SpectrumAnalyzer.Write("FREQ:CENT 4880 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 7, $"4880MHz");
            txSheet.Write(2, 7, $"{freqMHz}");
            txSheet.Write(3, 7, $"{TXpower}");

            SpectrumAnalyzer.Write("FREQ:CENT 7320 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 8, $"7320MHz");
            txSheet.Write(2, 8, $"{freqMHz}");
            txSheet.Write(3, 8, $"{TXpower}");

            // 2480MHz
            WriteRegister(0xDC34_003C, 80);
            await TogglePllPen();

            SpectrumAnalyzer.Write("FREQ:CENT 2480 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 9, $"2480MHz");
            txSheet.Write(2, 9, $"{freqMHz}");
            txSheet.Write(3, 9, $"{TXpower}");

            SpectrumAnalyzer.Write("FREQ:CENT 4960 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 10, $"4960MHz");
            txSheet.Write(2, 10, $"{freqMHz}");
            txSheet.Write(3, 10, $"{TXpower}");

            await Task.Delay(1000);
            SpectrumAnalyzer.Write("FREQ:CENT 7440 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 11, $"7206MHz");
            txSheet.Write(2, 11, $"{freqMHz}");
            txSheet.Write(3, 11, $"{TXpower}");

            // DA_LDO = 0, TX_DA_GC = 0
            uint reg0xDC34_00B0 = ReadRegister(0xDC34_00B0);    // AFE_REG 0x2C
            uint reg0xDC34_00C0 = ReadRegister(0xDC34_00C0);    // AFE_REG 0x30

            //TX_DA_GC.Value = 0;
            WriteRegister(0xDC34_00B0, reg0xDC34_00B0 & 0x87FF);
            //DA_LDO_CONT.Value = 0;
            WriteRegister(0xDC34_00C0, reg0xDC34_00C0 & 0xFF03);

            txSheet.Write(1, 13, $"TX_Max_Power");
            txSheet.Write(1, 14, $"Freq [MHz]");
            txSheet.Write(2, 14, $"Power [dBm]");

            SpectrumAnalyzer.Write("FREQ:CENT 2480 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 15, $"2480MHz");
            txSheet.Write(2, 15, $"{freqMHz}");
            txSheet.Write(3, 15, $"{TXpower}");

            // 2440MHz
            WriteRegister(0xDC34_003C, 40);
            await TogglePllPen();
            await Task.Delay(1000);

            SpectrumAnalyzer.Write("FREQ:CENT 2440 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 16, $"2440MHz");
            txSheet.Write(2, 16, $"{freqMHz}");
            txSheet.Write(3, 16, $"{TXpower}");

            // 2402MHz
            WriteRegister(0xDC34_003C, 2);
            await TogglePllPen();
            await Task.Delay(1000);

            SpectrumAnalyzer.Write("FREQ:CENT 2402 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            await Task.Delay(1000);
            SpectrumAnalyzer.Write("CALC:MARK:MAX");
            await Task.Delay(1000);
            freqMHz = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:X?")) / 1_000_000, 3);
            TXpower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
            txSheet.Write(1, 17, $"2402MHz");
            txSheet.Write(2, 17, $"{freqMHz}");
            txSheet.Write(3, 17, $"{TXpower}");
        }

        private async Task AutoMeasureRxRfGain(CancellationToken ct, RunTestContext ctx)
        {
            ct.ThrowIfCancellationRequested();

            if (_regCont == null)
                throw new InvalidOperationException("RegisterControlForm is null.");

            if (SpectrumAnalyzer == null)
                throw new InvalidOperationException("SpectrumAnalyzer is not connected.");

            if (SignalGenerator0 == null)
                throw new InvalidOperationException("SpectrumAnalyzer is not connected.");

            var PLL_PEN = _regCont.RegMgr.GetRegisterItem(this, "w_PLL_PEN");            //0x0C
            var DRV_TPE = _regCont.RegMgr.GetRegisterItem(this, "O_ABB_DRV_TPE[3:0]");   //0x14
            var SPI_CH_SEL = _regCont.RegMgr.GetRegisterItem(this, "m_SPI_CH_SEL[6:0]"); //0x0F
            var RX_RF_GC = _regCont.RegMgr.GetRegisterItem(this, "r_RX_RF_GC[1:0]");     //0x2C

            WriteRegister(0xDC34001C, 0x4213);  //REG 0x07
            WriteRegister(0xDC340020, 0x8C00);  //REG 0x08
            WriteRegister(0xDC340030, 0xD500);  //REG 0x0C
            WriteRegister(0xDC340038, 0x9500);  //REG 0x0E
            WriteRegister(0xDC34003C, 0x0002);  //REG 0x0F
            WriteRegister(0xDC340040, 0x4428);  //REG 0x10
            WriteRegister(0xDC340044, 0x001C);  //REG 0x11
            WriteRegister(0xDC340048, 0x3EDB);  //REG 0x12
            WriteRegister(0xDC34004C, 0x380C);  //REG 0x13
            WriteRegister(0xDC340050, 0xFA49);  //REG 0x14
            WriteRegister(0xDC34005C, 0xEFCF);  //REG 0x17
            WriteRegister(0xDC340060, 0x06EF);  //REG 0x18
            WriteRegister(0xDC3400AC, 0x5B7F);  //REG 0x2B
            WriteRegister(0xDC3400B0, 0x7FBD);  //REG 0x2C
            WriteRegister(0xDC3400C4, 0x00FE);  //REG 0x31
            await Task.Delay(500);
            //위에 writeregister에서 되어있지만, 한번 더 세팅함

            IReportSheet rxSheet;

            string time = DateTime.Now.ToString("HHmmss");
            rxSheet = ctx.Report.CreateSheet($"{time}_RxGain");
            rxSheet.SetSheetFont("Consolas", 10);

            rxSheet.Write(1, 1, $"TX_Max_Power");
            rxSheet.Write(2, 1, $"Freq [MHz]");
            rxSheet.Write(2, 2, $"Power [dBm]");

            rxSheet.Write(1, 3, $"Freq [MHz]");
            rxSheet.Write(1, 4, $"TPE_TYPE");
            rxSheet.Write(1, 5, $"BW [Hz]");
            rxSheet.Write(1, 6, $"Nin [dBm]");
            rxSheet.Write(1, 7, $"Nout [dBm]");
            rxSheet.Write(1, 8, $"Pin [dBm]");
            rxSheet.Write(1, 9, $"Pout [dBm]");
            rxSheet.Write(1, 10, $"Gain [dB]");
            rxSheet.Write(1, 11, $"NF [dB]");
            for (int i = 0; i < 8; i++)
            {
                rxSheet.Write(2 + i, 5, $"1000000");
                rxSheet.Write(2 + i, 6, $"-114");
                rxSheet.Write(2 + i, 8, $"-80");

            }
            rxSheet.Write(2, 4, $"2402 MHz");
            rxSheet.Write(2, 10, $"= -B8 + B9 - 6");
            rxSheet.Write(2, 11, $"= B8 - B6 - (B9 - B7)");

            rxSheet.Write(2, 12, $"* TIA_GC = 6, FLT_GC = 3, PGA_GC = 23, RX_RF_GC = 3");

            await Task.Delay(500);
            /*
            TPE.Value = 9; //TIA OUT
            TPE.Write();
            SPI_CH_SEL.Value = 2;
            SPI_CH_SEL.Write();
            PLL_PEN.Value = 0;
            PLL_PEN.Write();
            PLL_PEN.Value = 1;
            PLL_PEN.Write();
            await Task.Delay(500);
            */
            SignalGenerator0.Write("OUTP ON");                  //ESG RF off //SignalGenerator0으로 설정 필요.
            SignalGenerator0.Write("OUTP:MOD OFF");             //ESG RF off //SignalGenerator0으로 설정 필요.
            SignalGenerator0.Write("POW:LEV -80DBM");           //ESG RF off //SignalGenerator0으로 설정 필요.
            SignalGenerator0.Write("FREQ 2402 MHz");            //ESG RF off //SignalGenerator0으로 설정 필요.
            SpectrumAnalyzer.Write("DISP:TRAC:Y:RLEV 20dBm");
            SpectrumAnalyzer.Write("INP:COUP DC");
            SpectrumAnalyzer.Write("FREQ:CENT 5 MHz");
            SpectrumAnalyzer.Write("FREQ:SPAN 10 MHz");
            SpectrumAnalyzer.Write("DISP:TRAC1:MODE AVER");

            uint[] ch_sel = { 2, 26, 40, 80 };
            for (int i = 0; i < ch_sel.Length; i++)
            {

                SignalGenerator0.Write("POW:LEV -80DBM");       //ESG RF off //SignalGenerator0으로 설정 필요.

                rxSheet.Write(2 + i * 2, 3, $"{2400 + ch_sel[i]}");
                DRV_TPE.Read();
                DRV_TPE.Value = 9;
                DRV_TPE.Write();
                SignalGenerator0.Write($"FREQ {2400 + ch_sel[i]}E6");

                rxSheet.Write(2 + i * 2, 4, $"TIA output");
                rxSheet.Write(3 + i * 2, 4, $"PGA output");

                SPI_CH_SEL.Read();
                SPI_CH_SEL.Value = ch_sel[i];
                SPI_CH_SEL.Write();
                await Task.Delay(1000);
                SpectrumAnalyzer.Write("CALC:MARK:MAX");
                await Task.Delay(1000);
                //Pout 측정
                double TonePower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
                rxSheet.Write(2 + i * 2, 9, $"{TonePower}");

                await Task.Delay(1000);
                SignalGenerator0.Write("OUTP OFF"); //ESG RF off //SignalGenerator0으로 설정 필요.
                SpectrumAnalyzer.Write("CALC1:MARK1:FUNC:BPOW:SPAN 1000000");
                await Task.Delay(1000);
                TonePower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK:FUNC:BPOW:RES?")), 3);
                rxSheet.Write(2 + i * 2, 7, $"{TonePower}");
                //Nout 측정

                await Task.Delay(1000);
                DRV_TPE.Read();
                DRV_TPE.Value = 0;
                DRV_TPE.Write();

                await Task.Delay(1000);
                TonePower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK:FUNC:BPOW:RES?")), 3);
                rxSheet.Write(3 + i * 2, 7, $"{TonePower}");
                await Task.Delay(1000);

                SignalGenerator0.Write("OUTP ON"); //ESG RF off //SignalGenerator0으로 설정 필요.
                await Task.Delay(1000);
                SpectrumAnalyzer.Write("CALC:MARK:MAX");
                TonePower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
                rxSheet.Write(3 + i * 2, 9, $"{TonePower}");
            }

            rxSheet.Write(1, 14, $"RX_RF_GC_Control");
            rxSheet.Write(1, 15, $"Freq [MHz]");
            rxSheet.Write(1, 16, $"GC_value");
            rxSheet.Write(1, 17, $"RX_input_power [dBm]");
            rxSheet.Write(1, 18, $"Pout [dBm]");
            rxSheet.Write(1, 19, $"Gain [dB]");

            //RX_RF_GC_control.
            for (int GC = 0; GC < 4; GC++)
            {
                uint[] ch_sell = { 2, 26, 40, 80 };
                for (int i = 0; i < ch_sell.Length; i++)
                {
                    DRV_TPE.Read();
                    DRV_TPE.Value = 9;
                    DRV_TPE.Write();
                    await Task.Delay(1000);

                    SignalGenerator0.Write("POW:LEV -70DBM"); //ESG RF off //SignalGenerator0으로 설정 필요.
                    RX_RF_GC.Read();
                    //RX_RF_GC.Value = 다음주!! 수정
                    RX_RF_GC.Write();
                    SignalGenerator0.Write($"FREQ {2400 + ch_sell[i]}E6");
                    rxSheet.Write(2 + i, 15, $"{2400 + ch_sell[i]}");
                    SPI_CH_SEL.Read();
                    SPI_CH_SEL.Value = ch_sell[i];
                    SPI_CH_SEL.Write();
                    await Task.Delay(1000);
                    SpectrumAnalyzer.Write("CALC:MARK:MAX");
                    await Task.Delay(1000);
                    //Pout 측정
                    double TonePower = Math.Round(double.Parse(SpectrumAnalyzer.Query($"CALC:MARK1:Y?")), 3);
                    rxSheet.Write(2 + i, 18 + GC * 4, $"{TonePower}");
                }
            }
        }
        #endregion AUTO TEST ITEMS

        #region FIRMWARE TEST ITEMS
        private enum FLASH_CMD : byte
        {
            WRSR = 0x01,
            PP = 0x02,
            RDCMD = 0x03,
            WRDI = 0x04,
            RDSR = 0x05,
            WREN = 0x06,
            F_RD = 0x0B,
            SE = 0x20,
            BE32 = 0x52,
            RSTEN = 0x66,
            REMS = 0x90,
            RST = 0x99,
            RDID = 0x9F,
            RES = 0xAB,
            ENSO = 0xB1,
            DP = 0xB9,
            EXSO = 0xC1,
            CE = 0xC7,
            BE64 = 0xD8,
        }

        public void SetFirmwareFilePath(string path) => _firmwareFilePath = path;

        public void SetFlashSize(uint size) => _flashSizeBytes = size;

        private byte[] ReadFlashBuffer(uint flashAddress, int bufferLen)
        {
            byte[] buffer = new byte[bufferLen];

            for (int i = 0; i < bufferLen; i += 4)
            {
                uint data = ReadRegister(flashAddress + (uint)i);
                buffer[i + 0] = (byte)(data & 0xFF);
                buffer[i + 1] = (byte)((data >> 8) & 0xFF);
                buffer[i + 2] = (byte)((data >> 16) & 0xFF);
                buffer[i + 3] = (byte)((data >> 24) & 0xFF);
                Thread.Sleep(3);
            }

            return buffer;
        }

        private async Task<bool> FirmwareCheckI2CId(Func<string, string, Task> log, CancellationToken ct)
        {
            uint id = ReadRegister(RegI2cId);
            uint ipId = id >> 12;

            if (ipId != 0x02021)
            {
                await log("ERROR", $"Fail to Check I2C IP ID. R = 0x{ipId:X5}");
                return false;
            }

            await log("INFO", "CheckI2C_ID OK.");
            return true;
        }

        private async Task<bool> FirmwareWaitFlashWrite(Func<string, string, Task> log, CancellationToken ct, int maxLoopCount = 20, int delayMs = 200)
        {
            for (int cnt = 0; cnt < maxLoopCount; cnt++)
            {
                ct.ThrowIfCancellationRequested();

                uint status;
                try
                {
                    status = ReadRegister(RegFlashStatus);
                }
                catch (Exception ex)
                {
                    await log("ERROR", $"WaitFlashReady: ReadRegister(0x50090020) failed: {ex.Message}");
                    return false;
                }
                uint busy = status & 0x01u;

                if (busy == 0)
                    return true;

                await Task.Delay(delayMs, ct);
            }

            await log("ERROR", "Flash controller did not become ready within timeout.");
            return false;
        }

        private async Task<bool> FirmwareWriteMemoryNvm(uint flashAddress, byte[] pageBuffer, Func<string, string, Task> log, CancellationToken ct)
        {
            const byte FlashCmdPageProgram = (byte)FLASH_CMD.PP;

            for (int i = 0; i < pageBuffer.Length; i += 4)
            {
                ct.ThrowIfCancellationRequested();

                uint word = (uint)(pageBuffer[i]
                    | (pageBuffer[Math.Min(i + 1, pageBuffer.Length - 1)] << 8)
                    | (pageBuffer[Math.Min(i + 2, pageBuffer.Length - 1)] << 16)
                    | (pageBuffer[Math.Min(i + 3, pageBuffer.Length - 1)] << 24));

                WriteRegister(RegFlashTxBase + (uint)i, word);
            }

            WriteRegister(RegFlashCmd, (FlashCmdPageProgram << 24) | (flashAddress & 0xFFFFFFu));

            for (int retry = 0; retry < 2000; retry++)
            {
                ct.ThrowIfCancellationRequested();

                uint status = ReadRegister(RegFlashStatusAlt);
                if ((status & 0x1u) == 0)
                    return true;

                await Task.Delay(1, ct);
            }

            await log("ERROR", "Timeout waiting for flash page program to complete.");
            return false;
        }

        [ChipTest("FW", "FLASH_ERASE", "Flash Erase")]
        private async Task FirmwareFlashErase(Func<string, string, Task> log, CancellationToken ct)
        {
            try
            {
                if (!await FirmwareCheckI2CId(log, ct))
                    return;

                HaltMcu();

                await log("INFO", "Start FLASH_ERASE (8 sectors of 64KB).");

                for (uint num = 0; num < 8; num++)
                {
                    ct.ThrowIfCancellationRequested();

                    uint secAddr = num * 0x10000u;
                    await log("INFO", $"Erase sector #{num} @ 0x{secAddr:X8}");

                    uint cmd = ((uint)FLASH_CMD.BE64 << 24) | (secAddr & 0x00FFFFFFu);
                    WriteRegister(RegFlashCmd, cmd);

                    bool ok = await FirmwareWaitFlashWrite(log, ct);
                    if (!ok)
                    {
                        await log("ERROR", $"Sector erase timeout @ 0x{secAddr:X8}");
                        return;
                    }
                }

                await log("INFO", "FLASH_ERASE completed successfully.");
            }
            finally
            {
                ResetMcu();
            }
        }

        [ChipTest("FW", "FLASH_WRITE", "Flash Write")]
        private async Task FirmwareFlashWrite(Func<string, string, Task> log, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_firmwareFilePath))
                {
                    await log("ERROR", "Firmware file path is not set.");
                    return;
                }

                byte[] fwData;
                try
                {
                    fwData = File.ReadAllBytes(_firmwareFilePath);
                }
                catch (Exception ex)
                {
                    await log("ERROR", $"Failed to read firmware file: {ex.Message}");
                    return;
                }

                if (fwData.Length == 0)
                {
                    await log("ERROR", "Firmware file is empty.");
                    return;
                }

                if (!await FirmwareCheckI2CId(log, ct))
                    return;

                await log("INFO", "FLASH_WRITE: Start FLASH_ERASE before FLASH_WRITE.");
                await FirmwareFlashErase(log, ct);

                if (!await FirmwareCheckI2CId(log, ct))
                {
                    await log("ERROR", "FLASH_WRITE: After erase, I2C ID check failed. Abort write.");
                    return;
                }

                HaltMcu();

                const int PageSize = 256;
                byte[] pageBuffer = new byte[PageSize];
                byte[] readBuffer = new byte[PageSize];

                for (uint flashAddress = 0; flashAddress < fwData.Length; flashAddress += PageSize)
                {
                    ct.ThrowIfCancellationRequested();

                    for (int i = 0; i < PageSize; i++)
                    {
                        int srcIndex = (int)flashAddress + i;
                        pageBuffer[i] = srcIndex < fwData.Length ? fwData[srcIndex] : (byte)0xFF;
                    }

                    if (flashAddress % 0x1000 == 0)
                        await log("INFO", $"Write page @ 0x{flashAddress:X8}");

                    if (!await FirmwareWriteMemoryNvm(flashAddress, pageBuffer, log, ct))
                        return;

                    await Task.Delay(50);

                    for (uint i = 0; i < PageSize; i += 4)
                    {
                        uint data = ReadRegister(flashAddress + i);
                        for (int j = 0; j < 4; j++)
                        {
                            int idx = (int)i + j;
                            if (idx < PageSize)
                                readBuffer[idx] = (byte)((data >> (8 * j)) & 0xFF);
                        }
                    }

                    for (int i = 0; i < PageSize; i++)
                    {
                        byte expected = ((int)flashAddress + i < fwData.Length) ? fwData[(int)flashAddress + i] : (byte)0xFF;
                        if (readBuffer[i] != expected)
                        {
                            await log("ERROR", $"Verify failed @ 0x{flashAddress + (uint)i:X8}: W=0x{expected:X2}, R=0x{readBuffer[i]:X2}");
                            return;
                        }
                    }
                }

                await log("INFO", "FLASH_WRITE completed successfully.");
            }
            finally
            {
                ResetMcu();
            }
        }

        [ChipTest("FW", "FLASH_READ", "Flash Read")]
        private async Task FirmwareFlashRead(Func<string, string, Task> log, CancellationToken ct)
        {
            try
            {
                await log("INFO", "Start FLASH_READ (Dump NV memory).");

                if (!await FirmwareCheckI2CId(log, ct))
                    return;

                HaltMcu();

                uint dumpSize = _flashSizeBytes;
                if (dumpSize <= 0)
                {
                    if (!string.IsNullOrWhiteSpace(_firmwareFilePath) && File.Exists(_firmwareFilePath))
                    {
                        dumpSize = (uint)new FileInfo(_firmwareFilePath).Length;
                    }
                    else
                    {
                        dumpSize = 256 * 1024;
                        await log("INFO", $"Dump size is not set. Use default {dumpSize} bytes (256KB).");
                    }
                }

                const int PageSize = 4;
                var firmwareData = new List<byte>((int)dumpSize);

                for (uint addr = 0; addr < dumpSize; addr += PageSize)
                {
                    ct.ThrowIfCancellationRequested();

                    if ((addr % 0x1000) == 0)
                        await log("INFO", $"Read Flash @ 0x{addr:X8}");

                    uint rcv = ReadRegister(addr);

                    firmwareData.Add((byte)(rcv & 0xFF));
                    firmwareData.Add((byte)((rcv >> 8) & 0xFF));
                    firmwareData.Add((byte)((rcv >> 16) & 0xFF));
                    firmwareData.Add((byte)((rcv >> 24) & 0xFF));
                }

                string time = DateTime.Now.ToString("HHmmss");
                string fileName = $"ReadFlash_{time}.bin";
                try
                {
                    File.WriteAllBytes(fileName, firmwareData.ToArray());
                }
                catch (Exception ex)
                {
                    await log("ERROR", $"Failed to write dump file '{fileName}': {ex.Message}");
                    return;
                }

                await log("INFO", $"FLASH_READ completed. File = {fileName}, Size = {firmwareData.Count} bytes.");
            }
            finally
            {
                ResetMcu();
            }
        }

        [ChipTest("FW", "FLASH_VERIFY", "Flash Verify Sequence")]
        private async Task FirmwareFlashVerify(Func<string, string, Task> log, CancellationToken ct)
        {
            uint flashSize = _flashSizeBytes;
            if (flashSize == 0)
            {
                await log("ERROR", "Flash verify size is not set. (SetFlashSize required)");
                return;
            }

            if (!await FirmwareCheckI2CId(log, ct))
                return;

            try
            {
                const int PageSize = 256;
                byte[] pageBuffer = new byte[PageSize];
                byte[] readBuffer = new byte[PageSize];
                byte[] patterns = new byte[] { 0xAA, 0x55 };

                await log("INFO", $"Start FW.FLASH_VERIFY. Size={flashSize} bytes");

                for (int p = 0; p < patterns.Length; p++)
                {
                    ct.ThrowIfCancellationRequested();

                    byte pattern = patterns[p];
                    await log("INFO", $"Pattern {p + 1}/{patterns.Length}: 0x{pattern:X2}");

                    await log("INFO", "Erase NV memory...");
                    await FirmwareFlashErase(log, ct);
                    HaltMcu();

                    await log("INFO", "Erase verify...");
                    for (uint addr = 0; addr < flashSize; addr += PageSize)
                    {
                        ct.ThrowIfCancellationRequested();

                        if ((addr % 0x1000) == 0)
                            await log("INFO", $"Erase verify @ 0x{addr:X8}");

                        int len = (int)Math.Min(PageSize, flashSize - addr);
                        readBuffer = new byte[len];
                        readBuffer = ReadFlashBuffer(addr, len);

                        for (int i = 0; i < len; i++)
                        {
                            if (readBuffer[i] != 0xFF)
                            {
                                await log("ERROR", $"Fail to Erase: Addr=0x{(addr + (uint)i):X8}, Read=0x{readBuffer[i]:X2}");
                                return;
                            }
                        }
                    }
                    await log("INFO", "Erase verify OK.");

                    await log("INFO", "Write pattern...");
                    for (uint addr = 0; addr < flashSize; addr += PageSize)
                    {
                        ct.ThrowIfCancellationRequested();

                        if ((addr % 0x1000) == 0)
                            await log("INFO", $"Write pattern @ 0x{addr:X8}");

                        int len = (int)Math.Min(PageSize, flashSize - addr);

                        for (int i = 0; i < len; i++)
                            pageBuffer[i] = pattern;

                        if (!await FirmwareWriteMemoryNvm(addr, pageBuffer, log, ct))
                        {
                            await log("ERROR", $"Fail to Write: Addr=0x{addr:X8}");
                            return;
                        }

                        await Task.Delay(1);

                        readBuffer = new byte[len];
                        readBuffer = ReadFlashBuffer(addr, len);

                        for (int i = 0; i < len; i++)
                        {
                            if (readBuffer[i] != pattern)
                            {
                                await log("ERROR", $"Fail to Verify: Addr=0x{(addr + (uint)i):X8}, W=0x{pattern:X2}, R=0x{readBuffer[i]:X2}");
                                return;
                            }
                        }
                    }
                    await log("INFO", "Write OK.");
                }

                await log("INFO", "FW.FLASH_VERIFY completed successfully.");
            }
            finally
            {
                ResetMcu();
            }
        }

        [ChipTest("FW", "FIRM_ON_CLEAR", "Firm On Clear")]
        private async Task FirmwareOnClear(Func<string, string, Task> log, CancellationToken ct)
        {
            await log("INFO", "FIRM_ON_CLEAR is not implemented in V4 yet.");
            await Task.Delay(100, ct);
        }

        [ChipTest("FW", "RAM_WRITE", "Ram Write")]
        private async Task FirmwareRamWrite(Func<string, string, Task> log, CancellationToken ct)
        {
            await log("INFO", "RAM_WRITE is not implemented in V4 yet.");
            await Task.Delay(100, ct);
        }

        [ChipTest("FW", "RAM_READ", "Ram Read")]
        private async Task FirmwareRamRead(Func<string, string, Task> log, CancellationToken ct)
        {
            await log("INFO", "RAM_READ is not implemented in V4 yet.");
            await Task.Delay(100, ct);
        }
        #endregion FIRMWARE TEST ITEMS
    }
}