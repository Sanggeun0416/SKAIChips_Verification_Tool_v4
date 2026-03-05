namespace SKAIChips_Verification_Tool.RegisterControl
{
    public interface IGpioController
    {
        void SetGpioDirection(int pinIndex, bool isOutput);

        void SetGpioValue(int pinIndex, bool isHigh);

        bool GetGpioValue(int pinIndex);
    }
}