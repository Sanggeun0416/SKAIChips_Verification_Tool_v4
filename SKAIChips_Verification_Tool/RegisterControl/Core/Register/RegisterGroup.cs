using System.Collections.Generic;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 관련된 여러 개의 레지스터(RegisterDetail)들을 논리적인 묶음으로 관리하기 위한 컨테이너 클래스입니다.
    /// 전체 레지스터 맵(Register Map)을 구성할 때 '그룹(Group) -> 레지스터(Register) -> 비트 필드(Item)' 계층 구조의 최상위를 담당합니다.
    /// </summary>
    public class RegisterGroup
    {
        /// <summary>
        /// 레지스터 그룹의 이름입니다. (예: "Power Management", "ADC Control", "System Configuration")
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// 이 그룹에 포함된 개별 레지스터(RegisterDetail) 객체들의 목록입니다.
        /// </summary>
        public List<RegisterDetail> Registers { get; } = new List<RegisterDetail>();

        /// <summary>
        /// RegisterGroup 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">생성할 그룹의 이름입니다.</param>
        public RegisterGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 이 그룹에 새로운 레지스터를 생성하여 추가하고, 생성된 레지스터 객체를 반환합니다.
        /// (반환된 객체에 메서드 체이닝 방식으로 비트 필드(Item)를 쉽게 추가할 수 있습니다.)
        /// </summary>
        /// <param name="name">추가할 레지스터의 고유 이름입니다.</param>
        /// <param name="address">추가할 레지스터의 메모리 맵 상 물리적 주소입니다.</param>
        /// <returns>생성 및 그룹에 추가된 새 RegisterDetail 인스턴스입니다.</returns>
        public RegisterDetail AddRegister(string name, uint address)
        {
            var reg = new RegisterDetail(name, address);
            Registers.Add(reg);
            return reg;
        }
    }
}