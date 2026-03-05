using System.Collections.Generic;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 칩 내부의 단일 레지스터(Register) 정보를 표현하는 데이터 모델 클래스입니다.
    /// 하나의 레지스터 주소에 맵핑되며, 내부에 여러 개의 세부 비트 필드(RegisterItem)를 포함할 수 있습니다.
    /// </summary>
    public class RegisterDetail
    {
        /// <summary>
        /// 레지스터의 고유 이름입니다. (예: "SYS_CTRL_REG")
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// 칩 메모리 맵 상에서 이 레지스터가 위치한 물리적 주소(Address)입니다.
        /// </summary>
        public uint Address
        {
            get;
        }

        /// <summary>
        /// 칩이 리셋(Reset)되었을 때 이 레지스터가 가지는 초기 기본값(Default Value)입니다.
        /// </summary>
        public uint ResetValue
        {
            get; set;
        }

        /// <summary>
        /// 레지스터의 전체 비트 폭(Bit Width)을 지정합니다. (기본값: 32비트)
        /// 일반적으로 8, 16, 32비트 단위로 사용됩니다.
        /// </summary>
        public int BitWidth { get; set; } = 32;

        /// <summary>
        /// 이 레지스터를 구성하는 세부 비트 필드(기능별 항목)들의 목록입니다.
        /// (예: 0~3번 비트는 모드 설정, 4번 비트는 활성화 스위치 등)
        /// </summary>
        public List<RegisterItem> Items { get; } = new List<RegisterItem>();

        /// <summary>
        /// RegisterDetail 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">레지스터의 이름</param>
        /// <param name="address">레지스터의 메모리 주소</param>
        public RegisterDetail(string name, uint address)
        {
            Name = name;
            Address = address;
        }

        /// <summary>
        /// 레지스터 내부에 특정 기능을 수행하는 세부 비트 필드(항목)를 추가합니다.
        /// </summary>
        /// <param name="name">비트 필드의 이름 (예: "ENABLE_TX")</param>
        /// <param name="upperBit">비트 필드가 차지하는 최상위 비트(MSB) 위치</param>
        /// <param name="lowerBit">비트 필드가 차지하는 최하위 비트(LSB) 위치</param>
        /// <param name="defaultValue">해당 비트 필드의 초기 기본값</param>
        /// <param name="description">이 항목이 어떤 기능을 하는지에 대한 상세 설명</param>
        public void AddItem(string name, int upperBit, int lowerBit, uint defaultValue, string description)
        {
            Items.Add(new RegisterItem(name, upperBit, lowerBit, defaultValue, description));
        }
    }
}