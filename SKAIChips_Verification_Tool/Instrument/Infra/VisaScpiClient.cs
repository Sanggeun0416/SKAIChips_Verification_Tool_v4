using System;
using Ivi.Visa;
using Keysight.Visa;

namespace SKAIChips_Verification_Tool.Instrument
{
    /// <summary>
    /// Keysight VISA 라이브러리를 사용하여 IScpiClient 인터페이스를 구현한 클래스입니다.
    /// 실제 계측기와의 물리적인 메시지 기반(Message-Based) 통신 세션을 관리합니다.
    /// </summary>
    public class VisaScpiClient : IScpiClient
    {
        private readonly string _visaAddress;
        private IMessageBasedSession _session;

        /// <summary>
        /// VisaScpiClient 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="visaAddress">계측기의 VISA 리소스 주소 (예: "TCPIP0::192.168.0.1::inst0::INSTR")</param>
        public VisaScpiClient(string visaAddress)
        {
            _visaAddress = visaAddress ?? throw new ArgumentNullException(nameof(visaAddress));
        }

        /// <summary>
        /// VISA ResourceManager를 사용하여 계측기와의 통신 세션을 엽니다.
        /// </summary>
        /// <returns>세션이 성공적으로 열리면 true, 실패하면 false를 반환합니다.</returns>
        public bool Open()
        {
            if (_session != null)
                return true;

            try
            {
                // VISA 표준 리소스 매니저 생성 및 장치 연결
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

        /// <summary>
        /// 현재 열려 있는 계측기 세션을 종료하고 리소스를 해제합니다.
        /// </summary>
        public void Close()
        {
            if (_session == null)
                return;

            try
            {
                _session.Dispose();
            }
            finally
            {
                _session = null;
            }
        }

        /// <summary>
        /// 계측기에 SCPI 명령어를 전송합니다. 명령어 끝에 개행 문(\n)을 자동으로 추가합니다.
        /// </summary>
        /// <param name="command">전송할 SCPI 명령어</param>
        public void Write(string command)
        {
            if (_session == null)
                throw new InvalidOperationException("계측기 세션이 열려 있지 않습니다.");

            _session.RawIO.Write(command + "\n");
        }

        /// <summary>
        /// 계측기에 명령어를 전송한 후 문자열 응답을 읽어옵니다.
        /// </summary>
        /// <param name="command">질의할 SCPI 명령어</param>
        /// <param name="timeoutMs">응답 대기 제한 시간(ms)</param>
        /// <returns>계측기로부터 수신된 응답 문자열</returns>
        public string Query(string command, int timeoutMs = 1000)
        {
            if (_session == null)
                throw new InvalidOperationException("계측기 세션이 열려 있지 않습니다.");

            _session.TimeoutMilliseconds = timeoutMs;
            _session.RawIO.Write(command + "\n");
            return _session.RawIO.ReadString();
        }

        /// <summary>
        /// 계측기에 명령어를 전송한 후 원시 바이트(Raw Bytes) 응답을 읽어옵니다.
        /// 대용량 파형 데이터 등을 수신할 때 사용합니다.
        /// </summary>
        /// <param name="command">질의할 SCPI 명령어</param>
        /// <param name="timeoutMs">데이터 수신 대기 제한 시간(ms)</param>
        /// <returns>수신된 바이트 배열 데이터</returns>
        public byte[] QueryBytes(string command, int timeoutMs = 30000)
        {
            if (_session == null)
                throw new InvalidOperationException("계측기 세션이 열려 있지 않습니다.");

            _session.TimeoutMilliseconds = timeoutMs;
            _session.RawIO.Write(command + "\n");
            return _session.RawIO.Read();
        }

        /// <summary>
        /// IDisposable 인터페이스 구현을 통해 세션을 안전하게 닫습니다.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}