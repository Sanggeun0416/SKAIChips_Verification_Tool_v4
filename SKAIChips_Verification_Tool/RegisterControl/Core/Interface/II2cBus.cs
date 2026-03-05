using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// I2C 통신 버스를 추상화한 표준 인터페이스입니다.
    /// 하드웨어의 세부 구현(FTDI 등)에 종속되지 않고, 칩(Chip)이나 레지스터(Register) 계층에서
    /// 일관된 방식으로 I2C 디바이스와 통신할 수 있도록 지원합니다.
    /// </summary>
    public interface II2cBus : IDisposable
    {
        /// <summary>
        /// 설정된 하드웨어 디바이스를 열고 I2C 마스터 모드로 통신 버스를 초기화합니다.
        /// </summary>
        /// <returns>연결 및 초기화에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        bool Connect();

        /// <summary>
        /// 현재 연결된 I2C 통신 버스를 안전하게 닫고 하드웨어 리소스를 해제합니다.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 현재 I2C 버스가 정상적으로 연결되어 통신 가능한 상태인지 여부를 가져옵니다.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로 데이터를 전송합니다.
        /// </summary>
        /// <param name="slaveAddr">데이터를 수신할 I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="data">전송할 데이터가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        /// <param name="stop">전송을 마친 후 STOP 비트를 발생시켜 버스 점유를 해제할지 여부입니다. (기본값: true)</param>
        void Write(byte slaveAddr, ReadOnlySpan<byte> data, bool stop = true);

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로부터 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="slaveAddr">데이터를 전송할 I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="buffer">읽어온 데이터를 저장할 대상 바이트 버퍼입니다.</param>
        /// <param name="timeoutMs">읽기 작업의 최대 대기 시간(밀리초)입니다.</param>
        void Read(byte slaveAddr, Span<byte> buffer, int timeoutMs);

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로 데이터를 전송한 직후, 버스를 점유한 상태(Repeated Start 등)에서 즉시 데이터를 읽어옵니다.
        /// 주로 특정 레지스터 주소를 쓴 뒤 해당 레지스터의 값을 읽어올 때 사용됩니다.
        /// </summary>
        /// <param name="slaveAddr">통신할 I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="w">전송할 명령어 또는 레지스터 주소가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        /// <param name="r">읽어온 데이터를 저장할 대상 바이트 버퍼입니다.</param>
        /// <param name="timeoutMs">전체 트랜잭션의 최대 대기 시간(밀리초)입니다.</param>
        void WriteRead(byte slaveAddr, ReadOnlySpan<byte> w, Span<byte> r, int timeoutMs);
    }
}