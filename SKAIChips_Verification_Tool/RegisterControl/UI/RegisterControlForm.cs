using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public partial class RegisterControlForm : Form
    {
        private Excel.Application? _excelApp;
        private Excel.Workbook? _excelWb;
        private ExcelWriter? _xl;

        private II2cBus? _i2cBus;
        private ISpiBus? _spiBus;
        private IRegisterChip? _chip;
        private const int I2cTimeoutMs = 1000;
        private FtdiDeviceSettings? _ftdiSettings;
        private ProtocolSettings? _protocolSettings;

        private readonly List<IChipProject> _projects = new();
        private IChipProject? _selectedProject;

        private string? _regMapFilePath;
        private readonly List<RegisterGroup> _groups = new();
        private RegisterGroup? _selectedGroup;
        private RegisterDetail? _selectedRegister;
        private RegisterItem? _selectedItem;
        private uint _currentRegValue;
        private bool _isUpdatingRegValue;
        private string? _scriptFilePath;
        private string? _firmwareFilePath;

        private RegisterFieldManager? _regMgr;
        public RegisterFieldManager RegMgr => _regMgr ?? throw new InvalidOperationException("Register map not loaded.");

        private readonly Button[] _bitButtons = new Button[32];
        private bool _isUpdatingBits;

        private int _prevTestCategoryIndex = -1;
        private bool _suppressTestCategoryEvent;

        private readonly List<TreeNode> _selectedNodes = new();
        private TreeNode? _pivotNode;

        private readonly Dictionary<RegisterDetail, uint> _regValues = new();

        private IChipTestSuite? _testSuite;
        private CancellationTokenSource? _testCts;
        private bool _isRunningTest;

        private Button[] _testSlotButtons;

        public RegisterControlForm()
        {
            InitializeComponent();

            InitUi();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            try
            {
                _testCts?.Cancel();
                _xl?.Close();
            }
            catch { }
            DisconnectBus();
            _xl = null;

            CloseRegMapExcel();
        }

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

        private void InitLogGrid()
        {
            dgvRegLog.Rows.Clear();

            dgvRegLog.RowHeadersVisible = false;

            dgvRegLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            if (dgvRegLog.Columns.Contains("colRegLogResult"))
            {
                dgvRegLog.Columns["colRegLogResult"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void InitTreeContextMenu()
        {
            var ctx = new ContextMenuStrip();
            var mExpand = new ToolStripMenuItem("모두 펼치기");
            var mCollapse = new ToolStripMenuItem("모두 접기");
            var mSearch = new ToolStripMenuItem("검색...");

            mExpand.Click += (s, e) => tvRegTree.ExpandAll();
            mCollapse.Click += (s, e) => tvRegTree.CollapseAll();
            mSearch.Click += (s, e) => ShowTreeSearchDialog();

            ctx.Items.Add(mExpand);
            ctx.Items.Add(mCollapse);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mSearch);
            tvRegTree.ContextMenuStrip = ctx;
        }

        private void InitBitButtons()
        {
            flpBitsTop.Controls.Clear();
            flpBitsBottom.Controls.Clear();

            for (int i = 0; i < 32; i++)
            {
                int indexInRow = (i < 16) ? i : i - 16;
                int leftMargin = (indexInRow > 0 && indexInRow % 4 == 0) ? 5 : 1;

                var btn = new Button
                {
                    Margin = new Padding(leftMargin, 1, 1, 1),
                    Padding = new Padding(0),
                    Width = 24,
                    Height = 25,
                    Text = "0",
                    Tag = i,
                    FlatStyle = FlatStyle.Flat
                };

                btn.FlatAppearance.BorderSize = 1;
                btn.UseVisualStyleBackColor = false;
                btn.BackColor = Color.WhiteSmoke;
                btn.ForeColor = Color.Black;
                btn.FlatAppearance.MouseOverBackColor = Color.SkyBlue;
                btn.FlatAppearance.MouseDownBackColor = Color.Gainsboro;

                btn.Click += BitButton_Click;

                _bitButtons[i] = btn;

                UpdateBitButtonVisual(btn);

                if (i < 16)
                    flpBitsTop.Controls.Add(btn);
                else
                    flpBitsBottom.Controls.Add(btn);
            }

            UpdateBitButtonsFromValue(_currentRegValue);
            UpdateBitButtonLayout();
        }

        private void InitBitButtonLayoutHandlers()
        {
            flpBitsTop.SizeChanged += (s, e) => UpdateBitButtonLayout();
            flpBitsBottom.SizeChanged += (s, e) => UpdateBitButtonLayout();
            grpRegControl.Resize += (s, e) => UpdateBitButtonLayout();
        }

        private void UpdateBitButtonLayout()
        {
            int cols = 16;
            const int groupSpacing = 5;

            if (flpBitsTop.ClientSize.Width > 0)
            {
                int panelWidth = flpBitsTop.ClientSize.Width;

                int btnWidth = (panelWidth - (cols + 1) * 2 - groupSpacing * 3) / cols;
                if (btnWidth < 16)
                    btnWidth = 16;
                if (btnWidth > 40)
                    btnWidth = 40;
                int btnHeight = 25;

                for (int i = 0; i < 16; i++)
                {
                    var btn = _bitButtons[i];
                    if (btn == null)
                        continue;
                    btn.Width = btnWidth;
                    btn.Height = btnHeight;
                }
            }

            if (flpBitsBottom.ClientSize.Width > 0)
            {
                int panelWidth = flpBitsBottom.ClientSize.Width;

                int btnWidth = (panelWidth - (cols + 1) * 2 - groupSpacing * 3) / cols;
                if (btnWidth < 16)
                    btnWidth = 16;
                if (btnWidth > 40)
                    btnWidth = 40;
                int btnHeight = 25;

                for (int i = 16; i < 32; i++)
                {
                    var btn = _bitButtons[i];
                    if (btn == null)
                        continue;
                    btn.Width = btnWidth;
                    btn.Height = btnHeight;
                }
            }
        }

        private void UpdateBitButtonVisual(Button btn)
        {
            bool isOne = btn.Text == "1";

            if (btn.Enabled)
            {
                btn.BackColor = isOne ? Color.LightSkyBlue : Color.WhiteSmoke;
                btn.ForeColor = Color.Black;
            }
            else
            {
                btn.BackColor = isOne ? Color.SteelBlue : Color.DarkGray;
                btn.ForeColor = Color.Gainsboro;
            }
        }

        private void InitRegisterMapControls()
        {
            lblMapFileName.Text = "(No file)";
            btnOpenMapPath.Enabled = false;
        }

        private void InitRegisterValueControls()
        {
            UpdateBitButtonsFromValue(_currentRegValue);
            SetBitButtonsEnabledForItem(null);

            txtRegValueHex.Leave += txtRegValueHex_Leave;

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

        private void InitScriptControls()
        {
            lblScriptFileName.Text = "(No script)";
            btnOpenScriptPath.Enabled = false;
        }

        private void InitStatusControls()
        {
            btnConnect.Text = "Connect";
            UpdateStatusText();
            UpdateConnectionControls();
        }

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

        private void TvRegTree_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var hitTest = tvRegTree.HitTest(e.Location);

            if (hitTest.Node == null ||
                (hitTest.Location != TreeViewHitTestLocations.Label &&
                 hitTest.Location != TreeViewHitTestLocations.Image &&
                 hitTest.Location != TreeViewHitTestLocations.StateImage))
            {
                return;
            }

            TreeNode clickedNode = hitTest.Node;

            if (tvRegTree.SelectedNode != clickedNode)
            {
                tvRegTree.SelectedNode = clickedNode;
            }

            Keys modifier = Control.ModifierKeys;

            tvRegTree.BeginUpdate();

            if (modifier == Keys.Control)
            {
                ToggleSelection(clickedNode);
                _pivotNode = clickedNode;
            }
            else if (modifier == Keys.Shift)
            {
                if (_pivotNode == null)
                    _pivotNode = clickedNode;
                ClearSelection();
                SelectRange(_pivotNode, clickedNode);
            }
            else
            {
                ClearSelection();
                AddSelection(clickedNode);
                _pivotNode = clickedNode;
            }

            tvRegTree.EndUpdate();
        }

        private void AddSelection(TreeNode node)
        {
            if (!_selectedNodes.Contains(node))
            {
                _selectedNodes.Add(node);
                UpdateNodeVisual(node, true);
            }
        }

        private void RemoveSelection(TreeNode node)
        {
            if (_selectedNodes.Contains(node))
            {
                _selectedNodes.Remove(node);
                UpdateNodeVisual(node, false);
            }
        }

        private void ToggleSelection(TreeNode node)
        {
            if (_selectedNodes.Contains(node))
                RemoveSelection(node);
            else
                AddSelection(node);
        }

        private void ClearSelection()
        {
            var oldNodes = _selectedNodes.ToList();
            _selectedNodes.Clear();

            foreach (var node in oldNodes)
            {
                UpdateNodeVisual(node, false);
            }
        }

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

        private void RefreshNodeStyle(TreeNode node)
        {
            if (node == null)
                return;

            bool isExplicitlySelected = _selectedNodes.Contains(node);

            bool isRegisterWithSelectedBit = (node.Tag is RegisterDetail) &&
                                             node.Nodes.Cast<TreeNode>().Any(c => _selectedNodes.Contains(c));

            bool hasSelectedParent = node.Parent != null && _selectedNodes.Contains(node.Parent);

            bool isBitWithSelectedSibling = (node.Tag is RegisterItem) &&
                                            node.Parent != null &&
                                            node.Parent.Nodes.Cast<TreeNode>().Any(c => _selectedNodes.Contains(c));

            if (isExplicitlySelected)
            {
                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
            else if (isRegisterWithSelectedBit || hasSelectedParent || isBitWithSelectedSibling)
            {
                node.BackColor = Color.LightSkyBlue;
                node.ForeColor = Color.Black;
            }
            else
            {
                node.BackColor = Color.Empty;
                node.ForeColor = Color.Black;
            }
        }

        private void SelectRange(TreeNode startNode, TreeNode endNode)
        {
            TreeNode? curr = tvRegTree.Nodes.Count > 0 ? tvRegTree.Nodes[0] : null;
            bool inRange = false;

            while (curr != null)
            {
                bool isBoundary = (curr == startNode || curr == endNode);

                if (isBoundary)
                {
                    if (!inRange)
                        inRange = true;
                    else
                    {
                        AddSelection(curr);
                        break;
                    }
                }

                if (inRange || isBoundary)
                {
                    AddSelection(curr);
                }

                curr = curr.NextVisibleNode;
            }
        }

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

        private string GetCurrentProjectName()
        {
            return _selectedProject?.Name ?? "UnknownProject";
        }

        private void InitTestLogContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var miSave = new ToolStripMenuItem("Save as .txt");
            miSave.Click += TestLog_SaveAsTxt_Click;

            var miClear = new ToolStripMenuItem("Clear");
            miClear.Click += TestLog_Clear_Click;

            var miSelectAll = new ToolStripMenuItem("Select All");
            miSelectAll.Click += TestLog_SelectAll_Click;

            var miCopy = new ToolStripMenuItem("Copy");
            miCopy.Click += TestLog_Copy_Click;

            ctx.Items.Add(miSave);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(miClear);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(miSelectAll);
            ctx.Items.Add(miCopy);

            rtbRunTestLog.ContextMenuStrip = ctx;
        }

        private void TestLog_SaveAsTxt_Click(object? sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.FileName = $"{GetCurrentProjectName()}_RunTestLog.txt";

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllText(sfd.FileName, rtbRunTestLog.Text);
            }
        }

        private void TestLog_Clear_Click(object? sender, EventArgs e)
        {
            rtbRunTestLog.Clear();
        }

        private void TestLog_SelectAll_Click(object? sender, EventArgs e)
        {
            rtbRunTestLog.SelectAll();
        }

        private void TestLog_Copy_Click(object? sender, EventArgs e)
        {
            if (rtbRunTestLog.SelectionLength > 0)
                rtbRunTestLog.Copy();
        }

        public void UpdateTestProgress(int percent)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(UpdateTestProgress), percent);
                return;
            }

            if (probarRuntest != null)
            {
                if (percent < 0)
                    percent = 0;
                if (percent > 100)
                    percent = 100;
                probarRuntest.Value = percent;
            }
        }

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

        private void TestSlotButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Action action)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    AppendLog($"[Error] Test Slot Execution Failed: {ex.Message}");
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

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
                MessageBox.Show("No registers found with keyword: " + keyword, "Search Result");
                return;
            }

            if (matchedNodes.Count == 1)
            {
                tvRegTree.SelectedNode = matchedNodes[0];
                matchedNodes[0].EnsureVisible();
                return;
            }

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
                        tvRegTree.SelectedNode = selected;
                        selected.EnsureVisible();
                        resultsForm.Close();
                    }
                };

                resultsForm.Controls.Add(listBox);
                resultsForm.ShowDialog(this);
            }
        }

        private List<TreeNode> FindAllMatchingNodes(TreeNode root, string keyword)
        {
            List<TreeNode> result = new List<TreeNode>();
            if (root.Text.ToUpper().Contains(keyword))
                result.Add(root);

            foreach (TreeNode child in root.Nodes)
                result.AddRange(FindAllMatchingNodes(child, keyword));

            return result;
        }

        private TreeNode FindNodeRecursive(TreeNode root, string keyword)
        {
            if (root.Text.ToUpper().Contains(keyword))
                return root;

            foreach (TreeNode child in root.Nodes)
            {
                TreeNode result = FindNodeRecursive(child, keyword);
                if (result != null)
                    return result;
            }
            return null;
        }

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

        private async Task<(bool success, T result)> RunWithTimeout<T>(Func<T> action, int timeoutMs)
        {
            var task = Task.Run(action);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            if (completed == task)
                return (true, task.Result);
            return (false, default!);
        }

        private async Task<bool> RunWithTimeout(Action action, int timeoutMs)
        {
            var task = Task.Run(action);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            return completed == task;
        }

        private bool TryParseHexUInt(string text, out uint value)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(2);

            return uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        private void OpenOrAttachWorkbook(string path, bool visible = true, bool readOnly = true)
        {
            path = Path.GetFullPath(path);

            _excelApp ??= GetOrCreateExcelApp();
            _excelApp.Visible = visible;

            if (_excelWb != null)
            {
                try
                {
                    if (string.Equals(Path.GetFullPath(_excelWb.FullName), path, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                catch { }
            }

            foreach (Excel.Workbook wb in _excelApp.Workbooks)
            {
                if (string.Equals(Path.GetFullPath(wb.FullName), path, StringComparison.OrdinalIgnoreCase))
                {
                    _excelWb = wb;
                    return;
                }
                ReleaseCom(wb);
            }

            _excelWb = _excelApp.Workbooks.Open(path, ReadOnly: readOnly);
        }

        private List<string> GetWorksheetNames()
        {
            if (_excelWb == null)
                return new List<string>();

            var names = new List<string>();
            foreach (Excel.Worksheet ws in _excelWb.Worksheets)
            {
                names.Add(ws.Name);
                ReleaseCom(ws);
            }
            return names;
        }

        private string[,] ReadWorksheetToStringArray(string sheetName)
        {
            var ws = (Excel.Worksheet)_excelWb.Worksheets[sheetName];

            var lastCell = ws.Cells.Find(
                What: "*",
                LookIn: Excel.XlFindLookIn.xlFormulas,
                LookAt: Excel.XlLookAt.xlPart,
                SearchOrder: Excel.XlSearchOrder.xlByRows,
                SearchDirection: Excel.XlSearchDirection.xlPrevious,
                MatchCase: false);

            if (lastCell == null)
            {
                ReleaseCom(ws);
                return new string[0, 0];
            }

            int lastRow = lastCell.Row;
            int lastCol = lastCell.Column;

            var start = (Excel.Range)ws.Cells[1, 1];
            var end = (Excel.Range)ws.Cells[lastRow, lastCol];
            var rng = ws.Range[start, end];

            object value2 = rng.Value2;

            ReleaseCom(rng);
            ReleaseCom(end);
            ReleaseCom(start);
            ReleaseCom(lastCell);
            ReleaseCom(ws);

            return ExcelDataConverter.ToStringArrayFromValue2(value2);
        }

        private static Excel.Application GetOrCreateExcelApp()
        {
            try
            {
                var app = ComActiveObject.TryGet<Excel.Application>("Excel.Application");
                return app ?? new Excel.Application();
            }
            catch { return new Excel.Application(); }
        }

        private static void ReleaseCom(object o)
        {
            if (o != null && Marshal.IsComObject(o))
                Marshal.ReleaseComObject(o);
        }

        private void AddLog(string type, string addrText, string dataText, string result)
        {
            int rowIndex = dgvRegLog.Rows.Add();
            var row = dgvRegLog.Rows[rowIndex];

            row.Cells["colRegLogTime"].Value = DateTime.Now.ToString("HH:mm:ss");
            row.Cells["colRegLogType"].Value = type;
            row.Cells["colRegLogAddr"].Value = addrText;
            row.Cells["colRegLogData"].Value = dataText;
            row.Cells["colRegLogResult"].Value = result;

            dgvRegLog.FirstDisplayedScrollingRowIndex = rowIndex;
        }

        public void AddTestLogRow(string level, string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            rtbRunTestLog.AppendText(line + Environment.NewLine);
            rtbRunTestLog.SelectionStart = rtbRunTestLog.TextLength;
            rtbRunTestLog.ScrollToCaret();
        }

        public void AppendLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string logMsg = $"[{timestamp}] {message}\r\n";

                rtbRunTestLog.AppendText(logMsg);

                rtbRunTestLog.SelectionStart = rtbRunTestLog.Text.Length;
                rtbRunTestLog.ScrollToCaret();
            }
            catch { /* 로그 에러는 무시 */ }
        }

        private static DeviceKind ResolveDeviceKind(FtdiDeviceSettings s)
        {
            var desc = s.Description ?? "";
            if (desc.IndexOf("UM232H", StringComparison.OrdinalIgnoreCase) >= 0)
                return DeviceKind.UM232H;
            if (desc.IndexOf("FT232H", StringComparison.OrdinalIgnoreCase) >= 0)
                return DeviceKind.UM232H;
            return DeviceKind.FT4222;
        }

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

        private void LoadProjects()
        {
            _projects.Clear();

            var projectType = typeof(IChipProject);
            var asm = typeof(Oasis).Assembly;

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
                MessageBox.Show($"No project corresponding to file name ('{fileName}') was found.\nPlease check the project keywords.",
                                "Project Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

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

        private void TryConnect()
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Selected Project is Null.\nSelect the Project Name to Connect!!");
                return;
            }

            DisconnectBus();

            if (_ftdiSettings == null)
            {
                MessageBox.Show("FTDI Device is Null.\nSetup Device to Connect!!");
                return;
            }

            if (_protocolSettings == null)
            {
                MessageBox.Show("Protocol Setting is Null.\nSetup Protocol Setting to Connect!!");
                return;
            }

            _protocolSettings.DeviceKind = ResolveDeviceKind(_ftdiSettings);
            uint devIndex = (uint)_ftdiSettings.DeviceIndex;

            try
            {
                _protocolSettings.ForceIdleHighOnConnect = (_selectedProject is Chicago);
                if (_selectedProject is Chicago)
                    _protocolSettings.DeviceKind = DeviceKind.UM232H;

                if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.I2C)
                {
                    if (_selectedProject is not II2cChipProject i2cProj)
                    {
                        MessageBox.Show("The selected project is not an I2C chip project.");
                        return;
                    }

                    _i2cBus = new I2cBus(devIndex, _protocolSettings);
                    if (!_i2cBus.Connect())
                    {
                        _i2cBus = null;
                        MessageBox.Show("I2C Connect Failed.");
                        UpdateStatusText();
                        return;
                    }

                    _chip = i2cProj.CreateChip(_i2cBus, _protocolSettings);
                }
                else if (_protocolSettings.ProtocolRegLogType == ProtocolRegLogType.SPI)
                {
                    if (_selectedProject is not ISpiChipProject spiProj)
                    {
                        MessageBox.Show("The selected project is not an SPI chip project.");
                        return;
                    }

                    _spiBus = new SpiBus(devIndex, _protocolSettings);
                    if (!_spiBus.Connect())
                    {
                        _spiBus = null;
                        MessageBox.Show("SPI Connect Failed.");
                        UpdateStatusText();
                        return;
                    }

                    _chip = spiProj.CreateChip(_spiBus, _protocolSettings);
                }
                else
                {
                    MessageBox.Show("Protocol not supported.");
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
                MessageBox.Show("Connection Error: " + ex.Message);
            }
        }

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

            bool hasTests = _testSuite != null && _testSuite.Tests != null && _testSuite.Tests.Count > 0;
            cmbTestCategory.Enabled = isConnected && hasTests;
            cmbTest.Enabled = isConnected && hasTests;
        }

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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool isConnected = (_i2cBus != null && _i2cBus.IsConnected) || (_spiBus != null && _spiBus.IsConnected);

            if (!isConnected)
                TryConnect();
            else
                DisconnectBus();
        }

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

        private void btnProtocolSetup_Click(object sender, EventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Selected Project is null.\nOpen the Register Map Excel to Select Project Name!!");
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

        private void txtRegValueHex_Leave(object sender, EventArgs e)
        {
            if (TryParseHexUInt(txtRegValueHex.Text, out uint v))
            {
                _currentRegValue = v;
                UpdateBitCurrentValues();
            }
            else
            {
                MessageBox.Show("Invalid register value format, e.g. 0x0000000000");
                txtRegValueHex.Text = $"0x{_currentRegValue:X8}";
            }
        }

        private void btnSelectMapFile_Click(object sender, EventArgs e)
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

                    _xl ??= new ExcelWriter();
                    _xl.OpenOrAttach(_regMapFilePath, visible: true, readOnly: true, createIfMissing: false);

                    foreach (var name in _xl.GetSheetNames())
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
                    MessageBox.Show("Fail to Open Excel: " + ex.Message);
                }
            }
        }

        private void CloseRegMapExcel()
        {
            try
            {
                if (_excelWb != null)
                {
                    _excelWb.Saved = true;
                    _excelWb.Close(false);
                }
            }
            catch { }

            try
            {
                if (_excelApp != null)
                    _excelApp.Quit();
            }
            catch { }

            try
            {
                if (_excelWb != null)
                    Marshal.FinalReleaseComObject(_excelWb);
            }
            catch { }
            try
            {
                if (_excelApp != null)
                    Marshal.FinalReleaseComObject(_excelApp);
            }
            catch { }

            _excelWb = null;
            _excelApp = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void btnLoadSelectedSheets_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_regMapFilePath))
            {
                MessageBox.Show("Register Map File Path is Null.\nOpen the Register Map Excel to Select Sheet!!");
                return;
            }

            if (clbRegMapSheets.CheckedItems.Count == 0)
            {
                MessageBox.Show("Checked Sheet is Null.\nCheck Sheet to Load Register Tree!!");
                return;
            }

            try
            {
                _xl ??= new ExcelWriter();
                _xl.OpenOrAttach(_regMapFilePath, visible: true, readOnly: true, createIfMissing: false);

                _groups.Clear();
                _regValues.Clear();

                foreach (var item in clbRegMapSheets.CheckedItems)
                {
                    var sheetName = item?.ToString();
                    if (string.IsNullOrWhiteSpace(sheetName))
                        continue;

                    var value2 = _xl.ReadUsedRangeValue2(sheetName);
                    var data = (value2.Length == 0) ? new string[0, 0] : ExcelDataConverter.FromObjectMatrix(value2);
                    var group = RegisterMapParser.MakeRegisterGroup(sheetName, data);
                    _groups.Add(group);
                }

                _regMgr = new RegisterFieldManager(() => _groups);

                BuildRegisterTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to Load Register Map: " + ex.Message);
            }
        }

        private void btnOpenMapPath_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_regMapFilePath) || !File.Exists(_regMapFilePath))
            {
                MessageBox.Show("There are no open register map files.");
                return;
            }

            try
            {
                var arg = $"/select,\"{_regMapFilePath}\"";
                Process.Start("explorer.exe", arg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to open Path: " + ex.Message);
            }
        }

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

        private uint GetRegisterValue(RegisterDetail reg)
        {
            if (_regValues.TryGetValue(reg, out var v))
                return v;

            v = reg.ResetValue;
            _regValues[reg] = v;
            return v;
        }

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

        private void ResetSelectionState()
        {
            _selectedGroup = null;
            _selectedRegister = null;
            _selectedItem = null;
        }

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

        private void HandleRegisterSelection(RegisterDetail register, RegisterGroup? parentGroup)
        {
            _selectedGroup = parentGroup;
            LoadRegisterToUi(register, null);
        }

        private void HandleItemSelection(RegisterItem item, RegisterDetail? parentRegister, RegisterGroup? parentGroup)
        {
            _selectedGroup = parentGroup;
            if (parentRegister != null)
                LoadRegisterToUi(parentRegister, item);
            else
                ShowNoSelectionState();
        }

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

        private void SetBitButtonsEnabledForItem(RegisterItem? item)
        {
            if (item == null)
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

        private void UpdateBitCurrentValues()
        {
            txtRegValueHex.Text = $"0x{_currentRegValue:X8}";

            if (_selectedRegister != null)
                _regValues[_selectedRegister] = _currentRegValue;

            UpdateBitButtonsFromValue(_currentRegValue);
            UpdateNumRegIndexForSelectedItem();

            if (_selectedRegister != null)
                UpdateTreeNodesForRegister(_selectedRegister, _currentRegValue);
        }

        private void UpdateTreeNodesForRegister(RegisterDetail reg, uint regValue)
        {
            if (tvRegTree == null)
                return;

            var stack = new Stack<TreeNode>();
            foreach (TreeNode root in tvRegTree.Nodes)
                stack.Push(root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                if (node.Tag is RegisterItem item && node.Parent?.Tag is RegisterDetail parentReg && ReferenceEquals(parentReg, reg))
                    node.Text = FormatItemNodeText(item, regValue);

                foreach (TreeNode child in node.Nodes)
                    stack.Push(child);
            }
        }

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

        private void UpdateNumRegIndexForSelectedItem()
        {
            _isUpdatingRegValue = true;
            try
            {
                if (_selectedItem == null)
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

        private void btnOpenScriptPath_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_scriptFilePath) || !File.Exists(_scriptFilePath))
            {
                MessageBox.Show("No script files are open.");
                return;
            }

            var arg = $"/select,\"{_scriptFilePath}\"";
            Process.Start("explorer.exe", arg);
        }

        private async void btnRead_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("Device is Null.\nConnect Device to Read!!");
                return;
            }

            var targets = GetSelectedRegisters();

            if (targets.Count == 0 && _selectedRegister != null)
            {
                targets.Add(_selectedRegister);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("Check or select the register to read.");
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

        private async void btnWrite_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("Device is Null.\nConnect Device to Write!!");
                return;
            }

            var targets = GetSelectedRegisters();

            if (targets.Count == 0 && _selectedRegister != null)
            {
                targets.Add(_selectedRegister);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("Check or select the register to write.");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var reg in targets)
            {
                uint addr = reg.Address;

                uint data = GetRegisterValue(reg);

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

        private async void btnWriteAll_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("Device is Null.\nConnect Device to Write!!");
                return;
            }

            if (_groups.Count == 0)
            {
                MessageBox.Show("Register Tree is Null.\nLoad Sheet on Register Tree to Write!!");
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

        private async void btnReadAll_Click(object sender, EventArgs e)
        {
            if (_chip == null)
            {
                MessageBox.Show("Device is Null.\nConnect Device to Read!!");
                return;
            }

            if (_groups.Count == 0)
            {
                MessageBox.Show("Register Tree is Null.\nLoad Sheet on Register Tree to Read!!");
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

        private async void btnRunTest_Click(object sender, EventArgs e)
        {
            if (_testSuite == null)
            {
                MessageBox.Show($"{_selectedProject} is not Supported Run Test!!");
                return;
            }

            if (_isRunningTest)
            {
                MessageBox.Show("Test is already running!!");
                return;
            }

            if (cmbTest.SelectedItem is not ChipTestInfo info)
            {
                MessageBox.Show("Selected Item is Null.\nSelect Test to Running!!");
                return;
            }

            if (info.Id == "FW.FLASH_WRITE")
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "FW File (*.bin;*.hex)|*.bin;*.hex|All files (*.*)|*.*";
                    ofd.Title = "Select Firmware File";

                    if (ofd.ShowDialog(this) != DialogResult.OK)
                    {
                        MessageBox.Show("Canceled to Select .bin File.");
                        return;
                    }

                    if (_testSuite is Oasis oasisSuite)
                        oasisSuite.SetFirmwareFilePath(ofd.FileName);
                }
            }

            if (info.Id == "FW.FLASH_VERIFY" || info.Id == "FW.FLASH_READ")
            {
                string? s = PromptText("FLASH FUNCTION", "Enter the Flash Size[Byte]:", "524288");
                if (string.IsNullOrWhiteSpace(s))
                {
                    MessageBox.Show("Canceled to enter Flash Size.");
                    return;
                }

                if (!uint.TryParse(s, out uint flashSize) || flashSize == 0)
                {
                    MessageBox.Show("Flash size must be bigger than 0!!.");
                    return;
                }

                if (_testSuite is Oasis oasisSuite)
                    oasisSuite.SetFlashSize(flashSize);
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

        private void btnStopTest_Click(object sender, EventArgs e)
        {
            if (!_isRunningTest)
                return;

            _testCts?.Cancel();
            btnStopTest.Enabled = false;
        }

        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 360,
                    Height = 150,
                    Text = caption,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent
                };

                Label label = new Label() { Left = 20, Top = 20, Text = text, Width = 300 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 300 };
                Button buttonOk = new Button() { Text = "OK", Left = 240, Width = 80, Top = 80 };

                buttonOk.Click += (sender, e) => { prompt.DialogResult = DialogResult.OK; prompt.Close(); };

                prompt.Controls.Add(label);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(buttonOk);
                prompt.AcceptButton = buttonOk;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        internal static class ComActiveObject
        {
            [DllImport("oleaut32.dll", PreserveSig = true)]
            private static extern int GetActiveObject(ref Guid rclsid, IntPtr reserved,
                [MarshalAs(UnmanagedType.Interface)] out object ppunk);

            public static T? TryGet<T>(string progId) where T : class
            {
                var type = Type.GetTypeFromProgID(progId, throwOnError: false);
                if (type == null)
                    return null;

                var clsid = type.GUID;

                var hr = GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
                if (hr != 0)
                    return null;

                return obj as T;
            }
        }
    }
}
