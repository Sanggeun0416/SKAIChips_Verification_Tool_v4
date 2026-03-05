using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 메인 UI의 커스텀 테스트 슬롯(Test Slot 01~10) 버튼에 바인딩할 사용자 정의 동작을 정의하는 클래스입니다.
    /// 각 칩 프로젝트(IChipProject)별로 자주 사용하는 하드웨어 제어 기능이나 스크립트를 원클릭 버튼으로 구성할 때 사용됩니다.
    /// </summary>
    public class TestSlotAction
    {
        /// <summary>
        /// UI 버튼에 표시될 텍스트(이름)입니다.
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// 해당 버튼을 클릭했을 때 백그라운드 스레드에서 비동기적으로 실행될 동작(Action) 델리게이트입니다.
        /// </summary>
        public Action Action
        {
            get; set;
        }

        /// <summary>
        /// 버튼의 클릭 가능(Enabled) 여부를 설정합니다. 기본값은 true입니다.
        /// (단, 하드웨어가 연결된 상태에서만 이 값이 UI에 반영됩니다.)
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 버튼의 화면 표시(Visible) 여부를 설정합니다. 기본값은 true입니다.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// TestSlotAction 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="name">버튼에 표시될 이름 (필수)</param>
        /// <param name="action">버튼 클릭 시 실행할 동작, 메서드 또는 람다식 (필수)</param>
        /// <param name="isEnabled">버튼 초기 활성화 여부 (기본값: true)</param>
        /// <exception cref="ArgumentNullException">이름이나 액션이 null일 경우 발생합니다.</exception>
        public TestSlotAction(string name, Action action, bool isEnabled = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            IsEnabled = isEnabled;
        }
    }
}