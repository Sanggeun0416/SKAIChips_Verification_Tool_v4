namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class RegisterDetail
    {

        public string Name
        {
            get;
        }
        public uint Address
        {
            get;
        }
        public uint ResetValue
        {
            get; set;
        }
        public List<RegisterItem> Items { get; } = new List<RegisterItem>();

        public RegisterDetail(string name, uint address)
        {
            Name = name;
            Address = address;
        }

        public void AddItem(string name, int upperBit, int lowerBit, uint defaultValue, string description)
        {
            Items.Add(new RegisterItem(name, upperBit, lowerBit, defaultValue, description));
        }

    }
}
