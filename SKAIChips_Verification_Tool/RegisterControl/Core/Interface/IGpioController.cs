namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 디바이스의 범용 입출력(GPIO) 핀을 제어하기 위한 표준 인터페이스입니다.
    /// 통신 칩(FT4222H, UM232H 등)의 종류와 무관하게 일관된 방식으로 GPIO 방향 및 상태를 설정하고 읽어올 수 있도록 추상화합니다.
    /// </summary>
    public interface IGpioController
    {
        /// <summary>
        /// 지정된 논리적 GPIO 핀의 데이터 흐름 방향(입력 또는 출력)을 설정합니다.
        /// </summary>
        /// <param name="pinIndex">제어할 GPIO 핀의 인덱스 번호입니다.</param>
        /// <param name="isOutput">true로 설정하면 출력(Output) 모드, false로 설정하면 입력(Input) 모드가 됩니다.</param>
        void SetGpioDirection(int pinIndex, bool isOutput);

        /// <summary>
        /// 출력 모드로 설정된 특정 GPIO 핀에 디지털 신호(High/Low)를 출력합니다.
        /// </summary>
        /// <param name="pinIndex">신호를 출력할 GPIO 핀의 인덱스 번호입니다.</param>
        /// <param name="isHigh">true일 경우 High(1) 신호를, false일 경우 Low(0) 신호를 출력합니다.</param>
        void SetGpioValue(int pinIndex, bool isHigh);

        /// <summary>
        /// 입력 모드로 설정된 특정 GPIO 핀의 현재 디지털 신호 상태(High/Low)를 읽어옵니다.
        /// </summary>
        /// <param name="pinIndex">상태를 읽어올 GPIO 핀의 인덱스 번호입니다.</param>
        /// <returns>핀의 상태가 High(1)이면 true, Low(0)이면 false를 반환합니다.</returns>
        bool GetGpioValue(int pinIndex);
    }
}