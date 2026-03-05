using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// SPI(Serial Peripheral Interface) 통신 버스를 추상화한 표준 인터페이스입니다.
    /// 하드웨어의 세부 구현(FTDI 등)에 종속되지 않고, 칩이나 레지스터 계층에서
    /// 일관된 방식으로 SPI 디바이스와 통신할 수 있도록 지원합니다.
    /// </summary>
    public interface ISpiBus : IDisposable
    {
        /// <summary>
        /// 설정된 하드웨어 디바이스를 열고 SPI 마스터 모드로 통신 버스를 초기화합니다.
        /// </summary>
        /// <returns>연결 및 초기화에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        bool Connect();

        /// <summary>
        /// 현재 연결된 SPI 통신 버스를 안전하게 닫고 하드웨어 리소스를 해제합니다.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 현재 SPI 버스가 정상적으로 연결되어 통신 가능한 상태인지 여부를 가져옵니다.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// SPI 버스의 전이중(Full-Duplex) 특성을 활용하여 데이터를 전송함과 동시에 수신합니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        /// <param name="rx">수신된 데이터를 저장할 대상 바이트 버퍼입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS(Chip Select) 핀을 비활성화(High)하여 트랜잭션을 종료할지 여부입니다. 연속된 통신이 필요할 경우 false로 설정합니다.</param>
        void Transfer(ReadOnlySpan<byte> tx, Span<byte> rx, bool endTransaction = true);

        /// <summary>
        /// SPI 버스를 통해 데이터를 전송(Write-Only)합니다. 수신되는 데이터는 무시됩니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS(Chip Select) 핀을 비활성화(High)하여 트랜잭션을 종료할지 여부입니다.</param>
        void Write(ReadOnlySpan<byte> tx, bool endTransaction = true);

        /// <summary>
        /// SPI 버스를 통해 데이터를 수신(Read-Only)합니다. 송신 데이터로는 일반적으로 더미(Dummy) 바이트가 전송됩니다.
        /// </summary>
        /// <param name="rx">수신된 데이터를 저장할 대상 바이트 버퍼입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS(Chip Select) 핀을 비활성화(High)하여 트랜잭션을 종료할지 여부입니다.</param>
        void Read(Span<byte> rx, bool endTransaction = true);
    }

    /// <summary>
    /// '시카고(Chicago)' 타겟 보드 또는 칩의 특수한 SPI 통신 규격을 지원하기 위한 확장 인터페이스입니다.
    /// 일반적인 SPI와 달리 통신 후 특정 핀의 상태를 강제로 유지해야 하는 등의 커스텀 동작을 정의합니다.
    /// </summary>
    public interface IChicagoSpiBus
    {
        /// <summary>
        /// 시카고 타겟 전용 SPI 쓰기 명령을 수행합니다. 전송 완료 후 지정된 핀 상태(Idle High)를 자동으로 유지합니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        void ChicagoWrite(ReadOnlySpan<byte> tx);

        /// <summary>
        /// 시카고 타겟 전용 복합(쓰기 후 읽기) 트랜잭션을 수행합니다. 반이중(Half-Duplex) 방향 전환이나 특정 대기 시간이 포함될 수 있습니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 버퍼입니다.</param>
        /// <param name="rx">수신된 데이터를 저장할 대상 바이트 버퍼입니다.</param>
        void ChicagoWriteRead(ReadOnlySpan<byte> tx, Span<byte> rx);

        /// <summary>
        /// 시카고 타겟 통신 버스를 강제로 대기(Idle High) 상태로 만듭니다. 주로 통신 에러 복구 나 초기화 시 사용됩니다.
        /// </summary>
        void ChicagoForceIdleHigh();
    }
}