namespace SKAIChips_Verification_Tool.RegisterControl
{
    public interface ISpiBus : IDisposable
    {
        bool Connect();
        void Disconnect();
        bool IsConnected
        {
            get;
        }

        void Transfer(ReadOnlySpan<byte> tx, Span<byte> rx, bool endTransaction = true);
        void Write(ReadOnlySpan<byte> tx, bool endTransaction = true);
        void Read(Span<byte> rx, bool endTransaction = true);
    }

    public interface IChicagoSpiBus
    {
        void ChicagoWrite(ReadOnlySpan<byte> tx);
        void ChicagoWriteRead(ReadOnlySpan<byte> tx, Span<byte> rx);
        void ChicagoForceIdleHigh();
    }
}
