namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// PC에 연결된 FTDI USB 디바이스의 하드웨어 식별 정보 및 설정값을 담는 데이터 모델 클래스입니다.
    /// 주로 장치 검색(Enumeration) 결과를 UI 목록에 표시하거나, 특정 디바이스를 열기 위한 식별자로 사용됩니다.
    /// </summary>
    public class FtdiDeviceSettings
    {
        /// <summary>
        /// FTDI 드라이버가 인식한 장치 목록 내에서의 0 기반 인덱스(Index) 번호입니다.
        /// 디바이스를 열(Open) 때 핵심적인 식별자로 사용됩니다.
        /// </summary>
        public int DeviceIndex
        {
            get; set;
        }

        /// <summary>
        /// FTDI 칩의 EEPROM에 기록되어 있는 제품 설명(Description) 문자열입니다.
        /// (예: "UM232H", "FT4222H A")
        /// </summary>
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// FTDI 칩의 고유 시리얼 번호(Serial Number) 문자열입니다.
        /// 여러 대의 동일한 평가 보드가 연결되어 있을 때 각 보드를 정확히 구분하는 용도로 사용됩니다.
        /// </summary>
        public string SerialNumber
        {
            get; set;
        }

        /// <summary>
        /// 디바이스가 연결된 USB 포트의 물리적/논리적 위치(Location ID) 정보입니다.
        /// </summary>
        public string Location
        {
            get; set;
        }

        /// <summary>
        /// 디바이스의 주요 정보를 UI의 목록(ListBox, ComboBox 등)에 알아보기 쉽게 표시하기 위해 
        /// 인덱스, 설명, 시리얼 번호를 조합한 문자열을 반환합니다.
        /// </summary>
        /// <returns>"[인덱스] - [설명] ([시리얼번호])" 형태의 문자열</returns>
        public override string ToString()
        {
            return $"{DeviceIndex} - {Description} ({SerialNumber})";
        }
    }
}