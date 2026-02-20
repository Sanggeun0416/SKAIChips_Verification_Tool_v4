namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Aladdin : ProjectBase
    {
        public override string Name => "Aladdin";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Aladdin",
            "AL8700"
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.I2C };

        public override uint ComFrequency => 100;
        public override byte DeviceAddress => 0x74;

        public Aladdin()
        {
        }

        public Aladdin(II2cBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return null;
            //return new[]
            //{
            //    new TestSlotAction("Aladdin Init", () => MessageBox.Show("Running Aladdin Initialization..."))
            //};
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            byte[] Bytes;

            if (address >= 0x10 && address <= 0x3F)
            {
                Bytes = new byte[2];
                Bytes[0] = (byte)(((address & 0xF0) | (data & 0x0F)) & 0xFF);
                Bytes[1] = Bytes[0];
            }
            else if (address >= 0x50 && address <= 0x5F)
            {
                Bytes = new byte[4];
                Bytes[0] = (byte)(address & 0xFF);
                Bytes[1] = (byte)((data >> 8) & 0xFF);
                Bytes[2] = (byte)(data & 0xFF);
                Bytes[3] = CalculateParity(Bytes, 3);
            }
            else if (address >= 0x60)
            {
                Bytes = new byte[3];
                Bytes[0] = (byte)(address & 0xFF);
                Bytes[1] = (byte)(data & 0xFF);
                Bytes[2] = CalculateParity(Bytes, 2);
            }
            else
            {
                return;
            }

            I2cBus.Write(DeviceAddress, Bytes, stop: true);
        }

        public override uint ReadRegister(uint address)
        {
            if (I2cBus == null)
                throw new InvalidOperationException("I2C Bus is not connected.");

            byte[] Addr = new byte[] { (byte)address };
            byte[] Bytes = new byte[5];
            uint Data = 0;

            I2cBus.Write(DeviceAddress, Addr, stop: false);

            switch (address)
            {
                case 0x00:
                    I2cBus.Read(DeviceAddress, Bytes, 5);

                    if (Bytes.Length >= 4)
                        Data = (uint)((Bytes[3] << 24) | (Bytes[2] << 16) | (Bytes[1] << 8) | Bytes[0]);
                    break;

                case >= 0x50 and <= 0x57:
                    I2cBus.Read(DeviceAddress, Bytes, 3);

                    if (Bytes.Length >= 2)
                        Data = (uint)((Bytes[1] << 8) | Bytes[0]);
                    break;

                default:
                    I2cBus.Read(DeviceAddress, Bytes, 2);

                    if (Bytes.Length >= 1)
                        Data = Bytes[0];
                    break;
            }
            return Data;
        }

        private byte CalculateParity(byte[] data, int length)
        {
            byte parity = 0x00;
            for (int i = 0; i < 8; i++)
            {
                int ones = 0;
                for (int j = 0; j < length; j++)
                {
                    if (((data[j] >> i) & 1) == 1)
                        ones++;
                }
                if (ones % 2 != 0)
                    parity |= (byte)(1 << i);
            }
            return parity;
        }
    }
}