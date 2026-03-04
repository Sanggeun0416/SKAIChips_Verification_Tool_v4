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
    }
}