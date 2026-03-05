using System.ComponentModel;

namespace SKAIChips_Verification_Tool.Instrument
{
    public class InstrumentInfo : INotifyPropertyChanged
    {
        string type;
        bool enabled;
        string visaAddress;
        string name;

        public string Type
        {
            get => type;
            set
            {
                if (type == value)
                    return;
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;
                enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public string VisaAddress
        {
            get => visaAddress;
            set
            {
                if (visaAddress == value)
                    return;
                visaAddress = value;
                OnPropertyChanged(nameof(VisaAddress));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
