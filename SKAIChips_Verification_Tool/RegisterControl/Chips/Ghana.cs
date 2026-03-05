namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class Ghana : ProjectBase
    {
        public override string Name => "Ghana";

        public override IEnumerable<string> ProjectKeywords => new[]
        {
            "Ghana",
        };

        public override IEnumerable<ProtocolRegLogType> SupportedProtocols => new[] { ProtocolRegLogType.SPI };

        public override uint ComFrequency => 1000;

        public Ghana()
        {
        }

        public Ghana(ISpiBus bus) : base(bus) { }

        public override TestSlotAction[] GetTestSlotActions()
        {
            return null;
        }

        public override void WriteRegister(uint address, uint data)
        {
            if (SpiBus == null || !SpiBus.IsConnected)
                throw new InvalidOperationException("SPI Bus is not connected.");

            byte[] Bytes = new byte[3];
            uint rwFlag = 1;

            Bytes[0] = (byte)((rwFlag << 7) | (address & 0x7F));
            Bytes[1] = (byte)((data >> 8) & 0xff);
            Bytes[2] = (byte)((data >> 0) & 0xff);

            SpiBus.Write(Bytes, true);
        }

        public override uint ReadRegister(uint address)
        {
            if (SpiBus == null || !SpiBus.IsConnected)
                throw new InvalidOperationException("SPI Bus is not connected.");

            uint rwFlag = 0;
            uint Data = 0xFFFF;
            byte[] Bytes = new byte[1];
            byte[] Buff = new byte[2];

            Bytes[0] = (byte)((rwFlag << 7) | (address & 0x7F));

            SpiBus.Transfer(Bytes, Buff, true);

            Data = (uint)((Buff[0] << 8) | (Buff[1] << 0));

            return Data;
        }
    }
}