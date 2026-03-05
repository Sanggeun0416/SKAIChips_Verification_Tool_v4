namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 단일 레지스터(RegisterDetail) 내부에 포함된 개별 세부 비트 필드(Bit-field) 항목을 표현하는 데이터 모델 클래스입니다.
    /// 레지스터의 특정 비트 구간이 담당하는 기능, 기본값, 비트 위치(MSB/LSB) 정보를 담고 있습니다.
    /// </summary>
    public class RegisterItem
    {
        /// <summary>
        /// 비트 필드의 고유 이름입니다. (예: "MODE_SELECT", "ENABLE_TX")
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// 이 비트 필드가 차지하는 구간의 최상위 비트(MSB, Most Significant Bit) 인덱스입니다.
        /// </summary>
        public int UpperBit
        {
            get;
        }

        /// <summary>
        /// 이 비트 필드가 차지하는 구간의 최하위 비트(LSB, Least Significant Bit) 인덱스입니다.
        /// 단일 비트(1-bit) 스위치 역할을 하는 항목의 경우 UpperBit와 동일한 값을 가집니다.
        /// </summary>
        public int LowerBit
        {
            get;
        }

        /// <summary>
        /// 하드웨어 칩이 초기화(Reset)되었을 때 이 비트 필드가 기본적으로 가지는 초기값입니다.
        /// </summary>
        public uint DefaultValue
        {
            get;
        }

        /// <summary>
        /// 이 비트 필드가 어떤 기능을 수행하는지, 또는 각 설정값(0, 1, 2...)이 무엇을 의미하는지에 대한 상세 설명입니다.
        /// </summary>
        public string Description
        {
            get;
        }

        /// <summary>
        /// RegisterItem 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">비트 필드의 이름</param>
        /// <param name="upperBit">최상위 비트(MSB) 위치</param>
        /// <param name="lowerBit">최하위 비트(LSB) 위치</param>
        /// <param name="defaultValue">필드의 초기 기본값</param>
        /// <param name="description">필드의 기능 설명</param>
        public RegisterItem(string name, int upperBit, int lowerBit, uint defaultValue, string description)
        {
            Name = name;
            UpperBit = upperBit;
            LowerBit = lowerBit;
            DefaultValue = defaultValue;
            Description = description;
        }
    }
}