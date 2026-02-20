namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class IRIS : ProjectBase
    {
        public override string Name => "IRIS";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "IRIS",
            "SCP1501"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.I2C };

        public override uint ComFrequency => 400;
        public override byte DeviceAddress => 0x3A;

        public IRIS()
        {
        }

        public IRIS(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return new[]
            {
                new TestSlotAction("WakeUp", WakeUp),
                new TestSlotAction("NVM ON", () => PowerToggle_NVM(true)),
                new TestSlotAction("NVM OFF", () => PowerToggle_NVM(false))
            };
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            string sheetName = this.CurrentSheetName;

            if (string.IsNullOrEmpty(sheetName))
            {
                sheetName = GetSheetNameByAddress(address);
            }

            byte[] sendData;

            switch (sheetName)
            {
                case "NVM":
                    sendData = new byte[]
                    {
                        0x00,
                        0x00,
                        0x06,
                        0x00,
                        0x0D,
                        0x00,
                        (byte)(address & 0xFF),
                        (byte)((address >> 8) & 0xFF)
                    };
                    I2cBus.Write(DeviceAddress, sendData);

                    // bist_sel=1, bist_type=3, bist_cmd=1, bist_addr=address
                    sendData[4] = 0x0F;
                    sendData[5] = 0x00;
                    I2cBus.Write(DeviceAddress, sendData);

                    // bist_wdata=data
                    sendData = new byte[]
                    {
                        0x04,
                        0x00,
                        0x06,
                        0x00,
                        (byte)(data & 0xFF),
                        (byte)((data >> 8) & 0xFF),
                        (byte)((data >> 16) & 0xFF),
                        (byte)((data >> 24) & 0xFF)
                    };
                    I2cBus.Write(DeviceAddress, sendData);

                    // bist_sel=1, bist_type=3, bist_cmd=0, bist_addr=address
                    sendData = new byte[]
                    {
                        0x00,
                        0x00,
                        0x06,
                        0x00,
                        0x0D,
                        0x00,
                        (byte)(address & 0xFF),
                        (byte)((address >> 8) & 0xFF)
                    };
                    I2cBus.Write(DeviceAddress, sendData);

                    Thread.Sleep(5);

                    // Cleanup
                    sendData = new byte[]
                    {
                        0x00,
                        0x00,
                        0x06,
                        0x00,
                        0x0C,
                        0x00,
                        0x00,
                        0x00
                    };
                    I2cBus.Write(DeviceAddress, sendData);
                    break;

                case "NVM Controller":
                case "NVM BIST":
                case "I2C Slave":
                case "LPCAL":
                case "ADC Controller":
                    sendData = new byte[]
                    {
                        (byte)(address & 0xFF),
                        (byte)((address >> 8) & 0xFF),
                        (byte)((address >> 16) & 0xFF),
                        (byte)((address >> 24) & 0xFF),
                        (byte)(data & 0xFF),
                        (byte)((data >> 8) & 0xFF),
                        (byte)((data >> 16) & 0xFF),
                        (byte)((data >> 24) & 0xFF)
                    };
                    I2cBus.Write(DeviceAddress, sendData);
                    break;

                default:
                    sendData = new byte[]
                    {
                        0x08,
                        0x00,
                        0x01,
                        0x00,
                        (byte)(data & 0xFF),
                        (byte)(address & 0xFF),
                        0x07,
                        0x00
                    };
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData[6] = 0x04;
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData[4] = 0x00;
                    sendData[5] = 0x00;
                    sendData[6] = 0x00;
                    I2cBus.Write(DeviceAddress, sendData);
                    break;
            }
        }

        public override uint ReadRegister(uint address)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            string sheetName = this.CurrentSheetName;

            if (string.IsNullOrEmpty(sheetName))
            {
                sheetName = GetSheetNameByAddress(address);
            }
            byte[] sendData;
            byte[] rcvBuf = new byte[4]; // 수신 버퍼 생성
            uint result = 0xFFFFFFFF;

            switch (sheetName)
            {
                case "NVM":
                    sendData = new byte[]
                    {
                        0x00,
                        0x00,
                        0x06,
                        0x00,
                        0x11,
                        0x00,
                        0x00,
                        0x00
                    };
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData[4] = 0x13;
                    sendData[6] = (byte)(address & 0xFF);
                    sendData[7] = (byte)((address >> 8) & 0xFF);
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData[4] = 0x11;
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData = new byte[]
                    {
                        0x14,
                        0x00,
                        0x06,
                        0x00
                    };
                    I2cBus.Write(DeviceAddress, sendData, stop: false);

                    I2cBus.Read(DeviceAddress, rcvBuf, 200); // 200ms Timeout

                    result = (uint)(rcvBuf[0] | (rcvBuf[1] << 8) | (rcvBuf[2] << 16) | (rcvBuf[3] << 24));

                    // Cleanup
                    sendData = new byte[] { 0x00, 0x00, 0x06, 0x00, 0x10, 0x00, 0x00, 0x00 };
                    I2cBus.Write(DeviceAddress, sendData);
                    break;

                case "NVM Controller":
                case "NVM BIST":
                case "I2C Slave":
                case "LPCAL":
                case "ADC Controller":
                    sendData = new byte[]
                    {
                        (byte)(address & 0xFF),
                        (byte)((address >> 8) & 0xFF),
                        (byte)((address >> 16) & 0xFF),
                        (byte)((address >> 24) & 0xFF)
                    };
                    I2cBus.Write(DeviceAddress, sendData, stop: false);
                    I2cBus.Read(DeviceAddress, rcvBuf, 200);

                    result = (uint)(rcvBuf[0] | (rcvBuf[1] << 8) | (rcvBuf[2] << 16) | (rcvBuf[3] << 24));
                    break;

                default:
                    sendData = new byte[] { 0x08, 0x00, 0x01, 0x00, 0x00, (byte)(address & 0xFF), 0x04, 0x00 };
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData[6] = 0x05;
                    I2cBus.Write(DeviceAddress, sendData);

                    sendData = new byte[] { 0x10, 0x00, 0x01, 0x00 };
                    I2cBus.Write(DeviceAddress, sendData, stop: false);

                    I2cBus.Read(DeviceAddress, rcvBuf, 200);
                    result = rcvBuf[3];

                    // Cleanup
                    sendData = new byte[] { 0x08, 0x00, 0x01, 0x00, 0x00, (byte)(address & 0xFF), 0x04, 0x00 };
                    I2cBus.Write(DeviceAddress, sendData);
                    sendData[5] = 0x00;
                    sendData[6] = 0x00;
                    I2cBus.Write(DeviceAddress, sendData);
                    break;
            }

            return result;
        }

        private void WakeUp()
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            byte[] sendData = new byte[2];

            sendData[0] = 0xAA;
            sendData[1] = 0xBB;

            I2cBus.Write(DeviceAddress, sendData);
        }

        private void PowerToggle_NVM(bool enable)
        {
            if (I2cBus == null || !I2cBus.IsConnected)
                throw new InvalidOperationException("I2C Bus is not connected.");

            byte[] sendData = new byte[8];

            if (enable)
            {
                // bist_sel=1, bist_type=1, bist_cmd=1
                sendData[0] = 0x00;
                sendData[1] = 0x00;
                sendData[2] = 0x06;
                sendData[3] = 0x00;
                sendData[4] = 0x07;
                sendData[5] = 0x00;
                sendData[6] = 0x00;
                sendData[7] = 0x00;
                I2cBus.Write(DeviceAddress, sendData, true);

                // bist_sel=1, bist_type=1, bist_cmd=0
                sendData[4] = 0x05;
                I2cBus.Write(DeviceAddress, sendData, true);
            }
            else
            {
                // bist_sel=1, bist_type=1, bist_cmd=1
                sendData[0] = 0x00;
                sendData[1] = 0x00;
                sendData[2] = 0x06;
                sendData[3] = 0x00;
                sendData[4] = 0x0B;
                sendData[5] = 0x00;
                sendData[6] = 0x00;
                sendData[7] = 0x00;
                I2cBus.Write(DeviceAddress, sendData, true);

                // bist_sel=1, bist_type=1, bist_cmd=0
                sendData[4] = 0x09;
                I2cBus.Write(DeviceAddress, sendData, true);
            }
        }
    }
}