namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 레지스터 제어에 사용될 물리적 통신 프로토콜의 종류를 정의합니다.
    /// </summary>
    public enum ProtocolRegLogType
    {
        /// <summary> Inter-Integrated Circuit 통신 </summary>
        I2C,
        /// <summary> Serial Peripheral Interface 통신 </summary>
        SPI
    }

    /// <summary>
    /// PC와 타겟 보드를 연결하는 데 사용되는 FTDI 인터페이스 장치의 종류를 정의합니다.
    /// </summary>
    public enum DeviceKind
    {
        /// <summary> FT4222H 고속 USB 브릿지 칩 </summary>
        FT4222,
        /// <summary> UM232H (FT232H 기반) 범용 하이 스피드 USB 모듈 </summary>
        UM232H
    }

    /// <summary>
    /// 칩과 통신하기 위한 프로토콜 및 하드웨어 장치의 세부 설정값을 보관하는 클래스입니다.
    /// 선택된 프로토콜(I2C/SPI)에 따라 필요한 클럭 속도, 주소, 모드 등의 파라미터를 제공합니다.
    /// </summary>
    public sealed class ProtocolSettings
    {
        /// <summary>
        /// 현재 사용할 통신 프로토콜 방식(I2C 또는 SPI)을 설정하거나 가져옵니다.
        /// </summary>
        public ProtocolRegLogType ProtocolRegLogType
        {
            get; set;
        }

        /// <summary>
        /// 통신에 사용할 실제 물리적 FTDI 장치의 종류를 설정하거나 가져옵니다.
        /// </summary>
        public DeviceKind DeviceKind
        {
            get; set;
        }

        /// <summary>
        /// [SPI 전용] 장치 연결 직후 특정 핀 상태를 Idle High(대기 상태 High)로 강제 고정할지 여부를 설정합니다.
        /// 주로 Chicago 프로젝트와 같은 특수 타겟의 통신 안정성을 위해 사용됩니다.
        /// </summary>
        public bool ForceIdleHighOnConnect
        {
            get; set;
        }

        #region I2C Specific Settings
        /// <summary>
        /// [I2C 전용] 통신 속도를 Kbps 단위로 설정하거나 가져옵니다. (기본값: 400Kbps)
        /// </summary>
        public int SpeedKbps { get; set; } = 400;

        /// <summary>
        /// [I2C 전용] 통신 대상이 되는 슬레이브 칩의 7비트 하드웨어 주소입니다.
        /// </summary>
        public byte I2cSlaveAddress { get; set; } = 0x00;
        #endregion

        #region SPI Specific Settings
        /// <summary>
        /// [SPI 전용] 통신 클럭 주파수를 KHz 단위로 설정하거나 가져옵니다. (기본값: 1000KHz / 1MHz)
        /// </summary>
        public int SpiClockKHz { get; set; } = 1000;

        /// <summary>
        /// [SPI 전용] SPI 통신 모드(CPOL/CPHA 조합, 0~3)를 설정하거나 가져옵니다.
        /// </summary>
        public int SpiMode { get; set; } = 0;

        /// <summary>
        /// [SPI 전용] 데이터 전송 시 최하위 비트(LSB)부터 보낼지 여부를 설정합니다. 
        /// false일 경우 최상위 비트(MSB)부터 전송합니다. (기본값: false / MSB First)
        /// </summary>
        public bool SpiLsbFirst { get; set; } = false;
        #endregion
    }
}