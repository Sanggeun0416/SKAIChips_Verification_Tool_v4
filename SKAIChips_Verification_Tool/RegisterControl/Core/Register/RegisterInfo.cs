namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 레지스터의 기본 정보(주소, 리셋 값, 원본 소스 위치 등)를 담는 평면적(Flat) 데이터 모델 클래스입니다.
    /// 주로 엑셀(Excel) 형태의 레지스터 맵 문서에서 하나의 행(Row) 데이터를 파싱하여 
    /// 화면(UI)에 리스트 형태로 표시하거나 내부 데이터로 활용할 때 사용되는 DTO(Data Transfer Object)입니다.
    /// </summary>
    public class RegisterInfo
    {
        /// <summary>
        /// 이 레지스터 정보가 정의되어 있던 원본 문서(엑셀 파일)의 워크시트(Sheet) 이름입니다.
        /// </summary>
        public string Sheet
        {
            get; set;
        }

        /// <summary>
        /// 레지스터가 속한 기능적 그룹 또는 카테고리 이름입니다. (예: "System_Control")
        /// </summary>
        public string Group
        {
            get; set;
        }

        /// <summary>
        /// 레지스터의 고유 이름입니다.
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// 칩 메모리 맵 상에서의 레지스터 물리적 주소(Address)입니다.
        /// </summary>
        public uint Address
        {
            get; set;
        }

        /// <summary>
        /// 레지스터 주소를 UI에 표시하기 좋게 32비트 16진수 문자열 형식(예: "0x000000A4")으로 변환하여 반환합니다.
        /// </summary>
        public string AddressText => $"0x{Address:X8}";

        /// <summary>
        /// 칩 초기화(Reset) 시 이 레지스터가 가지는 초기 기본값(Default Value)입니다.
        /// </summary>
        public uint Reset
        {
            get; set;
        }

        /// <summary>
        /// 레지스터 리셋 값을 UI에 표시하기 좋게 32비트 16진수 문자열 형식(예: "0x00000000")으로 변환하여 반환합니다.
        /// </summary>
        public string ResetText => $"0x{Reset:X8}";

        /// <summary>
        /// 레지스터의 전반적인 기능, 주의사항 등에 대한 상세 설명 또는 비고 내용입니다.
        /// </summary>
        public string Description
        {
            get; set;
        }

        /// <summary>
        /// 이 레지스터 객체의 정보를 "시트/그룹 - 이름 (주소)" 형태의 직관적인 문자열로 반환합니다.
        /// (디버깅이나 간단한 콤보박스 항목 표시 등에 유용합니다.)
        /// </summary>
        /// <returns>레지스터 정보가 요약된 문자열</returns>
        public override string ToString() => $"{Sheet}/{Group} - {Name} ({AddressText})";
    }
}