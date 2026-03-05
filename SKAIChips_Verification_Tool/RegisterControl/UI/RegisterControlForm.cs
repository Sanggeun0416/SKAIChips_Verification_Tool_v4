using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 레지스터 제어, 하드웨어 통신(I2C/SPI), 엑셀 레지스터 맵 파싱, 및 자동화 테스트를 수행하는 메인 UI 폼입니다.
    /// ITestUiContext를 구현하여 외부 테스트 스위트에서 UI 기능에 접근할 수 있도록 합니다.
    /// </summary>
    public partial class RegisterControlForm : Form, ITestUiContext
    {
        #region Fields

        /// <summary>I2C 통신을 위한 버스 인터페이스입니다.</summary>
        private II2cBus? _i2cBus;

        /// <summary>SPI 통신을 위한 버스 인터페이스입니다.</summary>
        private ISpiBus? _spiBus;

        /// <summary>현재 연결된 레지스터 칩 인스턴스입니다.</summary>
        private IRegisterChip? _chip;

        /// <summary>하드웨어 통신 시 적용할 기본 타임아웃(밀리초)입니다.</summary>
        private const int I2cTimeoutMs = 1000;

        /// <summary>FTDI 장치 설정 정보입니다.</summary>
        private FtdiDeviceSettings? _ftdiSettings;

        /// <summary>통신 프로토콜(I2C/SPI 등) 설정 정보입니다.</summary>
        private ProtocolSettings? _protocolSettings;

        /// <summary>지원 가능한 칩 프로젝트 목록입니다.</summary>
        private readonly List<IChipProject> _projects = new();

        /// <summary>현재 선택된 칩 프로젝트입니다.</summary>
        private IChipProject? _selectedProject;

        /// <summary>프로그램이 직접 실행하여 추적 중인 엑셀 프로세스 목록입니다.</summary>
        private readonly List<Process> _openedExcelProcesses = new();

        /// <summary>현재 로드된 레지스터 맵(엑셀) 파일의 경로입니다.</summary>
        private string? _regMapFilePath;

        /// <summary>파싱된 레지스터 그룹(시트) 목록입니다.</summary>
        private readonly List<RegisterGroup> _groups = new();

        /// <summary>트리에서 현재 선택된 레지스터 그룹(시트)입니다.</summary>
        private RegisterGroup? _selectedGroup;

        /// <summary>트리에서 현재 선택된 레지스터입니다.</summary>
        private RegisterDetail? _selectedRegister;

        /// <summary>트리에서 현재 선택된 레지스터의 세부 항목(비트 필드)입니다.</summary>
        private RegisterItem? _selectedItem;

        /// <summary>현재 포커스된 레지스터의 32비트 값입니다.</summary>
        private uint _currentRegValue;

        /// <summary>레지스터 값이 UI나 코드로 업데이트 중인지 여부를 나타내는 플래그입니다.</summary>
        private bool _isUpdatingRegValue;

        /// <summary>현재 로드되거나 저장할 스크립트 파일의 경로입니다.</summary>
        private string? _scriptFilePath;

        /// <summary>현재 로드된 펌웨어 파일의 경로입니다.</summary>
        private string? _firmwareFilePath;

        /// <summary>레지스터 필드를 관리하는 매니저 객체입니다.</summary>
        private RegisterFieldManager? _regMgr;

        /// <summary>트리 노드와 레지스터 디테일을 매핑하는 캐시 딕셔너리입니다.</summary>
        private readonly Dictionary<RegisterDetail, List<TreeNode>> _registerNodeCache = new();

        /// <summary>32비트 레지스터 값을 개별적으로 토글할 수 있는 버튼 배열입니다.</summary>
        private readonly Button[] _bitButtons = new Button[32];

        /// <summary>비트 버튼 값이 업데이트 중인지 여부를 나타내는 플래그입니다.</summary>
        private bool _isUpdatingBits;

        private int _prevTestCategoryIndex = -1;
        private bool _suppressTestCategoryEvent;

        /// <summary>트리뷰에서 다중 선택된 노드 목록입니다.</summary>
        private readonly List<TreeNode> _selectedNodes = new();
        private TreeNode? _pivotNode;

        /// <summary>각 레지스터의 현재 값을 저장하는 딕셔너리입니다.</summary>
        private readonly Dictionary<RegisterDetail, uint> _regValues = new();

        /// <summary>현재 프로젝트의 테스트 스위트 인스턴스입니다.</summary>
        private IChipTestSuite? _testSuite;

        /// <summary>실행 중인 테스트를 취소하기 위한 토큰 소스입니다.</summary>
        private CancellationTokenSource? _testCts;

        /// <summary>현재 테스트가 실행 중인지 여부를 나타냅니다.</summary>
        private bool _isRunningTest;

        /// <summary>사용자 정의 동작을 할당할 수 있는 10개의 테스트 슬롯 버튼 배열입니다.</summary>
        private Button[] _testSlotButtons;

        #endregion

        /// <summary>
        /// 레지스터 맵이 로드되지 않았을 경우 예외를 발생시키며 매니저 객체를 반환합니다.
        /// </summary>
        public RegisterFieldManager RegMgr => _regMgr ?? throw new InvalidOperationException("Register map not loaded.");

        /// <summary>
        /// RegisterControlForm 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public RegisterControlForm()
        {
            InitializeComponent();
            InitUi();
        }

        /// <summary>
        /// 폼이 닫힐 때 발생하는 이벤트를 재정의하여 리소스를 해제합니다.
        /// 진행 중인 테스트 취소, 버스 연결 해제, 추적 중인 엑셀 좀비 프로세스 강제 종료를 수행합니다.
        /// </summary>
        /// <param name="e">폼 닫힘 이벤트 인자입니다.</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            try
            {
                _testCts?.Cancel();
            }
            catch { }
            DisconnectBus();

            foreach (var p in _openedExcelProcesses)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(500);
                        if (!p.HasExited)
                            p.Kill();
                    }
                }
                catch { }
            }
            _openedExcelProcesses.Clear();
        }

        /// <summary>
        /// 폼의 모든 UI 요소(로그 그리드, 메뉴, 트리, 버튼 등)를 초기화합니다.
        /// </summary>
        private void InitUi()
        {
            InitLogGrid();
            InitTreeContextMenu();
            InitBitButtons();
            InitBitButtonLayoutHandlers();
            InitRegisterMapControls();
            InitRegisterValueControls();
            InitScriptControls();
            InitStatusControls();
            InitRunTestUi();
            InitializeTestSlotButtons();
            LoadProjects();
            UpdateStatusText();

            tvRegTree.HideSelection = false;
            tvRegTree.MouseDown += TvRegTree_MouseDown;
        }

        /// <summary>
        /// 레지스터 맵 파일 관련 컨트롤의 초기 상태를 설정합니다.
        /// </summary>
        private void InitRegisterMapControls()
        {
            lblMapFileName.Text = "(No file)";
            btnOpenMapPath.Enabled = false;
        }

        /// <summary>
        /// 레지스터 값 제어(Read/Write)와 관련된 UI 컨트롤을 초기화합니다.
        /// </summary>
        private void InitRegisterValueControls()
        {
            UpdateBitButtonsFromValue(_currentRegValue);
            SetBitButtonsEnabledForItem(null);

            txtRegValueHex.Leave += txtRegValueHex_Leave;
            txtRegValueHex.KeyDown += TxtRegValueHex_KeyDown;

            btnWriteAll.Click += btnWriteAll_Click;
            btnReadAll.Click += btnReadAll_Click;

            lblRegName.Text = "(No Register)";
            lblRegAddrSummary.Text = "Address: -";
            lblRegResetSummary.Text = "Reset Value: -";
            txtRegValueHex.Text = "0x00000000";

            numRegIndex.Minimum = 0;
            numRegIndex.Maximum = 0;
            numRegIndex.Value = 0;
            numRegIndex.Enabled = false;
            numRegIndex.ValueChanged += numRegIndex_ValueChanged;
        }

        /// <summary>
        /// 스크립트 파일 로드/저장과 관련된 UI 컨트롤을 초기화합니다.
        /// </summary>
        private void InitScriptControls()
        {
            lblScriptFileName.Text = "(No script)";
            btnOpenScriptPath.Enabled = false;
        }

        /// <summary>
        /// 하드웨어 연결 상태를 표시하는 컨트롤을 초기화합니다.
        /// </summary>
        private void InitStatusControls()
        {
            btnConnect.Text = "Connect";
            UpdateStatusText();
            UpdateConnectionControls();
        }

        /// <summary>
        /// 테스트 자동화 실행과 관련된 UI 컨트롤을 초기화합니다.
        /// </summary>
        private void InitRunTestUi()
        {
            cmbTestCategory.Items.Clear();
            cmbTest.Items.Clear();
            btnRunTest.Enabled = false;
            btnStopTest.Enabled = false;
            rtbRunTestLog.Clear();

            InitTestLogContextMenu();

            cmbTestCategory.SelectedIndexChanged += comboTestCategory_SelectedIndexChanged;
            btnRunTest.Click += btnRunTest_Click;
            btnStopTest.Click += btnStopTest_Click;
        }

        /// <summary>
        /// 특정 트리 노드의 선택 상태에 따른 시각적 스타일을 업데이트합니다.
        /// </summary>
        /// <param name="node">대상 트리 노드입니다.</param>
        /// <param name="isSelected">선택 여부입니다.</param>
        private void UpdateNodeVisual(TreeNode node, bool isSelected)
        {
            if (node == null)
                return;

            RefreshNodeStyle(node);

            if (node.Parent != null)
            {
                RefreshNodeStyle(node.Parent);
                foreach (TreeNode sibling in node.Parent.Nodes)
                {
                    RefreshNodeStyle(sibling);
                }
            }

            foreach (TreeNode child in node.Nodes)
            {
                RefreshNodeStyle(child);
            }
        }

        /// <summary>
        /// 트리뷰에서 다중 선택되거나 활성화된 모든 레지스터 디테일 목록을 수집하여 반환합니다.
        /// </summary>
        /// <returns>선택된 레지스터 목록 (주소순 정렬)</returns>
        private List<RegisterDetail> GetSelectedRegisters()
        {
            var targetRegs = new HashSet<RegisterDetail>();

            foreach (var node in _selectedNodes)
            {
                if (node.Tag is RegisterDetail reg)
                {
                    targetRegs.Add(reg);
                }
                else if (node.Tag is RegisterItem && node.Parent?.Tag is RegisterDetail parentReg)
                {
                    targetRegs.Add(parentReg);
                }
                else if (node.Tag is RegisterGroup group)
                {
                    foreach (var r in group.Registers)
                        targetRegs.Add(r);
                }
            }

            return targetRegs.OrderBy(r => r.Address).ToList();
        }

        /// <summary>
        /// 현재 선택된 칩 프로젝트의 이름을 반환합니다.
        /// </summary>
        /// <returns>프로젝트 이름 문자열</returns>
        private string GetCurrentProjectName()
        {
            return _selectedProject?.Name ?? "UnknownProject";
        }

        /// <summary>
        /// 10개의 테스트 슬롯 버튼 배열을 할당하고 클릭 이벤트를 바인딩합니다.
        /// </summary>
        private void InitializeTestSlotButtons()
        {
            _testSlotButtons = new Button[]
            {
                btnTestSlot01, btnTestSlot02, btnTestSlot03, btnTestSlot04, btnTestSlot05,
                btnTestSlot06, btnTestSlot07, btnTestSlot08, btnTestSlot09, btnTestSlot10
            };

            foreach (var btn in _testSlotButtons)
            {
                btn.Text = "Empty";
                btn.Enabled = false;
                btn.Click -= TestSlotButton_Click;
                btn.Click += TestSlotButton_Click;
            }
        }

        /// <summary>
        /// 테스트 슬롯 버튼 클릭 시, 해당 버튼에 할당된 동작(Action)을 비동기적으로 실행합니다.
        /// </summary>
        private async void TestSlotButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Action action)
            {
                try
                {
                    btn.Enabled = false;

                    await Task.Run(() => action.Invoke());

                    AppendLog($"[Info] Test Slot Executed Successfully: {btn.Text}");
                }
                catch (Exception ex)
                {
                    AppendLog($"[Error] Test Slot Execution Failed: {ex.Message}");
                    MessageBox.Show($"Error: {ex.Message}", "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btn.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 선택된 칩 프로젝트의 설정에 맞게 테스트 슬롯 버튼(이름, 동작, 활성화 상태)을 갱신합니다.
        /// </summary>
        /// <param name="project">현재 선택된 칩 프로젝트입니다.</param>
        private void UpdateTestSlotButtons(IChipProject? project)
        {
            var actions = project?.GetTestSlotActions();

            bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) ||
                               (_spiBus != null && _spiBus.IsConnected);

            for (int i = 0; i < 10; i++)
            {
                var btn = _testSlotButtons[i];

                if (actions != null && i < actions.Length && actions[i] != null)
                {
                    var slotInfo = actions[i];
                    btn.Text = slotInfo.Name;
                    btn.Enabled = isConnected && slotInfo.IsEnabled;
                    btn.Visible = slotInfo.IsVisible;
                    btn.Tag = slotInfo.Action;
                }
                else
                {
                    btn.Text = $"Slot {i + 1}";
                    btn.Enabled = false;
                    btn.Tag = null;
                }
            }
        }

        /// <summary>
        /// 런타임에 사용자로부터 텍스트를 입력받기 위한 커스텀 다이얼로그 폼을 띄웁니다.
        /// </summary>
        /// <param name="title">다이얼로그 제목</param>
        /// <param name="label">입력창 상단의 라벨 텍스트</param>
        /// <param name="defaultValue">기본 입력값</param>
        /// <returns>사용자가 입력한 문자열, 취소 시 null</returns>
        public string? PromptText(string title, string label, string defaultValue)
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ClientSize = new Size(320, 120);

                lbl.AutoSize = true;
                lbl.Text = label;
                lbl.Location = new Point(9, 9);

                txt.Size = new Size(300, 23);
                txt.Location = new Point(9, 30);
                txt.Text = defaultValue;

                btnOk.Text = "OK";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Location = new Point(152, 70);

                btnCancel.Text = "Cancel";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Location = new Point(234, 70);

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                var result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                    return txt.Text;

                return null;
            }
        }

        /// <summary>
        /// 사용자가 입력한 키워드로 레지스터 트리를 검색하여 결과 목록을 표시하거나 해당 노드로 이동합니다.
        /// </summary>
        private void ShowTreeSearchDialog()
        {
            string keyword = Prompt.ShowDialog("Enter the name of the register to search for.", "Search");
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            List<TreeNode> matchedNodes = new List<TreeNode>();
            foreach (TreeNode node in tvRegTree.Nodes)
                matchedNodes.AddRange(FindAllMatchingNodes(node, keyword.ToUpper()));

            if (matchedNodes.Count == 0)
            {
                MessageBox.Show($"키워드 '{keyword}'에 해당하는 레지스터를 찾을 수 없습니다.", "검색 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (matchedNodes.Count == 1)
            {
                tvRegTree.BeginUpdate();
                ClearSelection();
                AddSelection(matchedNodes[0]);
                _pivotNode = matchedNodes[0];
                tvRegTree.EndUpdate();

                tvRegTree.SelectedNode = matchedNodes[0];
                matchedNodes[0].EnsureVisible();
                return;
            }

            // 다중 결과가 나왔을 경우 커스텀 리스트박스 다이얼로그 표시
            using (Form resultsForm = new Form())
            {
                resultsForm.Text = "Select Register";
                resultsForm.StartPosition = FormStartPosition.CenterParent;

                ListBox listBox = new ListBox()
                {
                    Font = new Font("Segoe UI", 10),
                    IntegralHeight = false,
                    Dock = DockStyle.Fill
                };

                int maxCharLen = 0;
                for (int i = 0; i < matchedNodes.Count; i++)
                {
                    string itemText = $"{(i + 1).ToString("D2")}. {matchedNodes[i].FullPath}";
                    listBox.Items.Add(itemText);
                    if (itemText.Length > maxCharLen)
                        maxCharLen = itemText.Length;
                }

                int maxPixelWidth = 0;
                using (Graphics g = tvRegTree.CreateGraphics())
                {
                    foreach (var item in listBox.Items)
                    {
                        int width = (int)g.MeasureString(item.ToString(), listBox.Font).Width;
                        if (width > maxPixelWidth)
                            maxPixelWidth = width;
                    }
                }

                int maxAllowedWidth = 900;
                listBox.Width = Math.Min(maxPixelWidth + 40, maxAllowedWidth);
                resultsForm.Width = listBox.Width + 30;

                int visibleItems = Math.Min(matchedNodes.Count, 12);
                int rowHeight = TextRenderer.MeasureText("Sample", listBox.Font).Height + 4;
                int listHeight = rowHeight * visibleItems + 20;

                int avgCharWidth = TextRenderer.MeasureText("0", listBox.Font).Width;
                int listWidth = avgCharWidth * Math.Min(maxCharLen + 4, 100);

                listBox.Height = listHeight;
                listBox.Width = listWidth;

                resultsForm.Width = listBox.Width + 30;
                resultsForm.Height = listBox.Height + 50;

                listBox.DoubleClick += (s, args) =>
                {
                    int index = listBox.SelectedIndex;
                    if (index >= 0)
                    {
                        TreeNode selected = matchedNodes[index];

                        tvRegTree.BeginUpdate();
                        ClearSelection();
                        AddSelection(selected);
                        _pivotNode = selected;
                        tvRegTree.EndUpdate();

                        tvRegTree.SelectedNode = selected;
                        selected.EnsureVisible();
                        resultsForm.Close();
                    }
                };

                resultsForm.Controls.Add(listBox);
                resultsForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 주어진 루트 노드 하위에서 키워드가 포함된 모든 트리 노드를 재귀적으로 찾습니다.
        /// </summary>
        /// <param name="root">검색을 시작할 루트 노드입니다.</param>
        /// <param name="keyword">검색할 키워드 문자열입니다.</param>
        /// <returns>일치하는 트리 노드 리스트</returns>
        private List<TreeNode> FindAllMatchingNodes(TreeNode root, string keyword)
        {
            List<TreeNode> result = new List<TreeNode>();

            if (root.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                result.Add(root);

            foreach (TreeNode child in root.Nodes)
                result.AddRange(FindAllMatchingNodes(child, keyword));

            return result;
        }

        /// <summary>
        /// 재귀적으로 트리를 탐색하여 키워드와 처음으로 일치하는 노드를 찾습니다.
        /// </summary>
        private TreeNode? FindNodeRecursive(TreeNode root, string keyword)
        {
            if (root.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                return root;

            foreach (TreeNode child in root.Nodes)
            {
                TreeNode? result = FindNodeRecursive(child, keyword);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// 현재 설정된 프로토콜(I2C/SPI), 장치, 그리고 연결 상태를 UI(Label)에 갱신합니다.
        /// </summary>
        private void UpdateStatusText()
        {
            if (_protocolSettings == null)
            {
                lblProtocolValue.Text = "(Not set)";
            }
            else
            {
                string t = _protocolSettings.ProtocolRegLogType.ToString();

                if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.I2C)
                {
                    t += $" | {_protocolSettings.SpeedKbps} kHz";
                    t += $" | 0x{_protocolSettings.I2cSlaveAddress:X2}";
                }
                else if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.SPI)
                {
                    t += $" | {_protocolSettings.SpiClockKHz} kHz";
                    t += $" | Mode {_protocolSettings.SpiMode}";
                    t += _protocolSettings.SpiLsbFirst ? " | LSB" : " | MSB";
                }
                lblProtocolValue.Text = t;
            }

            if (_ftdiSettings == null)
            {
                lblDeviceValue.Text = "(Not set)";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_ftdiSettings.Description))
                    lblDeviceValue.Text = _ftdiSettings.Description;
                else
                    lblDeviceValue.Text = $"DevIdx {_ftdiSettings.DeviceIndex}";
            }

            bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) || (_spiBus != null && _spiBus.IsConnected);
            lblStatus.Text = isConnected ? "Connected" : "Disconnected";
            lblStatus.ForeColor = isConnected ? Color.LimeGreen : Color.DarkRed;
        }

        /// <summary>
        /// 반환값이 있는 비동기 작업을 지정된 시간 내에 실행합니다. 타임아웃 발생 시 실패로 처리합니다.
        /// </summary>
        /// <typeparam name="T">반환될 결과의 타입입니다.</typeparam>
        /// <param name="action">실행할 작업(Func)입니다.</param>
        /// <param name="timeoutMs">최대 대기 시간(밀리초)입니다.</param>
        /// <returns>성공 여부와 작업 결과가 포함된 튜플</returns>
        private async Task<(bool success, T result)> RunWithTimeout<T>(Func<T> action, int timeoutMs)
        {
            var task = Task.Run(action);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            if (completed == task)
                return (true, task.Result);
            return (false, default!);
        }

        /// <summary>
        /// 반환값이 없는 비동기 작업을 지정된 시간 내에 실행합니다. 타임아웃 발생 시 실패로 처리합니다.
        /// </summary>
        /// <param name="action">실행할 작업(Action)입니다.</param>
        /// <param name="timeoutMs">최대 대기 시간(밀리초)입니다.</param>
        /// <returns>작업이 타임아웃 전에 완료되었는지 여부</returns>
        private async Task<bool> RunWithTimeout(Action action, int timeoutMs)
        {
            var task = Task.Run(action);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            return completed == task;
        }

        /// <summary>
        /// "0x" 접두사가 포함된 16진수 문자열을 안전하게 uint 값으로 변환합니다.
        /// </summary>
        /// <param name="text">변환할 16진수 문자열입니다.</param>
        /// <param name="value">변환된 uint 결과값입니다.</param>
        /// <returns>변환 성공 여부</returns>
        private bool TryParseHexUInt(string text, out uint value)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(2);

            return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// 사용자가 선택한 FTDI 장치 설명 텍스트를 분석하여 내부 DeviceKind 열거형을 매핑합니다.
        /// </summary>
        /// <param name="s">FTDI 장치 설정 객체입니다.</param>
        /// <returns>판별된 DeviceKind (UM232H, FT4222 등)</returns>
        private static DeviceKind ResolveDeviceKind(FtdiDeviceSettings s)
        {
            var desc = s.Description ?? "";
            if (desc.IndexOf("UM232H", StringComparison.OrdinalIgnoreCase) >= 0)
                return DeviceKind.UM232H;
            if (desc.IndexOf("FT232H", StringComparison.OrdinalIgnoreCase) >= 0)
                return DeviceKind.UM232H;
            return DeviceKind.FT4222;
        }

        /// <summary>
        /// 사용할 칩 프로젝트를 선택하고 UI 상태 및 슬롯 버튼을 해당 프로젝트에 맞게 변경합니다.
        /// </summary>
        /// <param name="project">선택할 IChipProject 인스턴스 (없으면 null)</param>
        private void SelectProject(IChipProject? project)
        {
            _selectedProject = project;

            if (_selectedProject != null)
            {
                lblSelectedProject.Text = _selectedProject.Name;
                UpdateTestSlotButtons(_selectedProject);
            }
            else
            {
                lblSelectedProject.Text = "(Unknown Project)";
                UpdateTestSlotButtons(null);
            }

            _protocolSettings = null;
            DisconnectBus();
            UpdateStatusText();
        }

        /// <summary>
        /// 어셈블리에 정의된 IChipProject 인터페이스 구현체들을 리플렉션을 통해 동적으로 로드합니다.
        /// </summary>
        private void LoadProjects()
        {
            _projects.Clear();

            var projectType = typeof(IChipProject);
            var asm = typeof(Oasis).Assembly; // 프로젝트 기준점(Oasis 클래스 등)이 있는 어셈블리

            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface)
                    continue;

                if (!projectType.IsAssignableFrom(t))
                    continue;

                if (Activator.CreateInstance(t) is IChipProject proj)
                {
                    _projects.Add(proj);
                }
            }

            SelectProject(null);
        }

        /// <summary>
        /// 선택된 엑셀 레지스터 맵 파일의 이름 키워드를 분석하여, 적절한 칩 프로젝트를 자동으로 매칭하고 선택합니다.
        /// </summary>
        private void AutoSelectProjectFromRegMapFile()
        {
            if (string.IsNullOrEmpty(_regMapFilePath))
                return;

            var fileName = Path.GetFileNameWithoutExtension(_regMapFilePath);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            IChipProject? selected = null;

            var tokens = fileName.Split(new[] { '_', '-', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in _projects)
            {
                foreach (var keyword in p.ProjectKeywords)
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                        continue;

                    if (string.Equals(keyword, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        selected = p;
                        break;
                    }

                    foreach (var token in tokens)
                    {
                        if (string.Equals(keyword, token, StringComparison.OrdinalIgnoreCase))
                        {
                            selected = p;
                            break;
                        }
                    }
                    if (selected != null)
                        break;

                    if (fileName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        selected = p;
                        break;
                    }
                }

                if (selected != null)
                    break;
            }

            if (selected != null)
            {
                SelectProject(selected);
                AppendLog($"[Info] Project selected: {selected.Name}");
            }
            else
            {
                SelectProject(null);
                MessageBox.Show($"파일 이름('{fileName}')에 일치하는 칩 프로젝트를 찾을 수 없습니다.\n프로젝트 키워드 설정을 확인해 주세요.",
                    "프로젝트 검색 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 현재 연결된 통신 버스(I2C/SPI)를 안전하게 해제하고 관련된 테스트, UI 상태를 초기화합니다.
        /// </summary>
        private void DisconnectBus()
        {
            try
            {
                _testCts?.Cancel();
            }
            catch { }
            _testCts = null;
            _isRunningTest = false;

            try
            {
                _i2cBus?.Disconnect();
            }
            catch { }
            try
            {
                _spiBus?.Disconnect();
            }
            catch { }

            _i2cBus = null;
            _spiBus = null;
            _chip = null;

            _testSuite = null;

            cmbTestCategory.Items.Clear();
            cmbTest.Items.Clear();
            btnRunTest.Enabled = false;
            btnStopTest.Enabled = false;
            rtbRunTestLog.Clear();

            btnConnect.Text = "Connect";
            UpdateStatusText();
            UpdateConnectionControls();

            if (_selectedProject != null)
            {
                UpdateTestSlotButtons(_selectedProject);
            }
        }

        /// <summary>
        /// 설정된 프로토콜 및 장치 정보를 바탕으로 I2C 또는 SPI 버스에 연결을 시도합니다.
        /// </summary>
        private void TryConnect()
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("선택된 프로젝트가 없습니다.\n연결할 프로젝트를 먼저 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DisconnectBus();

            if (_ftdiSettings == null)
            {
                MessageBox.Show("FTDI 장치가 설정되지 않았습니다.\n연결하려면 상단의 [Device Setup]을 진행해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_protocolSettings == null)
            {
                MessageBox.Show("프로토콜 설정이 누락되었습니다.\n연결하려면 상단의[Protocol Setup]을 진행해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _protocolSettings.DeviceKind = ResolveDeviceKind(_ftdiSettings);
            uint devIndex = (uint)_ftdiSettings.DeviceIndex;

            try
            {
                // 특정 칩(Chicago)의 예외 케이스 처리
                _protocolSettings.ForceIdleHighOnConnect = (_selectedProject is Chicago);
                if (_selectedProject is Chicago)
                    _protocolSettings.DeviceKind = DeviceKind.UM232H;

                if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.I2C)
                {
                    if (_selectedProject is not II2cChipProject i2cProj)
                    {
                        MessageBox.Show("선택한 프로젝트는 I2C 통신을 지원하지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _i2cBus = new I2cBus(devIndex, _protocolSettings);
                    if (!_i2cBus.Connect())
                    {
                        _i2cBus = null;
                        MessageBox.Show("I2C 하드웨어 연결에 실패했습니다. 장치 연결 상태를 확인해 주세요.", "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatusText();
                        return;
                    }

                    _chip = i2cProj.CreateChip(_i2cBus, _protocolSettings);
                }
                else if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.SPI)
                {
                    if (_selectedProject is not ISpiChipProject spiProj)
                    {
                        MessageBox.Show("선택한 프로젝트는 SPI 통신을 지원하지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _spiBus = new SpiBus(devIndex, _protocolSettings);
                    if (!_spiBus.Connect())
                    {
                        _spiBus = null;
                        MessageBox.Show("SPI 하드웨어 연결에 실패했습니다. 장치 연결 상태를 확인해 주세요.", "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatusText();
                        return;
                    }

                    _chip = spiProj.CreateChip(_spiBus, _protocolSettings);
                }
                else
                {
                    MessageBox.Show("지원하지 않는 프로토콜입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadTestSuiteIfAny();

                if (_chip is IChipProject activeChipProject)
                {
                    UpdateTestSlotButtons(activeChipProject);
                }

                btnConnect.Text = "Disconnect";
                UpdateStatusText();
                UpdateConnectionControls();
            }
            catch (Exception ex)
            {
                DisconnectBus();
                MessageBox.Show($"연결 중 오류가 발생했습니다:\n{ex.Message}", "연결 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 연결 상태에 따라 레지스터 제어 버튼, 설정 버튼 등의 활성화 여부를 동적으로 갱신합니다.
        /// </summary>
        private void UpdateConnectionControls()
        {
            bool isConnected =
                (_i2cBus != null && _i2cBus.IsConnected) ||
                (_spiBus != null && _spiBus.IsConnected);

            btnProtocolSetup.Enabled = !isConnected;
            btnDeviceSetup.Enabled = !isConnected;

            foreach (var btn in grpRegControl.Controls.OfType<Button>())
                btn.Enabled = isConnected;

            foreach (var b in _bitButtons)
                if (b != null)
                    b.Enabled = isConnected;

            txtRegValueHex.Enabled = isConnected;

            btnSaveScript.Enabled = isConnected;
            btnLoadScript.Enabled = isConnected;

            SetBitButtonsEnabledForItem(_selectedItem);
            UpdateNumRegIndexForSelectedItem();

            bool hasTests = _testSuite != null && _testSuite.Tests != null && _testSuite.Tests.Count > 0;
            cmbTestCategory.Enabled = isConnected && hasTests;
            cmbTest.Enabled = isConnected && hasTests;
        }

        /// <summary>
        /// 프로젝트가 자동화 테스트(IChipTestSuite)를 지원하는 경우 관련 테스트 정보를 로드하고 콤보박스에 바인딩합니다.
        /// </summary>
        private void LoadTestSuiteIfAny()
        {
            _testSuite = null;
            cmbTestCategory.Items.Clear();
            cmbTest.Items.Clear();
            btnRunTest.Enabled = false;
            btnStopTest.Enabled = false;
            rtbRunTestLog.Clear();

            if (_chip == null || _selectedProject == null)
                return;

            if (_selectedProject is not IChipProjectWithTests projWithTests)
                return;

            _testSuite = projWithTests.CreateTestSuite(_chip);
            if (_testSuite == null || _testSuite.Tests == null || _testSuite.Tests.Count == 0)
                return;

            var categories = _testSuite.Tests
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (var c in categories)
                cmbTestCategory.Items.Add(c);

            if (cmbTestCategory.Items.Count > 0)
            {
                cmbTestCategory.SelectedIndex = 0;
                _prevTestCategoryIndex = 0;
            }
        }

        /// <summary>
        /// 하드웨어 연결 / 연결 해제 토글 버튼 클릭 이벤트 핸들러입니다.
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_isRunningTest)
            {
                MessageBox.Show("현재 테스트가 실행 중입니다.\n연결을 끊기 전에 먼저 테스트를 중지해 주세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) || (_spiBus != null && _spiBus.IsConnected);

            if (!isConnected)
                TryConnect();
            else
                DisconnectBus();
        }

        /// <summary>
        /// FTDI 장치 설정 창을 엽니다.
        /// </summary>
        private void btnFtdiSetup_Click(object sender, EventArgs e)
        {
            using (var dlg = new FtdiSetupForm(_ftdiSettings))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _ftdiSettings = dlg.Result;
                    UpdateStatusText();
                }
            }
        }

        /// <summary>
        /// 프로토콜(I2C/SPI 등) 설정 창을 엽니다.
        /// </summary>
        private void btnProtocolSetup_Click(object sender, EventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("선택된 프로젝트가 없습니다.\n연결할 프로젝트를 먼저 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new ProtocolSetupForm(_selectedProject, _protocolSettings))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _protocolSettings = dlg.Result;
                    UpdateStatusText();
                }
            }
        }

        /// <summary>
        /// 레지스터 32비트 토글 버튼 중 하나가 클릭되었을 때 값을 반전(0 <-> 1)시킵니다.
        /// </summary>
        private void BitButton_Click(object sender, EventArgs e)
        {
            if (_isUpdatingBits)
                return;

            if (sender is not Button btn)
                return;

            btn.Text = (btn.Text == "0") ? "1" : "0";
            UpdateBitButtonVisual(btn);

            _currentRegValue = GetValueFromBitButtons();
            UpdateBitCurrentValues();
        }

        /// <summary>
        /// 전달된 uint 값을 파싱하여 32개의 비트 버튼 텍스트(0 또는 1)에 반영합니다.
        /// </summary>
        /// <param name="value">적용할 32비트 값입니다.</param>
        private void UpdateBitButtonsFromValue(uint value)
        {
            _isUpdatingBits = true;

            for (int bit = 0; bit < 32; bit++)
            {
                int btnIndex = 31 - bit;
                uint mask = 1u << bit;
                bool isOne = (value & mask) != 0;

                var btn = _bitButtons[btnIndex];
                if (btn != null)
                {
                    btn.Text = isOne ? "1" : "0";
                    UpdateBitButtonVisual(btn);
                }
            }

            _isUpdatingBits = false;
        }

        /// <summary>
        /// 현재 UI에 표시된 32개의 비트 버튼 상태를 취합하여 하나의 uint 값을 반환합니다.
        /// </summary>
        /// <returns>취합된 32비트 레지스터 값</returns>
        private uint GetValueFromBitButtons()
        {
            uint value = 0;

            for (int btnIndex = 0; btnIndex < 32; btnIndex++)
            {
                var btn = _bitButtons[btnIndex];
                if (btn == null)
                    continue;

                int bit = 31 - btnIndex;
                if (btn.Text == "1")
                    value |= (1u << bit);
            }

            return value;
        }

        /// <summary>
        /// 레지스터 헥사(Hex) 텍스트박스에서 포커스가 벗어났을 때, 입력값을 검증하고 반영합니다.
        /// </summary>
        private void txtRegValueHex_Leave(object sender, EventArgs e)
        {
            if (TryParseHexUInt(txtRegValueHex.Text, out uint v))
            {
                _currentRegValue = v;
                UpdateBitCurrentValues();
            }
            else
            {
                MessageBox.Show("잘못된 레지스터 값 형식입니다.\n올바른 16진수 형태로 입력해 주세요. (예: 0x00000000)", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                int hexLength = (_selectedRegister != null) ? (_selectedRegister.BitWidth / 4) : 8;
                txtRegValueHex.Text = $"0x{_currentRegValue.ToString("X" + hexLength)}";
            }
        }

        /// <summary>
        /// 헥사(Hex) 입력 텍스트박스에서 Enter 키를 누를 때 입력을 확정합니다.
        /// </summary>
        private void TxtRegValueHex_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtRegValueHex_Leave(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 레지스터 맵(엑셀) 파일을 선택하고, 자동으로 프로젝트를 매칭하며, 엑셀 창을 스냅 분할(Aero Snap)하여 엽니다.
        /// </summary>
        private async void btnSelectMapFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                try
                {
                    ofd.Filter = "Excel Files|*.xlsx;*.xlsm;*.xls";
                    ofd.Title = "Select RegisterMap Excel";

                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;

                    _regMapFilePath = ofd.FileName;
                    lblMapFileName.Text = Path.GetFileName(_regMapFilePath);
                    btnOpenMapPath.Enabled = true;
                    clbRegMapSheets.Items.Clear();

                    AutoSelectProjectFromRegMapFile();

                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "excel.exe",
                            Arguments = $"/x /r \"{_regMapFilePath}\"",
                            UseShellExecute = true
                        };

                        Process? excelProcess = Process.Start(psi);
                        if (excelProcess != null)
                        {
                            _openedExcelProcesses.Add(excelProcess);

                            if (chkAutoArrange.Checked)
                            {
                                ArrangeWindowsSideBySide(excelProcess);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(_regMapFilePath) { UseShellExecute = true });
                        }
                        catch { }
                    }

                    var sheetNames = ExcelReader.GetSheetNames(_regMapFilePath);
                    foreach (var name in sheetNames)
                        clbRegMapSheets.Items.Add(name, false);

                    _groups.Clear();
                    _regValues.Clear();
                    tvRegTree.Nodes.Clear();
                }
                catch (Exception ex)
                {
                    _regMapFilePath = null;
                    lblMapFileName.Text = "(No file)";
                    btnOpenMapPath.Enabled = false;
                    MessageBox.Show($"엑셀 파일을 여는 중 오류가 발생했습니다:\n{ex.Message}", "엑셀 열기 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 엑셀에서 선택된 시트(체크박스)들을 파싱하여 레지스터 트리뷰(TreeView)에 노드로 구성합니다.
        /// </summary>
        private void btnLoadSelectedSheets_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_regMapFilePath))
            {
                MessageBox.Show("레지스터 맵 파일이 선택되지 않았습니다.\n먼저 엑셀 파일을 열어 시트를 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (clbRegMapSheets.CheckedItems.Count == 0)
            {
                MessageBox.Show("선택된 시트가 없습니다.\n트리에 로드할 시트를 목록에서 체크해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _groups.Clear();
                _regValues.Clear();

                foreach (var item in clbRegMapSheets.CheckedItems)
                {
                    var sheetName = item?.ToString();
                    if (string.IsNullOrWhiteSpace(sheetName))
                        continue;

                    var data = ExcelReader.ReadUsedRangeAsStringArray(_regMapFilePath, sheetName);

                    var group = RegisterMapParser.MakeRegisterGroup(sheetName, data);
                    _groups.Add(group);
                }

                _regMgr = new RegisterFieldManager(() => _groups);

                BuildRegisterTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"레지스터 맵을 로드하는 중 오류가 발생했습니다:\n{ex.Message}", "로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 현재 선택된 엑셀 레지스터 맵 파일이 위치한 폴더를 Windows 탐색기로 엽니다.
        /// </summary>
        private void btnOpenMapPath_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_regMapFilePath) || !File.Exists(_regMapFilePath))
            {
                MessageBox.Show("현재 열려 있는 레지스터 맵 파일이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var arg = $"/select,\"{_regMapFilePath}\"";
                Process.Start("explorer.exe", arg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("경로를 여는 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        /// <summary>
        /// 파싱된 레지스터 그룹 데이터를 사용하여 트리뷰 컨트롤(UI) 노드를 구성합니다.
        /// </summary>
        private void BuildRegisterTree()
        {
            tvRegTree.Nodes.Clear();

            foreach (var g in _groups)
            {
                var groupNode = new TreeNode(g.Name)
                {
                    Tag = g
                };

                foreach (var reg in g.Registers)
                {
                    var regNode = new TreeNode($"{reg.Name} (0x{reg.Address:X8})")
                    {
                        Tag = reg
                    };

                    uint regVal = GetRegisterValue(reg);

                    foreach (var item in reg.Items)
                    {
                        var itemNode = new TreeNode(FormatItemNodeText(item, regVal))
                        {
                            Tag = item
                        };

                        regNode.Nodes.Add(itemNode);

                        if (!_registerNodeCache.ContainsKey(reg))
                            _registerNodeCache[reg] = new List<TreeNode>();
                        _registerNodeCache[reg].Add(itemNode);
                    }

                    groupNode.Nodes.Add(regNode);
                }

                tvRegTree.Nodes.Add(groupNode);
            }

            tvRegTree.BeginUpdate();

            foreach (TreeNode sheetNode in tvRegTree.Nodes)
            {
                sheetNode.Expand();

                foreach (TreeNode child in sheetNode.Nodes)
                    child.Collapse();
            }

            tvRegTree.EndUpdate();
        }

        /// <summary>
        /// 단일 레지스터 항목(비트 필드) 노드에 표시할 텍스트를 포맷팅하여 생성합니다.
        /// </summary>
        /// <param name="item">레지스터 항목 정보</param>
        /// <param name="regValue">해당 레지스터의 전체 32비트 값</param>
        /// <returns>트리에 표시할 포맷된 문자열</returns>
        private static string FormatItemNodeText(RegisterItem item, uint regValue)
        {
            string bitText = item.UpperBit == item.LowerBit
                ? item.UpperBit.ToString()
                : $"{item.UpperBit}:{item.LowerBit}";

            int width = item.UpperBit - item.LowerBit + 1;
            uint mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
            uint fieldVal = (regValue >> item.LowerBit) & mask;

            return $"[{bitText}] {item.Name} = {fieldVal} (0x{fieldVal:X})";
        }

        /// <summary>
        /// 캐시된 레지스터 값을 반환하거나, 없다면 리셋 기본값을 반환합니다.
        /// </summary>
        /// <param name="reg">값을 조회할 레지스터</param>
        /// <returns>현재 저장된 레지스터 값</returns>
        private uint GetRegisterValue(RegisterDetail reg)
        {
            if (_regValues.TryGetValue(reg, out var v))
                return v;

            v = reg.ResetValue;
            _regValues[reg] = v;
            return v;
        }

        /// <summary>
        /// 레지스터 트리뷰에서 특정 노드가 선택되었을 때 발생하는 이벤트 핸들러입니다.
        /// </summary>
        private void tvRegs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByKeyboard && e.Node != null)
            {
                tvRegTree.BeginUpdate();
                ClearSelection();
                AddSelection(e.Node);
                _pivotNode = e.Node;
                tvRegTree.EndUpdate();
            }

            ResetSelectionState();

            if (e.Node?.Tag is RegisterGroup group)
            {
                HandleGroupSelection(group);
            }
            else if (e.Node?.Tag is RegisterDetail register)
            {
                HandleRegisterSelection(register, e.Node.Parent?.Tag as RegisterGroup);
            }
            else if (e.Node?.Tag is RegisterItem item)
            {
                HandleItemSelection(item, e.Node?.Parent?.Tag as RegisterDetail, e.Node?.Parent?.Parent?.Tag as RegisterGroup);
            }
            else
            {
                ShowNoSelectionState();
            }

            if (_chip is ProjectBase projBase)
            {
                projBase.CurrentSheetName = _selectedGroup?.Name ?? string.Empty;
                Debug.WriteLine($"Active Sheet Changed to: {projBase.CurrentSheetName}");
            }
        }

        /// <summary>
        /// 트리 노드 선택 시 내부 상태 변수를 초기화합니다.
        /// </summary>
        private void ResetSelectionState()
        {
            _selectedGroup = null;
            _selectedRegister = null;
            _selectedItem = null;
        }

        /// <summary>
        /// 그룹(시트) 노드를 선택했을 때 UI 상태를 처리합니다.
        /// </summary>
        private void HandleGroupSelection(RegisterGroup group)
        {
            _selectedGroup = group;

            lblRegName.Text = "(Group Selected)";
            lblRegAddrSummary.Text = "Address: -";
            lblRegResetSummary.Text = "Reset Value: -";

            _currentRegValue = 0;
            UpdateBitCurrentValues();

            SetBitButtonsEnabledForItem(null);
            UpdateNumRegIndexForSelectedItem();
        }

        /// <summary>
        /// 레지스터 노드를 선택했을 때 UI 상태를 처리합니다.
        /// </summary>
        private void HandleRegisterSelection(RegisterDetail register, RegisterGroup? parentGroup)
        {
            _selectedGroup = parentGroup;
            LoadRegisterToUi(register, null);
        }

        /// <summary>
        /// 아이템(비트 필드) 노드를 선택했을 때 UI 상태를 처리합니다.
        /// </summary>
        private void HandleItemSelection(RegisterItem item, RegisterDetail? parentRegister, RegisterGroup? parentGroup)
        {
            _selectedGroup = parentGroup;
            if (parentRegister != null)
                LoadRegisterToUi(parentRegister, item);
            else
                ShowNoSelectionState();
        }

        /// <summary>
        /// 선택된 노드가 없을 때의 빈 UI 상태를 표시합니다.
        /// </summary>
        private void ShowNoSelectionState()
        {
            lblRegName.Text = "(No Register)";
            lblRegAddrSummary.Text = "Address: -";
            lblRegResetSummary.Text = "Reset Value: -";

            _currentRegValue = 0;
            UpdateBitCurrentValues();

            SetBitButtonsEnabledForItem(null);
            UpdateNumRegIndexForSelectedItem();
        }

        /// <summary>
        /// 선택된 레지스터의 상세 정보를 텍스트박스와 버튼 UI에 로드하여 표시합니다.
        /// </summary>
        private void LoadRegisterToUi(RegisterDetail register, RegisterItem? selectedItem)
        {
            _selectedRegister = register;
            _selectedItem = selectedItem;

            lblRegName.Text = register.Name;
            lblRegAddrSummary.Text = $"Address: 0x{register.Address:X8}";
            lblRegResetSummary.Text = $"Reset Value: 0x{register.ResetValue:X8}";

            _currentRegValue = GetRegisterValue(register);
            UpdateBitCurrentValues();

            SetBitButtonsEnabledForItem(selectedItem);
            UpdateNumRegIndexForSelectedItem();
        }

        /// <summary>
        /// 선택된 레지스터 아이템(비트 필드)의 범위에 해당하는 비트 버튼만 활성화합니다.
        /// </summary>
        /// <param name="item">선택된 레지스터 세부 항목 (null일 경우 전부 비활성화)</param>
        private void SetBitButtonsEnabledForItem(RegisterItem? item)
        {
            bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) ||
                               (_spiBus != null && _spiBus.IsConnected);

            if (item == null || !isConnected)
            {
                for (int i = 0; i < _bitButtons.Length; i++)
                {
                    var btn = _bitButtons[i];
                    if (btn != null)
                    {
                        btn.Enabled = false;
                        UpdateBitButtonVisual(btn);
                    }
                }
                return;
            }

            for (int bit = 0; bit < 32; bit++)
            {
                int btnIndex = 31 - bit;
                var btn = _bitButtons[btnIndex];
                if (btn == null)
                    continue;

                bool inRange = bit >= item.LowerBit && bit <= item.UpperBit;
                btn.Enabled = inRange;
                UpdateBitButtonVisual(btn);
            }
        }

        /// <summary>
        /// _currentRegValue 변수 값에 맞춰 모든 UI 컨트롤(텍스트박스, 버튼, 트리 노드)을 동기화합니다.
        /// </summary>
        private void UpdateBitCurrentValues()
        {
            int hexLength = (_selectedRegister != null) ? (_selectedRegister.BitWidth / 4) : 8;

            txtRegValueHex.Text = $"0x{_currentRegValue:X8}";

            if (_selectedRegister != null)
                _regValues[_selectedRegister] = _currentRegValue;

            UpdateBitButtonsFromValue(_currentRegValue);
            UpdateNumRegIndexForSelectedItem();

            if (_selectedRegister != null)
                UpdateTreeNodesForRegister(_selectedRegister, _currentRegValue);
        }

        /// <summary>
        /// 레지스터 값이 변경되었을 때, 하위 아이템(비트 필드) 노드들의 표시 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateTreeNodesForRegister(RegisterDetail reg, uint regValue)
        {
            if (_registerNodeCache.TryGetValue(reg, out var itemNodes))
            {
                foreach (var node in itemNodes)
                {
                    if (node.Tag is RegisterItem item)
                    {
                        node.Text = FormatItemNodeText(item, regValue);
                    }
                }
            }
        }

        /// <summary>
        /// 트리에 표시된 모든 레지스터의 값을 최신 상태로 강제 갱신합니다. (ReadAll 등의 작업 후 호출)
        /// </summary>
        private void RefreshRegisterTreeValues()
        {
            if (tvRegTree.Nodes.Count == 0)
                return;

            tvRegTree.BeginUpdate();
            try
            {
                foreach (TreeNode groupNode in tvRegTree.Nodes)
                {
                    if (groupNode.Tag is not RegisterGroup g)
                        continue;

                    foreach (TreeNode regNode in groupNode.Nodes)
                    {
                        if (regNode.Tag is not RegisterDetail reg)
                            continue;

                        uint regVal = GetRegisterValue(reg);

                        foreach (TreeNode itemNode in regNode.Nodes)
                        {
                            if (itemNode.Tag is not RegisterItem item)
                                continue;

                            itemNode.Text = FormatItemNodeText(item, regVal);
                        }
                    }
                }
            }
            finally
            {
                tvRegTree.EndUpdate();
            }
        }

        /// <summary>
        /// 선택된 아이템(비트 필드)의 값 범위에 맞게 NumUp/Down 컨트롤(스피너)의 최소/최대값과 현재 값을 갱신합니다.
        /// </summary>
        private void UpdateNumRegIndexForSelectedItem()
        {
            _isUpdatingRegValue = true;
            try
            {
                bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) ||
                                   (_spiBus != null && _spiBus.IsConnected);

                if (_selectedItem == null || !isConnected)
                {
                    numRegIndex.Enabled = false;
                    numRegIndex.Minimum = 0;
                    numRegIndex.Maximum = 0;
                    numRegIndex.Value = 0;
                    return;
                }

                int width = _selectedItem.UpperBit - _selectedItem.LowerBit + 1;
                uint mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                uint fieldVal = (_currentRegValue >> _selectedItem.LowerBit) & mask;

                numRegIndex.Minimum = 0;
                numRegIndex.Maximum = mask;
                numRegIndex.Enabled = true;

                numRegIndex.Value = fieldVal <= mask ? fieldVal : mask;
            }
            finally
            {
                _isUpdatingRegValue = false;
            }
        }

        /// <summary>
        /// NumUp/Down 컨트롤의 값이 변경되었을 때 레지스터의 특정 비트 필드 값을 계산하여 반영합니다.
        /// </summary>
        private void numRegIndex_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingRegValue)
                return;

            if (_selectedItem == null)
                return;

            uint fieldVal = (uint)numRegIndex.Value;

            int width = _selectedItem.UpperBit - _selectedItem.LowerBit + 1;
            uint mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);

            if (fieldVal > mask)
                fieldVal = mask;

            uint regVal = _currentRegValue;
            uint fieldMask = mask << _selectedItem.LowerBit;

            regVal &= ~fieldMask;
            regVal |= (fieldVal << _selectedItem.LowerBit);

            _currentRegValue = regVal;
            UpdateBitCurrentValues();
        }

        /// <summary>
        /// 레거시 스크립트 형식으로 현재 레지스터 설정값을 텍스트 파일로 저장합니다.
        /// </summary>
        private void SaveRegisterScriptLegacy(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                foreach (var group in _groups)
                {
                    sw.WriteLine(group.Name);

                    foreach (var reg in group.Registers)
                    {
                        uint value = GetRegisterValue(reg);
                        sw.WriteLine($"\t{reg.Address:X8}\t{value:X8}\t{reg.Name}");

                        foreach (var item in reg.Items)
                        {
                            int width = item.UpperBit - item.LowerBit + 1;
                            uint mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                            uint fieldVal = (value >> item.LowerBit) & mask;

                            string bitText = $"[{item.UpperBit}:{item.LowerBit}]";
                            sw.WriteLine($"\t\t{bitText}{item.Name}\t{fieldVal}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 저장된 텍스트 스크립트 파일을 읽어와 레지스터 값 설정 딕셔너리에 반영합니다. (하드웨어에 직접 Write하지 않음)
        /// </summary>
        private void LoadRegisterScriptLegacy(string path)
        {
            var addrToReg = new Dictionary<uint, RegisterDetail>();
            foreach (var g in _groups)
            {
                foreach (var reg in g.Registers)
                    addrToReg[reg.Address] = reg;
            }

            foreach (var raw in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                string line = raw.Trim();
                if (line.StartsWith("["))
                    continue;

                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2 &&
                    TryParseHexUInt(parts[0], out uint addr) &&
                    TryParseHexUInt(parts[1], out uint value))
                {
                    if (addrToReg.TryGetValue(addr, out var reg))
                        _regValues[reg] = value;
                }
            }

            if (_selectedRegister != null)
            {
                _currentRegValue = GetRegisterValue(_selectedRegister);
                UpdateBitCurrentValues();
            }

            RefreshRegisterTreeValues();
        }

        /// <summary>
        /// 'Save Script' 버튼 클릭 시 스크립트 저장 다이얼로그를 엽니다.
        /// </summary>
        private void btnSaveScript_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Register Script|*.txt|All Files|*.*";

                if (!string.IsNullOrEmpty(_scriptFilePath))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(_scriptFilePath);
                    sfd.FileName = Path.GetFileName(_scriptFilePath);
                }

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                SaveRegisterScriptLegacy(sfd.FileName);
                SetScriptFilePath(sfd.FileName);
            }
        }

        /// <summary>
        /// 'Load Script' 버튼 클릭 시 스크립트 열기 다이얼로그를 엽니다.
        /// </summary>
        private void btnLoadScript_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Register Script|*.txt|All Files|*.*";

                if (!string.IsNullOrEmpty(_scriptFilePath))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(_scriptFilePath);
                    ofd.FileName = Path.GetFileName(_scriptFilePath);
                }

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                LoadRegisterScriptLegacy(ofd.FileName);
                SetScriptFilePath(ofd.FileName);

                if (_selectedRegister != null)
                {
                    _currentRegValue = GetRegisterValue(_selectedRegister);
                    UpdateBitCurrentValues();
                }
            }
        }

        /// <summary>
        /// 활성화된 스크립트 파일의 경로를 갱신하고 UI 텍스트를 변경합니다.
        /// </summary>
        private void SetScriptFilePath(string path)
        {
            _scriptFilePath = path;

            if (string.IsNullOrEmpty(path))
            {
                lblScriptFileName.Text = "(No script)";
                btnOpenScriptPath.Enabled = false;
            }
            else
            {
                lblScriptFileName.Text = Path.GetFileName(path);
                btnOpenScriptPath.Enabled = true;
            }
        }

        /// <summary>
        /// 로드된 스크립트 파일이 위치한 폴더를 Windows 탐색기로 엽니다.
        /// </summary>
        private void btnOpenScriptPath_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_scriptFilePath) || !File.Exists(_scriptFilePath))
            {
                MessageBox.Show("현재 열려 있는 스크립트 파일이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var arg = $"/select,\"{_scriptFilePath}\"";
            Process.Start("explorer.exe", arg);
        }

        /// <summary>
        /// 현재 트리에서 선택되거나 체크된 레지스터들의 값을 타겟 하드웨어 칩에서 비동기로 읽어옵니다(Read).
        /// </summary>
        private async void btnRead_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("하드웨어가 연결되지 않았습니다.\n읽기 작업을 수행하려면 먼저 장치를 연결해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var targets = GetSelectedRegisters();

            if (targets.Count == 0 && _selectedRegister != null)
            {
                targets.Add(_selectedRegister);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("읽어올 레지스터를 트리에서 선택하거나 다중 체크해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var reg in targets)
            {
                uint addr = reg.Address;

                try
                {
                    var result = await RunWithTimeout(() => _chip.ReadRegister(addr), I2cTimeoutMs);

                    if (!result.success)
                    {
                        AddLog("READ", $"0x{addr:X8}", "", "TIMEOUT");
                        continue;
                    }

                    uint data = result.result;

                    _regValues[reg] = data;

                    if (reg == _selectedRegister)
                    {
                        _currentRegValue = data;
                    }

                    AddLog("READ", $"0x{addr:X8}", $"0x{data:X8}", "OK");
                }
                catch (Exception ex)
                {
                    AddLog("READ", $"0x{addr:X8}", "", "ERR");
                    Debug.WriteLine(ex);
                }
            }

            if (_selectedRegister != null && targets.Contains(_selectedRegister))
            {
                UpdateBitCurrentValues();
            }

            RefreshRegisterTreeValues();
        }

        /// <summary>
        /// 현재 트리에서 선택되거나 텍스트박스에 입력된 값을 타겟 하드웨어 칩에 비동기로 기록합니다(Write).
        /// </summary>
        private async void btnWrite_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("하드웨어가 연결되지 않았습니다.\n쓰기 작업을 수행하려면 먼저 장치를 연결해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var targets = GetSelectedRegisters();

            if (targets.Count == 0 && _selectedRegister != null)
            {
                targets.Add(_selectedRegister);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("읽어올 레지스터를 트리에서 선택하거나 다중 체크해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var reg in targets)
            {
                uint addr = reg.Address;
                uint data = GetRegisterValue(reg);

                // 현재 단일 선택 중인 레지스터라면, UI에 있는 헥사 텍스트박스 값을 우선 적용
                if (reg == _selectedRegister && TryParseHexUInt(txtRegValueHex.Text, out uint txtVal))
                {
                    data = txtVal;
                }

                try
                {
                    bool success = await RunWithTimeout(() =>
                    {
                        _chip.WriteRegister(addr, data);
                    }, I2cTimeoutMs);

                    if (!success)
                    {
                        AddLog("WRITE", $"0x{addr:X8}", $"0x{data:X8}", "TIMEOUT");
                        failCount++;
                    }
                    else
                    {
                        _regValues[reg] = data;
                        _currentRegValue = data;
                        AddLog("WRITE", $"0x{addr:X8}", $"0x{data:X8}", "OK");
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    AddLog("WRITE", $"0x{addr:X8}", $"0x{data:X8}", "ERR");
                    failCount++;
                    Debug.WriteLine(ex);
                }
            }

            if (_selectedRegister != null && targets.Contains(_selectedRegister))
            {
                UpdateBitCurrentValues();
            }
        }

        /// <summary>
        /// 트리에 로드된 모든 레지스터의 설정된 값을 타겟 하드웨어 칩에 일괄 기록합니다(Write All).
        /// </summary>
        private async void btnWriteAll_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("하드웨어가 연결되지 않았습니다.\n쓰기 작업을 수행하려면 먼저 장치를 연결해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_groups.Count == 0)
            {
                MessageBox.Show("레지스터 트리가 비어 있습니다.\n작업을 수행할 시트를 먼저 로드해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var group in _groups)
            {
                foreach (var reg in group.Registers)
                {
                    uint addr = reg.Address;
                    uint data = GetRegisterValue(reg);

                    try
                    {
                        bool success = await RunWithTimeout(() => _chip.WriteRegister(addr, data), I2cTimeoutMs);
                        if (!success)
                        {
                            AddLog("WRITE_ALL", $"0x{addr:X8}", $"0x{data:X8}", "TIMEOUT");
                            continue;
                        }

                        _regValues[reg] = data;
                        AddLog("WRITE_ALL", $"0x{addr:X8}", $"0x{data:X8}", "OK");
                    }
                    catch (Exception ex)
                    {
                        AddLog("WRITE_ALL", $"0x{addr:X8}", $"0x{data:X8}", "ERR");
                        Debug.WriteLine(ex);
                    }
                }
            }

            if (_selectedRegister != null)
            {
                _currentRegValue = GetRegisterValue(_selectedRegister);
                UpdateBitCurrentValues();
            }

            RefreshRegisterTreeValues();
        }

        /// <summary>
        /// 트리에 로드된 모든 레지스터의 값을 타겟 하드웨어 칩에서 일괄로 읽어옵니다(Read All).
        /// </summary>
        private async void btnReadAll_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("하드웨어가 연결되지 않았습니다.\n읽기 작업을 수행하려면 먼저 장치를 연결해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_groups.Count == 0)
            {
                MessageBox.Show("레지스터 트리가 비어 있습니다.\n작업을 수행할 시트를 먼저 로드해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var group in _groups)
            {
                foreach (var reg in group.Registers)
                {
                    uint addr = reg.Address;

                    try
                    {
                        var result = await RunWithTimeout(() => _chip.ReadRegister(addr), I2cTimeoutMs);
                        if (!result.success)
                        {
                            AddLog("READ_ALL", $"0x{addr:X8}", "", "TIMEOUT");
                            continue;
                        }

                        uint data = result.result;
                        _regValues[reg] = data;
                        AddLog("READ_ALL", $"0x{addr:X8}", $"0x{data:X8}", "OK");
                    }
                    catch (Exception ex)
                    {
                        AddLog("READ_ALL", $"0x{addr:X8}", "", "ERR");
                        Debug.WriteLine(ex);
                    }
                }
            }

            if (_selectedRegister != null)
            {
                _currentRegValue = GetRegisterValue(_selectedRegister);
                UpdateBitCurrentValues();
            }

            RefreshRegisterTreeValues();
        }

        /// <summary>
        /// 테스트 카테고리 콤보박스의 선택 항목이 변경될 때, 해당 카테고리에 속한 테스트 목록을 콤보박스에 바인딩합니다.
        /// </summary>
        private void comboTestCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressTestCategoryEvent)
                return;

            if (_isRunningTest)
            {
                _suppressTestCategoryEvent = true;
                try
                {
                    if (_prevTestCategoryIndex >= 0 &&
                        _prevTestCategoryIndex < cmbTestCategory.Items.Count)
                    {
                        cmbTestCategory.SelectedIndex = _prevTestCategoryIndex;
                    }
                }
                finally
                {
                    _suppressTestCategoryEvent = false;
                }
                return;
            }

            cmbTest.Items.Clear();

            if (_testSuite == null)
                return;

            if (cmbTestCategory.SelectedItem is not string category)
                return;

            var testsInCategory = _testSuite.Tests
                .Where(t => string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var t in testsInCategory)
                cmbTest.Items.Add(t);

            cmbTest.DisplayMember = "Name";

            if (cmbTest.Items.Count > 0)
                cmbTest.SelectedIndex = 0;

            btnRunTest.Enabled = cmbTest.Items.Count > 0;
            cmbTestCategory.Enabled = cmbTest.Items.Count > 0;
            cmbTest.Enabled = cmbTest.Items.Count > 0;
            btnStopTest.Enabled = false;

            _prevTestCategoryIndex = cmbTestCategory.SelectedIndex;
        }

        /// <summary>
        /// 'Run Test' 버튼 클릭 시, 선택된 자동화 테스트를 비동기(Task.Run)로 실행합니다.
        /// </summary>
        private async void btnRunTest_Click(object sender, EventArgs e)
        {
            if (_testSuite == null || _isRunningTest)
                return;
            if (cmbTest.SelectedItem is not ChipTestInfo info)
                return;

            if (!_testSuite.PrepareTest(info.Id, this))
            {
                AppendLog("[Info] Test cancelled by user.");
                return;
            }

            _testCts = new CancellationTokenSource();
            _isRunningTest = true;
            cmbTestCategory.Enabled = false;
            cmbTest.Enabled = false;

            btnRunTest.Enabled = false;
            btnStopTest.Enabled = true;
            rtbRunTestLog.Clear();
            UpdateTestProgress(0);

            try
            {
                AddTestLogRow("START", $"Test Started → {info.Id}");

                await Task.Run(async () =>
                {
                    await _testSuite.Run_TEST(
                        info.Id,
                        (level, message) =>
                        {
                            if (IsDisposed)
                                return Task.CompletedTask;

                            if (InvokeRequired)
                                BeginInvoke(new Action(() => AddTestLogRow(level, message)));
                            else
                                AddTestLogRow(level, message);

                            return Task.CompletedTask;
                        },
                        _testCts.Token);
                }, _testCts.Token);
            }
            catch (OperationCanceledException)
            {
                AddTestLogRow("STOP", "Test is stopped by user.");
            }
            catch (Exception ex)
            {
                AddTestLogRow("ERROR", ex.Message);
            }
            finally
            {
                AddTestLogRow("COMPLETE", $"Test Completed → {info.Id}");

                _isRunningTest = false;
                _testCts?.Dispose();
                _testCts = null;

                bool canRun = _testSuite != null && cmbTest.Items.Count > 0;
                cmbTestCategory.Enabled = canRun;
                cmbTest.Enabled = canRun;
                btnRunTest.Enabled = canRun;
                btnStopTest.Enabled = false;
            }
        }

        /// <summary>
        /// 실행 중인 비동기 테스트를 취소하기 위해 Cancellation Token을 트리거합니다.
        /// </summary>
        private void btnStopTest_Click(object sender, EventArgs e)
        {
            if (!_isRunningTest)
                return;

            _testCts?.Cancel();
            btnStopTest.Enabled = false;
        }

        /// <summary>
        /// ITestUiContext 구현: 외부 플러그인에서 파일 열기 다이얼로그를 사용할 수 있도록 노출합니다.
        /// </summary>
        public string? OpenFileDialog(string filter, string title)
        {
            using var ofd = new OpenFileDialog { Filter = filter, Title = title };
            return ofd.ShowDialog(this) == DialogResult.OK ? ofd.FileName : null;
        }

        /// <summary>
        /// ITestUiContext 구현: 런타임에 사용자로부터 텍스트 입력을 받기 위해 인터페이스를 매핑합니다.
        /// </summary>
        public string? PromptInput(string title, string label, string defaultValue)
        {
            return PromptText(title, label, defaultValue);
        }

        #region Window Snap (Aero Snap API)[DllImport("user32.dll")]
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_LWIN = 0x5B;
        private const byte VK_LEFT = 0x25;
        private const byte VK_RIGHT = 0x27;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// Windows OS의 단축키(Win + 방향키)를 에뮬레이트하여 창을 화면의 좌측 또는 우측에 스냅시킵니다.
        /// </summary>
        /// <param name="hWnd">대상 창의 핸들(Handle)입니다.</param>
        /// <param name="directionKey">스냅 방향(VK_LEFT 또는 VK_RIGHT)입니다.</param>
        private void SnapWindow(IntPtr hWnd, byte directionKey)
        {
            SetForegroundWindow(hWnd);
            System.Threading.Thread.Sleep(100);

            // Win 키 누름
            keybd_event(VK_LWIN, 0, 0, 0);
            // 방향키 누름
            keybd_event(directionKey, 0, 0, 0);

            // 키 뗌
            keybd_event(directionKey, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
        }

        /// <summary>
        /// 현재 실행 중인 툴 메인 창과 외부 프로세스(엑셀) 창을 좌/우 5:5 비율로 화면에 나란히 배치합니다.
        /// </summary>
        /// <param name="excelProcess">나란히 배치할 외부 엑셀 프로세스입니다.</param>
        private async void ArrangeWindowsSideBySide(Process excelProcess)
        {
            try
            {
                Form mainWindow = this.TopLevelControl as Form
                  ?? Application.OpenForms["MainForm"]
                  ?? this;

                if (mainWindow.InvokeRequired)
                {
                    mainWindow.Invoke(new Action(() =>
                    {
                        if (mainWindow.WindowState == FormWindowState.Minimized)
                            mainWindow.WindowState = FormWindowState.Normal;
                        SnapWindow(mainWindow.Handle, VK_RIGHT);
                    }));
                }
                else
                {
                    if (mainWindow.WindowState == FormWindowState.Minimized)
                        mainWindow.WindowState = FormWindowState.Normal;
                    SnapWindow(mainWindow.Handle, VK_RIGHT);
                }

                IntPtr excelHandle = IntPtr.Zero;
                for (int i = 0; i < 30; i++)
                {
                    if (excelProcess.HasExited)
                        return;

                    excelProcess.Refresh();
                    if (excelProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        excelHandle = excelProcess.MainWindowHandle;
                        break;
                    }
                    await Task.Delay(100);
                }

                if (excelHandle != IntPtr.Zero)
                {
                    await Task.Delay(200);
                    SnapWindow(excelHandle, VK_LEFT);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Aero Snap Error] {ex.Message}");
            }
        }

        #endregion
    }
}