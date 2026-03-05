using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// RegisterControlForm의 UI 구성 요소 초기화, 동적 레이아웃 조정, 사용자 입력 처리(트리뷰 다중 선택 등),
    /// 그리고 로그 출력(그리드, 텍스트박스) 기능을 담당하는 Partial 클래스입니다.
    /// </summary>
    public partial class RegisterControlForm
    {
        #region Initialization Methods

        /// <summary>
        /// 동적으로 구성되는 UI 요소들과 컨텍스트 메뉴 등을 일괄 초기화합니다.
        /// (MainForm의 InitUi() 내에서 호출되거나 별도로 사용될 수 있습니다.)
        /// </summary>
        private void InitDynamicUi()
        {
            InitLogGrid();
            InitTreeContextMenu();
            InitBitButtons();
            InitBitButtonLayoutHandlers();
            InitTestLogContextMenu();
            InitializeTestSlotButtons();
        }

        /// <summary>
        /// 레지스터 읽기/쓰기 결과를 표시하는 DataGridView(dgvRegLog)의 속성과 열 크기 모드를 초기화합니다.
        /// </summary>
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

        /// <summary>
        /// 레지스터 트리뷰(TreeView)에서 사용할 우클릭 컨텍스트 메뉴(모두 펼치기, 모두 접기, 검색)를 생성하고 연결합니다.
        /// </summary>
        private void InitTreeContextMenu()
        {
            var ctx = new ContextMenuStrip();
            ctx.Items.Add(new ToolStripMenuItem("모두 펼치기", null, (s, e) => tvRegTree.ExpandAll()));
            ctx.Items.Add(new ToolStripMenuItem("모두 접기", null, (s, e) => tvRegTree.CollapseAll()));
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(new ToolStripMenuItem("검색...", null, (s, e) => ShowTreeSearchDialog()));

            tvRegTree.ContextMenuStrip = ctx;
        }

        /// <summary>
        /// 테스트 자동화 로그 창(RichTextBox)에서 사용할 우클릭 컨텍스트 메뉴(저장, 지우기, 전체 선택, 복사)를 생성하고 연결합니다.
        /// </summary>
        private void InitTestLogContextMenu()
        {
            var ctx = new ContextMenuStrip();
            ctx.Items.Add(new ToolStripMenuItem("Save as .txt", null, TestLog_SaveAsTxt_Click));
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(new ToolStripMenuItem("Clear", null, TestLog_Clear_Click));
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(new ToolStripMenuItem("Select All", null, TestLog_SelectAll_Click));
            ctx.Items.Add(new ToolStripMenuItem("Copy", null, TestLog_Copy_Click));

            rtbRunTestLog.ContextMenuStrip = ctx;
        }

        /// <summary>
        /// 32비트 레지스터 값을 개별적으로 제어할 수 있는 32개의 버튼을 동적으로 생성하여 패널(상단/하단)에 배치합니다.
        /// </summary>
        private void InitBitButtons()
        {
            flpBitsTop.Controls.Clear();
            flpBitsBottom.Controls.Clear();

            for (int i = 0; i < 32; i++)
            {
                int indexInRow = (i < 16) ? i : i - 16;

                // 4비트(Nibble) 단위로 시각적 구분을 주기 위해 왼쪽 여백(Margin)을 조정합니다.
                int leftMargin = (indexInRow > 0 && indexInRow % 4 == 0) ? 5 : 1;

                var btn = new Button
                {
                    Margin = new Padding(leftMargin, 1, 1, 1),
                    Padding = new Padding(0),
                    Width = 24,
                    Height = 25,
                    Text = "0",
                    Tag = i,
                    FlatStyle = FlatStyle.Flat,
                    UseVisualStyleBackColor = false,
                    BackColor = Color.WhiteSmoke,
                    ForeColor = Color.Black
                };

                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = Color.SkyBlue;
                btn.FlatAppearance.MouseDownBackColor = Color.Gainsboro;
                btn.Click += BitButton_Click;

                _bitButtons[i] = btn;
                UpdateBitButtonVisual(btn);

                // MSB부터 LSB 방향을 고려하여 상/하단 패널에 나누어 배치
                if (i < 16)
                    flpBitsTop.Controls.Add(btn);
                else
                    flpBitsBottom.Controls.Add(btn);
            }
        }

        /// <summary>
        /// 창 크기 변경에 따라 비트 버튼들의 크기를 유동적으로 조절하기 위한 이벤트 핸들러를 등록합니다.
        /// </summary>
        private void InitBitButtonLayoutHandlers()
        {
            flpBitsTop.SizeChanged += (s, e) => UpdateBitButtonLayout();
            flpBitsBottom.SizeChanged += (s, e) => UpdateBitButtonLayout();
            grpRegControl.Resize += (s, e) => UpdateBitButtonLayout();
        }

        #endregion

        #region Bit Buttons Layout & Visual

        /// <summary>
        /// 부모 패널의 너비에 맞춰 16개의 비트 버튼 너비를 균등하게 계산하고 조정합니다. (반응형 UI)
        /// </summary>
        private void UpdateBitButtonLayout()
        {
            int cols = 16;
            const int groupSpacing = 5;

            void ResizePanelButtons(FlowLayoutPanel panel, int startIndex, int endIndex)
            {
                if (panel.ClientSize.Width <= 0)
                    return;

                // 패널 너비에서 여백을 제외하고 버튼 1개당 할당될 가용 너비 계산
                int btnWidth = (panel.ClientSize.Width - (cols + 1) * 2 - groupSpacing * 3) / cols;
                btnWidth = Math.Max(16, Math.Min(btnWidth, 40));

                for (int i = startIndex; i < endIndex; i++)
                {
                    if (_bitButtons[i] != null)
                    {
                        _bitButtons[i].Width = btnWidth;
                        _bitButtons[i].Height = 25;
                    }
                }
            }

            ResizePanelButtons(flpBitsTop, 0, 16);
            ResizePanelButtons(flpBitsBottom, 16, 32);
        }

        /// <summary>
        /// 비트 버튼의 활성화 상태와 값(0 또는 1)에 따라 배경색과 글자색을 시각적으로 업데이트합니다.
        /// </summary>
        /// <param name="btn">업데이트할 대상 버튼입니다.</param>
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

        #endregion

        #region TreeView Multi-Selection Logic

        /// <summary>
        /// 트리뷰에서 마우스 클릭 시 Ctrl/Shift 키 조합을 감지하여 윈도우 탐색기 스타일의 다중 선택 기능을 구현합니다.
        /// </summary>
        private void TvRegTree_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var hitTest = tvRegTree.HitTest(e.Location);
            if (hitTest.Node == null ||
                (hitTest.Location != TreeViewHitTestLocations.Label &&
                 hitTest.Location != TreeViewHitTestLocations.Image &&
                 hitTest.Location != TreeViewHitTestLocations.StateImage))
                return;

            TreeNode clickedNode = hitTest.Node;

            if (tvRegTree.SelectedNode != clickedNode)
                tvRegTree.SelectedNode = clickedNode;

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

        /// <summary>
        /// 특정 노드를 선택 목록에 추가하고 시각적 스타일을 갱신합니다.
        /// </summary>
        private void AddSelection(TreeNode node)
        {
            if (!_selectedNodes.Contains(node))
            {
                _selectedNodes.Add(node);
                UpdateNodeVisual(node);
            }
        }

        /// <summary>
        /// 특정 노드를 선택 목록에서 제거하고 시각적 스타일을 갱신합니다.
        /// </summary>
        private void RemoveSelection(TreeNode node)
        {
            if (_selectedNodes.Contains(node))
            {
                _selectedNodes.Remove(node);
                UpdateNodeVisual(node);
            }
        }

        /// <summary>
        /// 노드의 선택 상태를 반전(토글)시킵니다.
        /// </summary>
        private void ToggleSelection(TreeNode node)
        {
            if (_selectedNodes.Contains(node))
                RemoveSelection(node);
            else
                AddSelection(node);
        }

        /// <summary>
        /// 다중 선택된 모든 노드의 선택을 해제하고 시각적 스타일을 초기화합니다.
        /// </summary>
        private void ClearSelection()
        {
            var oldNodes = _selectedNodes.ToList();
            _selectedNodes.Clear();

            foreach (var node in oldNodes)
                UpdateNodeVisual(node);
        }

        /// <summary>
        /// 특정 노드와 연관된 부모/자식 노드들의 하이라이트 스타일을 일괄 갱신합니다.
        /// </summary>
        private void UpdateNodeVisual(TreeNode node)
        {
            if (node == null)
                return;

            RefreshNodeStyle(node);

            if (node.Parent != null)
            {
                RefreshNodeStyle(node.Parent);
                foreach (TreeNode sibling in node.Parent.Nodes)
                    RefreshNodeStyle(sibling);
            }

            foreach (TreeNode child in node.Nodes)
                RefreshNodeStyle(child);
        }

        /// <summary>
        /// 노드의 실제 데이터(Group, Register, Item) 및 선택 상태에 따라 배경색과 글자색을 적용합니다.
        /// </summary>
        private void RefreshNodeStyle(TreeNode node)
        {
            if (node == null)
                return;

            bool isExplicitlySelected = _selectedNodes.Contains(node);
            bool isRegisterWithSelectedBit = (node.Tag is RegisterDetail) && node.Nodes.Cast<TreeNode>().Any(c => _selectedNodes.Contains(c));
            bool hasSelectedParent = node.Parent != null && _selectedNodes.Contains(node.Parent);
            bool isBitWithSelectedSibling = (node.Tag is RegisterItem) && node.Parent != null && node.Parent.Nodes.Cast<TreeNode>().Any(c => _selectedNodes.Contains(c));

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

        /// <summary>
        /// Shift 키를 누른 상태에서 클릭했을 때, 기준 노드(startNode)와 클릭 노드(endNode) 사이의 모든 노드를 선택합니다.
        /// </summary>
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
                    {
                        inRange = true;
                    }
                    else
                    {
                        AddSelection(curr);
                        break;
                    }
                }

                if (inRange || isBoundary)
                    AddSelection(curr);

                curr = curr.NextVisibleNode;
            }
        }

        #endregion

        #region UI Logging & Status Methods

        /// <summary>
        /// 레지스터 제어 로그(Read/Write)를 DataGridView에 추가합니다. 메모리 관리를 위해 최대 1000줄로 제한됩니다.
        /// </summary>
        /// <param name="type">작업 타입 (예: "READ", "WRITE")</param>
        /// <param name="addrText">레지스터 주소 텍스트</param>
        /// <param name="dataText">입/출력 데이터 텍스트</param>
        /// <param name="result">결과 (예: "OK", "ERR", "TIMEOUT")</param>
        public void AddLog(string type, string addrText, string dataText, string result)
        {
            if (dgvRegLog.Rows.Count > 1000)
                dgvRegLog.Rows.RemoveAt(0);

            int rowIndex = dgvRegLog.Rows.Add();
            var row = dgvRegLog.Rows[rowIndex];
            row.Cells["colRegLogTime"].Value = DateTime.Now.ToString("HH:mm:ss");
            row.Cells["colRegLogType"].Value = type;
            row.Cells["colRegLogAddr"].Value = addrText;
            row.Cells["colRegLogData"].Value = dataText;
            row.Cells["colRegLogResult"].Value = result;

            dgvRegLog.FirstDisplayedScrollingRowIndex = rowIndex;
        }

        /// <summary>
        /// 테스트 자동화 스위트 실행 시 발생하는 로그를 RichTextBox에 추가합니다. (최대 5000줄 유지)
        /// </summary>
        /// <param name="level">로그 레벨 (예: "INFO", "ERROR")</param>
        /// <param name="message">출력할 메시지 본문</param>
        public void AddTestLogRow(string level, string message)
        {
            if (rtbRunTestLog.Lines.Length > 5000)
            {
                rtbRunTestLog.ReadOnly = false;
                rtbRunTestLog.Select(0, rtbRunTestLog.GetFirstCharIndexFromLine(1000));
                rtbRunTestLog.SelectedText = "";
                rtbRunTestLog.ReadOnly = true;
            }

            string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            rtbRunTestLog.AppendText(line + Environment.NewLine);
            rtbRunTestLog.SelectionStart = rtbRunTestLog.TextLength;
            rtbRunTestLog.ScrollToCaret();
        }

        /// <summary>
        /// 스레드 안전(Thread-Safe)하게 테스트 로그 창에 일반 메시지를 추가합니다.
        /// </summary>
        /// <param name="message">출력할 메시지 내용</param>
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
                rtbRunTestLog.AppendText($"[{timestamp}] {message}\r\n");
                rtbRunTestLog.SelectionStart = rtbRunTestLog.Text.Length;
                rtbRunTestLog.ScrollToCaret();
            }
            catch { }
        }

        /// <summary>
        /// 스레드 안전(Thread-Safe)하게 테스트 진행률 표시기(ProgressBar)의 값을 업데이트합니다.
        /// </summary>
        /// <param name="percent">진행률 (0 ~ 100)</param>
        public void UpdateTestProgress(int percent)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(UpdateTestProgress), percent);
                return;
            }
            if (probarRuntest != null)
                probarRuntest.Value = Math.Max(0, Math.Min(percent, 100));
        }

        #endregion

        #region Helper Classes & Event Handlers

        /// <summary>
        /// 런타임에 사용자로부터 간단한 텍스트 입력을 받기 위한 다이얼로그 헬퍼 클래스입니다.
        /// </summary>
        public static class Prompt
        {
            /// <summary>
            /// 입력 다이얼로그를 모달로 띄우고 사용자가 입력한 문자열을 반환합니다.
            /// </summary>
            /// <param name="text">다이얼로그 내 설명 텍스트</param>
            /// <param name="caption">다이얼로그 창의 제목</param>
            /// <returns>사용자가 입력한 텍스트 (취소 시 빈 문자열)</returns>
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
                Button buttonOk = new Button() { Text = "OK", Left = 240, Width = 80, Top = 80, DialogResult = DialogResult.OK };

                buttonOk.Click += (sender, e) => { prompt.Close(); };

                prompt.Controls.Add(label);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(buttonOk);
                prompt.AcceptButton = buttonOk;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void TestLog_SaveAsTxt_Click(object? sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*", FileName = $"{GetCurrentProjectName()}_RunTestLog.txt" })
            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
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

        #endregion
    }
}