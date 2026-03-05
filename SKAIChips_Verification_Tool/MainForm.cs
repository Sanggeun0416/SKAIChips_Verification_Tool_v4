using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SKAIChips_Verification_Tool.HCIControl;
using SKAIChips_Verification_Tool.Instrument;
using SKAIChips_Verification_Tool.RegisterControl;

namespace SKAIChips_Verification_Tool
{
    public partial class MainForm : Form
    {
        // 계측기 설정 폼을 관리하기 위한 변수
        private InstrumentForm _instrumentForm;

        // 애플리케이션 정보 상수
        public static string AppName { get; } = "SKAIChips_Verification";
        public static string Version { get; } = "v4.0.0 [Confidential]";

        public MainForm()
        {
            InitializeComponent();

            // MDI 자식 창 활성화 이벤트 연결
            this.MdiChildActivate += MainForm_MdiChildActivate;

            // 계측기 레지스트리 설정 로드
            InstrumentRegistry.Instance.Load();

            // 초기 메뉴 상태 설정
            menuSimpleSerial.Enabled = false;
            menuFile.Visible = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 타이틀 바에 앱 이름과 버전 표시
            Text = $"{AppName} {Version}";

            // 프로그램 시작 시 RegisterControlForm을 기본으로 띄움
            OpenOrShowForm<RegisterControlForm>();
        }

        /// <summary>
        /// MDI 자식 폼을 열거나, 이미 열려있다면 활성화(Bring to Front)하는 제네릭 메서드
        /// </summary>
        private void OpenOrShowForm<T>() where T : Form, new()
        {
            // 이미 열려있는 해당 타입의 폼이 있는지 확인
            var form = MdiChildren.OfType<T>().FirstOrDefault();
            if (form != null)
            {
                // 최소화 상태라면 원래대로 복구
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form.Activate();
                return;
            }

            // 없으면 새로 생성
            var newForm = new T
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };

            newForm.Show();
        }

        /// <summary>
        /// 활성화된 자식 창(MDI Child)이 변경될 때 메뉴 상태를 갱신
        /// </summary>
        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            // 현재 활성화된 창이 RegisterControlForm인지 확인
            bool isRegFormActive = this.ActiveMdiChild is RegisterControlForm;

            // RegisterControlForm일 때만 계측기 설정 및 연결 메뉴 활성화
            if (menuSetupInstrument != null)
                menuSetupInstrument.Enabled = isRegFormActive;

            if (getsAllToolStripMenuItem != null)
                getsAllToolStripMenuItem.Enabled = isRegFormActive;
        }

        #region Menu Click Events

        private void menuRegisterControl_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<RegisterControlForm>();
        }

        private void menuHCIControl_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<HCIControlForm>();
        }

        private void menuSimpleSerial_Click(object sender, EventArgs e)
        {
            OpenOrShowForm<SimpleSerialForm>();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        /// <summary>
        /// 계측기 설정 창 열기 (Modeless Dialog 처럼 동작하되 중복 실행 방지)
        /// </summary>
        private void menuSetupInstrument_Click(object sender, EventArgs e)
        {
            if (_instrumentForm == null || _instrumentForm.IsDisposed)
            {
                _instrumentForm = new InstrumentForm
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                _instrumentForm.Show(this);
            }
            else
            {
                if (!_instrumentForm.Visible)
                    _instrumentForm.Show(this);

                _instrumentForm.Activate();
            }
        }

        /// <summary>
        /// 등록된 모든 계측기에 연결을 시도하고 *IDN? 쿼리를 날리는 기능
        /// 비동기(Async)로 동작하여 UI 멈춤 방지
        /// </summary>
        private async void getsAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var regForm = this.ActiveMdiChild as RegisterControlForm;
            if (regForm == null)
                return;

            regForm.AppendLog("[System] Try to Connect All Instruments...");

            var instrumentTypes = InstrumentRegistry.Instance.GetEnabledInstrumentTypes();

            int successCount = 0;
            int totalCount = 0;

            await Task.Run(() =>
            {
                foreach (var typeName in instrumentTypes)
                {
                    totalCount++;
                    try
                    {
                        var instrument = InstrumentRegistry.Instance.GetByType(typeName);

                        // SCPI 표준 명령어로 장비 ID 확인
                        string idn = instrument.Query("*IDN?");

                        regForm.AppendLog($"[OK] {typeName}: {idn.Trim()}");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        regForm.AppendLog($"[FAIL] {typeName}: {ex.Message}");
                    }
                }
            });

            regForm.AppendLog($"[Result] {successCount} / {totalCount} Instruments Connected.");
        }

        /// <summary>
        /// 폼 종료 시 사용자 확인
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "프로그램을 정말 종료하시겠습니까?\n(진행 중인 모든 연결이 끊어집니다.)",
                "종료 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }
    }
}