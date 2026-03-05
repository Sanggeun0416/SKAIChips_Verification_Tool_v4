using Ivi.Visa;
using Keysight.Visa;

namespace SKAIChips_Verification_Tool.Instrument
{
    public class VisaScpiClient : IScpiClient
    {
        readonly string _visaAddress;
        IMessageBasedSession _session;

        public VisaScpiClient(string visaAddress)
        {
            _visaAddress = visaAddress ?? throw new ArgumentNullException(nameof(visaAddress));
        }

        public bool Open()
        {
            if (_session != null)
                return true;

            try
            {
                var rm = new ResourceManager();
                _session = rm.Open(_visaAddress) as IMessageBasedSession;
                return _session != null;
            }
            catch
            {
                _session = null;
                return false;
            }
        }

        public void Close()
        {
            if (_session == null)
                return;

            try
            {
                _session.Dispose();
            }
            finally { _session = null; }
        }

        public void Write(string command)
        {
            if (_session == null)
                throw new InvalidOperationException("Session not open");
            _session.RawIO.Write(command + "\n");
        }

        public string Query(string command, int timeoutMs = 1000)
        {
            if (_session == null)
                throw new InvalidOperationException("Session not open");
            _session.TimeoutMilliseconds = timeoutMs;
            _session.RawIO.Write(command + "\n");
            return _session.RawIO.ReadString();
        }

        public byte[] QueryBytes(string command, int timeoutMs = 30000)
        {
            if (_session == null)
                throw new InvalidOperationException("Session not open");
            _session.TimeoutMilliseconds = timeoutMs;
            _session.RawIO.Write(command + "\n");
            return _session.RawIO.Read();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
