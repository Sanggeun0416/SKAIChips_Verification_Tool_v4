namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 대상 하드웨어 칩의 내부 레지스터(Register)에 접근하여 값을 읽고 쓰기 위한 공통 인터페이스입니다.
    /// 통신 방식(I2C, SPI 등)이나 하드웨어 종류에 구애받지 않고,
    /// 상위 로직에서 일관된 방식으로 칩의 메모리 맵(Memory Map)을 제어할 수 있도록 추상화합니다.
    /// </summary>
    public interface IRegisterChip
    {
        /// <summary>
        /// 현재 제어 대상이 되는 칩의 이름 또는 식별자를 가져옵니다.
        /// (예: "SKAI_PowerIC_v1", "Sensor_AFE")
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 지정된 메모리 주소(Address)에 위치한 레지스터의 현재 값을 읽어옵니다.
        /// </summary>
        /// <param name="address">값을 읽어올 레지스터의 주소입니다.</param>
        /// <returns>해당 레지스터에 저장된 데이터 값입니다.</returns>
        uint ReadRegister(uint address);

        /// <summary>
        /// 지정된 메모리 주소(Address)의 레지스터에 새로운 데이터 값을 씁니다.
        /// </summary>
        /// <param name="address">데이터를 기록할 대상 레지스터의 주소입니다.</param>
        /// <param name="data">레지스터에 기록할 새로운 데이터 값입니다.</param>
        void WriteRegister(uint address, uint data);
    }
}