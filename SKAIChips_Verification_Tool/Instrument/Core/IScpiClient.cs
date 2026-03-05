namespace SKAIChips_Verification_Tool.Instrument
{
    public interface IScpiClient : IDisposable
    {
        bool Open();
        void Close();
        void Write(string command);
        string Query(string command, int timeoutMs = 1000);
        byte[] QueryBytes(string command, int timeoutMs = 30000);
    }
}
