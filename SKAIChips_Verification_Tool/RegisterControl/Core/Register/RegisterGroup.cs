namespace SKAIChips_Verification_Tool.RegisterControl
{
    public class RegisterGroup
    {

        public string Name
        {
            get;
        }

        public List<RegisterDetail> Registers { get; } = new List<RegisterDetail>();

        public RegisterGroup(string name)
        {
            Name = name;
        }

        public RegisterDetail AddRegister(string name, uint address)
        {
            var reg = new RegisterDetail(name, address);
            Registers.Add(reg);
            return reg;
        }

    }
}
