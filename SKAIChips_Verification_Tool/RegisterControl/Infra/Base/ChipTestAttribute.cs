namespace SKAIChips_Verification_Tool.RegisterControl
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ChipTestAttribute : Attribute
    {
        public string Category
        {
            get;
        }
        public string Name
        {
            get;
        }
        public string Description
        {
            get;
        }

        public ChipTestAttribute(string category, string name, string description = "")
        {
            Category = category;
            Name = name;
            Description = description;
        }
    }
}