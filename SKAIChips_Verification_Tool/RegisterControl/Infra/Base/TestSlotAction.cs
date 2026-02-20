namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class TestSlotAction
    {
        public string Name
        {
            get; set;
        }        // 버튼 텍스트
        public Action Action
        {
            get; set;
        }      // 클릭 시 실행할 동작
        public bool IsEnabled { get; set; } = true; // 활성화 여부
        public bool IsVisible { get; set; } = true; // 표시 여부

        public TestSlotAction(string name, Action action, bool isEnabled = true)
        {
            Name = name;
            Action = action;
            IsEnabled = isEnabled;
        }
    }
}