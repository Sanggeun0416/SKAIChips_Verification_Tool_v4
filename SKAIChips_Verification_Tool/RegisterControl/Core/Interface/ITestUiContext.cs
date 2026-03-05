namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 검증 테스트 시나리오(IChipTestSuite) 실행 중 필요한 사용자 인터페이스(UI) 상호작용을 추상화한 인터페이스입니다.
    /// 핵심 비즈니스 로직(Domain Layer)이 특정 UI 프레임워크(WinForms 등)에 강하게 결합(Coupling)되지 않도록 분리하는 역할을 합니다.
    /// </summary>
    public interface ITestUiContext
    {
        /// <summary>
        /// 사용자에게 파일 선택 대화상자(File Dialog)를 띄우고 선택된 파일의 경로를 반환합니다.
        /// 주로 테스트에 필요한 설정 파일이나 데이터 양식을 불러올 때 사용됩니다.
        /// </summary>
        /// <param name="filter">파일 확장자 필터 (예: "Excel Files|*.xlsx|All Files|*.*")</param>
        /// <param name="title">대화상자 창 상단에 표시될 제목입니다.</param>
        /// <returns>사용자가 선택한 파일의 절대 경로입니다. 취소하거나 창을 닫은 경우 null을 반환합니다.</returns>
        string? OpenFileDialog(string filter, string title);

        /// <summary>
        /// 사용자로부터 간단한 텍스트나 값을 입력받을 수 있는 프롬프트 팝업 창을 띄웁니다.
        /// 테스트 실행 전 동적인 파라미터(예: 보드 시리얼 번호, 반복 횟수 등)를 입력받을 때 유용합니다.
        /// </summary>
        /// <param name="title">프롬프트 팝업 창 상단에 표시될 제목입니다.</param>
        /// <param name="label">입력란 옆이나 위에 표시될 안내 문구(레이블)입니다.</param>
        /// <param name="defaultValue">입력란에 미리 채워둘 기본값입니다.</param>
        /// <returns>사용자가 입력하고 확인한 문자열입니다. 취소하거나 창을 닫은 경우 null을 반환합니다.</returns>
        string? PromptInput(string title, string label, string defaultValue);
    }
}