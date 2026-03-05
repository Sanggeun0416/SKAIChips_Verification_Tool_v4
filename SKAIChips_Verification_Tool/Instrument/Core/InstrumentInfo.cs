using System;

namespace SKAIChips_Verification_Tool.Instrument
{
    /// <summary>
    /// SCPI(Standard Commands for Programmable Instruments) 프로토콜을 사용하는 
    /// 계측기와의 통신을 위한 표준 인터페이스입니다.
    /// VISA(Virtual Instrument Software Architecture) 라이브러리를 통한 TCP/IP, USB, GPIB 연결을 추상화합니다.
    /// </summary>
    public interface IScpiClient : IDisposable
    {
        /// <summary>
        /// 설정된 리소스 주소를 사용하여 계측기와의 통신 세션을 엽니다.
        /// </summary>
        /// <returns>연결에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        bool Open();

        /// <summary>
        /// 현재 활성화된 계측기 통신 세션을 닫고 리소스를 해제합니다.
        /// </summary>
        void Close();

        /// <summary>
        /// 계측기에 SCPI 명령어를 전송합니다. (응답을 기다리지 않는 단방향 명령)
        /// 예: "CONF:VOLT:DC 10", "OUTP ON"
        /// </summary>
        /// <param name="command">전송할 SCPI 명령어 문자열</param>
        void Write(string command);

        /// <summary>
        /// 계측기에 질의(Query) 명령어를 전송하고 문자열 응답을 읽어옵니다.
        /// 예: "*IDN?", "MEAS:VOLT:DC?"
        /// </summary>
        /// <param name="command">질의할 SCPI 명령어 문자열</param>
        /// <param name="timeoutMs">응답을 기다릴 최대 시간(밀리초)입니다. (기본값: 1000ms)</param>
        /// <returns>계측기에서 반환한 응답 문자열</returns>
        string Query(string command, int timeoutMs = 1000);

        /// <summary>
        /// 계측기에 질의 명령어를 전송하고 결과 데이터를 바이트 배열로 읽어옵니다.
        /// 주로 오실로스코프의 파형 데이터나 대량의 바이너리 데이터를 수신할 때 사용됩니다.
        /// </summary>
        /// <param name="command">질의할 SCPI 명령어 문자열</param>
        /// <param name="timeoutMs">데이터 수신을 기다릴 최대 시간(밀리초)입니다. 대용량 데이터 처리를 위해 기본값이 깁니다. (기본값: 30000ms)</param>
        /// <returns>수신된 원시(Raw) 바이트 데이터 배열</returns>
        byte[] QueryBytes(string command, int timeoutMs = 30000);
    }
}