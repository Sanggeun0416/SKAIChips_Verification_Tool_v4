using System.ComponentModel;

namespace SKAIChips_Verification_Tool.Instrument
{
    /// <summary>
    /// 시스템에서 관리하는 개별 계측기(Instrument)의 설정 정보를 보관하는 모델 클래스입니다.
    /// INotifyPropertyChanged 인터페이스를 구현하여 UI 요소와 양방향 데이터 바인딩을 지원합니다.
    /// </summary>
    public class InstrumentInfo : INotifyPropertyChanged
    {
        private string type;
        private bool enabled;
        private string visaAddress;
        private string name;

        /// <summary>
        /// 계측기의 종류 또는 역할을 구분하는 타입입니다. (예: "Oscilloscope", "PowerSupply", "SMU")
        /// </summary>
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

        /// <summary>
        /// 해당 계측기를 검증 프로세스에서 활성화하여 사용할지 여부를 설정하거나 가져옵니다.
        /// </summary>
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

        /// <summary>
        /// 계측기와 통신하기 위한 고유한 VISA 리소스 주소입니다.
        /// (예: "TCPIP::192.168.0.10::INSTR", "USB0::0x0957::0x17A6::MY52350123::INSTR")
        /// </summary>
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

        /// <summary>
        /// 사용자가 식별하기 위해 부여한 계측기의 별칭(Alias) 또는 모델명입니다.
        /// </summary>
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

        /// <summary>
        /// 속성 값이 변경될 때 발생하는 이벤트입니다. UI 프레임워크에서 이 이벤트를 구독하여 화면을 갱신합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 속성 변경 이벤트를 발생시키는 헬퍼 메서드입니다.
        /// </summary>
        /// <param name="propertyName">변경될 속성의 이름</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}