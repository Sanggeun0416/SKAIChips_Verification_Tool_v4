namespace SKAIChips_Verification_Tool.RegisterControl
{
    public interface ITestUiContext
    {
        string? OpenFileDialog(string filter, string title);
        string? PromptInput(string title, string label, string defaultValue);
    }
}