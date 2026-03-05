using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 자동화 검증 테스트를 나타내는 메서드에 부여하는 사용자 정의 특성(Attribute)입니다.
    /// 이 특성이 붙은 메서드는 리플렉션을 통해 수집되며, UI의 테스트 목록 콤보박스에 자동으로 바인딩됩니다.
    /// </summary>[AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ChipTestAttribute : Attribute
    {
        /// <summary>
        /// 테스트가 속하는 대분류 또는 그룹 이름입니다. (예: "ADC Test", "Power Test")
        /// UI의 'Test Category' 콤보박스에 분류 기준으로 표시됩니다.
        /// </summary>
        public string Category
        {
            get;
        }

        /// <summary>
        /// 테스트의 고유 이름 또는 제목입니다.
        /// UI의 'Test' 콤보박스에 선택 가능한 항목으로 표시됩니다.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// 테스트가 수행하는 동작이나 검증 목적에 대한 부가적인 설명입니다.
        /// </summary>
        public string Description
        {
            get;
        }

        /// <summary>
        /// ChipTestAttribute 특성의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="category">테스트 카테고리 (예: "I2C Communication")</param>
        /// <param name="name">테스트 이름 (예: "Read/Write Register Validation")</param>
        /// <param name="description">테스트에 대한 상세 설명 (기본값: 빈 문자열)</param>
        public ChipTestAttribute(string category, string name, string description = "")
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
        }
    }
}