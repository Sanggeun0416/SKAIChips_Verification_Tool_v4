namespace SKAIChips_Verification_Tool.RegisterControl
{
    public partial class RegisterControlForm
    {
        private void InitDynamicUi()
        {
            InitLogGrid();
            InitTreeContextMenu();
            InitBitButtons();
            InitBitButtonLayoutHandlers();
            InitTestLogContextMenu();
            InitializeTestSlotButtons();
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
            ctx.Items.Add(new ToolStripMenuItem("모두 펼치기", null, (s, e) => tvRegTree.ExpandAll()));
            ctx.Items.Add(new ToolStripMenuItem("모두 접기", null, (s, e) => tvRegTree.CollapseAll()));
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(new ToolStripMenuItem("검색...", null, (s, e) => ShowTreeSearchDialog()));
            tvRegTree.ContextMenuStrip = ctx;
        }

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

                if (i < 16)
                    flpBitsTop.Controls.Add(btn);
                else
                    flpBitsBottom.Controls.Add(btn);
            }
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

            void ResizePanelButtons(FlowLayoutPanel panel, int startIndex, int endIndex)
            {
                if (panel.ClientSize.Width <= 0)
                    return;
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

        private void AddSelection(TreeNode node)
        {
            if (!_selectedNodes.Contains(node))
            {
                _selectedNodes.Add(node);
                UpdateNodeVisual(node);
            }
        }

        private void RemoveSelection(TreeNode node)
        {
            if (_selectedNodes.Contains(node))
            {
                _selectedNodes.Remove(node);
                UpdateNodeVisual(node);
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
                UpdateNodeVisual(node);
        }

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
                    AddSelection(curr);
                curr = curr.NextVisibleNode;
            }
        }

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
    }
}