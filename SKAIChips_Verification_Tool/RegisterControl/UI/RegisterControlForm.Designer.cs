using System.Drawing;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    partial class RegisterControlForm
    {
        private System.ComponentModel.IContainer components = null;

        private Button btnConnect;
        private DataGridView dgvRegLog;
        private DataGridViewTextBoxColumn colRegLogTime;
        private DataGridViewTextBoxColumn colRegLogType;
        private DataGridViewTextBoxColumn colRegLogAddr;
        private DataGridViewTextBoxColumn colRegLogData;
        private DataGridViewTextBoxColumn colRegLogResult;
        private Button btnRead;
        private Label lblStatus;
        private Button btnSelectMapFile;
        private CheckedListBox clbRegMapSheets;
        private Button btnLoadSelectedSheets;
        private TreeView tvRegTree;
        private Button btnSaveScript;
        private Button btnLoadScript;
        private Label lblScriptFileName;
        private Button btnOpenScriptPath;
        private Label lblProject;
        private Button btnProtocolSetup;
        private Button btnDeviceSetup;
        private GroupBox grpSetup;
        private GroupBox grpRegMap;
        private GroupBox grpRegControl;
        private GroupBox grpRegLog;
        private GroupBox grpRegTree;
        private Label lblMapFileName;
        private Button btnOpenMapPath;
        private NumericUpDown numRegIndex;
        private TextBox txtRegValueHex;
        private Label lblRegName;
        private FlowLayoutPanel flpBitsTop;
        private FlowLayoutPanel flpBitsBottom;
        private Label lblRegAddrSummary;
        private Label lblRegResetSummary;
        private Button btnWrite;
        private Button btnWriteAll;
        private Button btnReadAll;
        private Label lblProtocol;
        private Label lblDevice;
        private Label lblStatusTitle;
        private Label lblProtocolValue;
        private Label lblDeviceValue;
        private GroupBox grpRunTest;
        private ComboBox cmbTestCategory;
        private ComboBox cmbTest;
        private Button btnRunTest;
        private Button btnStopTest;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnConnect = new Button();
            dgvRegLog = new DataGridView();
            colRegLogTime = new DataGridViewTextBoxColumn();
            colRegLogType = new DataGridViewTextBoxColumn();
            colRegLogAddr = new DataGridViewTextBoxColumn();
            colRegLogData = new DataGridViewTextBoxColumn();
            colRegLogResult = new DataGridViewTextBoxColumn();
            btnRead = new Button();
            lblStatus = new Label();
            btnSelectMapFile = new Button();
            clbRegMapSheets = new CheckedListBox();
            btnLoadSelectedSheets = new Button();
            tvRegTree = new TreeView();
            btnSaveScript = new Button();
            btnLoadScript = new Button();
            lblScriptFileName = new Label();
            btnOpenScriptPath = new Button();
            lblProject = new Label();
            btnProtocolSetup = new Button();
            btnDeviceSetup = new Button();
            grpSetup = new GroupBox();
            lblSelectedProject = new Label();
            lblProtocol = new Label();
            lblProtocolValue = new Label();
            lblDevice = new Label();
            lblDeviceValue = new Label();
            lblStatusTitle = new Label();
            grpRegMap = new GroupBox();
            tlpRegMap = new TableLayoutPanel();
            pnlRegMapButtons = new Panel();
            btnOpenMapPath = new Button();
            lblMapFileName = new Label();
            grpRegControl = new GroupBox();
            numRegIndex = new NumericUpDown();
            txtRegValueHex = new TextBox();
            lblRegName = new Label();
            flpBitsTop = new FlowLayoutPanel();
            flpBitsBottom = new FlowLayoutPanel();
            lblRegAddrSummary = new Label();
            lblRegResetSummary = new Label();
            btnWrite = new Button();
            btnWriteAll = new Button();
            btnReadAll = new Button();
            grpRegLog = new GroupBox();
            grpRegTree = new GroupBox();
            tlpRegTree = new TableLayoutPanel();
            pnlScriptButtons = new Panel();
            splMain = new SplitContainer();
            tlpLeft = new TableLayoutPanel();
            grpRunTest = new GroupBox();
            tlpRunTest = new TableLayoutPanel();
            panelTestLog = new Panel();
            rtbRunTestLog = new RichTextBox();
            panelProgress = new Panel();
            probarRuntest = new ProgressBar();
            tlpRight = new TableLayoutPanel();
            pnlRunTestButtons = new Panel();
            btnTestSlot06 = new Button();
            btnTestSlot07 = new Button();
            btnTestSlot08 = new Button();
            btnTestSlot09 = new Button();
            btnTestSlot10 = new Button();
            cmbTestCategory = new ComboBox();
            cmbTest = new ComboBox();
            btnRunTest = new Button();
            btnStopTest = new Button();
            btnTestSlot01 = new Button();
            btnTestSlot04 = new Button();
            btnTestSlot05 = new Button();
            btnTestSlot02 = new Button();
            btnTestSlot03 = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvRegLog).BeginInit();
            grpSetup.SuspendLayout();
            grpRegMap.SuspendLayout();
            tlpRegMap.SuspendLayout();
            pnlRegMapButtons.SuspendLayout();
            grpRegControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRegIndex).BeginInit();
            grpRegLog.SuspendLayout();
            grpRegTree.SuspendLayout();
            tlpRegTree.SuspendLayout();
            pnlScriptButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
            splMain.Panel1.SuspendLayout();
            splMain.Panel2.SuspendLayout();
            splMain.SuspendLayout();
            tlpLeft.SuspendLayout();
            grpRunTest.SuspendLayout();
            tlpRunTest.SuspendLayout();
            panelTestLog.SuspendLayout();
            panelProgress.SuspendLayout();
            tlpRight.SuspendLayout();
            pnlRunTestButtons.SuspendLayout();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(258, 92);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(98, 23);
            btnConnect.TabIndex = 10;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // dgvRegLog
            // 
            dgvRegLog.AllowUserToAddRows = false;
            dgvRegLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRegLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRegLog.Columns.AddRange(new DataGridViewColumn[] { colRegLogTime, colRegLogType, colRegLogAddr, colRegLogData, colRegLogResult });
            dgvRegLog.Dock = DockStyle.Fill;
            dgvRegLog.Location = new Point(3, 19);
            dgvRegLog.Name = "dgvRegLog";
            dgvRegLog.ReadOnly = true;
            dgvRegLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRegLog.Size = new Size(358, 373);
            dgvRegLog.TabIndex = 8;
            // 
            // colRegLogTime
            // 
            colRegLogTime.HeaderText = "Time";
            colRegLogTime.Name = "colRegLogTime";
            colRegLogTime.ReadOnly = true;
            // 
            // colRegLogType
            // 
            colRegLogType.HeaderText = "Type";
            colRegLogType.Name = "colRegLogType";
            colRegLogType.ReadOnly = true;
            // 
            // colRegLogAddr
            // 
            colRegLogAddr.HeaderText = "Addr";
            colRegLogAddr.Name = "colRegLogAddr";
            colRegLogAddr.ReadOnly = true;
            // 
            // colRegLogData
            // 
            colRegLogData.HeaderText = "Data";
            colRegLogData.Name = "colRegLogData";
            colRegLogData.ReadOnly = true;
            // 
            // colRegLogResult
            // 
            colRegLogResult.HeaderText = "Result";
            colRegLogResult.Name = "colRegLogResult";
            colRegLogResult.ReadOnly = true;
            // 
            // btnRead
            // 
            btnRead.Location = new Point(183, 146);
            btnRead.Name = "btnRead";
            btnRead.Size = new Size(82, 23);
            btnRead.TabIndex = 9;
            btnRead.Text = "Read";
            btnRead.UseVisualStyleBackColor = true;
            btnRead.Click += btnRead_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblStatus.Location = new Point(64, 96);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(179, 15);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "Disconnected";
            // 
            // btnSelectMapFile
            // 
            btnSelectMapFile.Location = new Point(3, 3);
            btnSelectMapFile.Name = "btnSelectMapFile";
            btnSelectMapFile.Size = new Size(100, 23);
            btnSelectMapFile.TabIndex = 18;
            btnSelectMapFile.Text = "Open RegMap";
            btnSelectMapFile.UseVisualStyleBackColor = true;
            btnSelectMapFile.Click += btnSelectMapFile_Click;
            // 
            // clbRegMapSheets
            // 
            clbRegMapSheets.Dock = DockStyle.Fill;
            clbRegMapSheets.FormattingEnabled = true;
            clbRegMapSheets.Location = new Point(3, 3);
            clbRegMapSheets.Name = "clbRegMapSheets";
            clbRegMapSheets.Size = new Size(352, 68);
            clbRegMapSheets.TabIndex = 20;
            // 
            // btnLoadSelectedSheets
            // 
            btnLoadSelectedSheets.Location = new Point(109, 3);
            btnLoadSelectedSheets.Name = "btnLoadSelectedSheets";
            btnLoadSelectedSheets.Size = new Size(100, 23);
            btnLoadSelectedSheets.TabIndex = 21;
            btnLoadSelectedSheets.Text = "Add RegTree";
            btnLoadSelectedSheets.UseVisualStyleBackColor = true;
            btnLoadSelectedSheets.Click += btnLoadSelectedSheets_Click;
            // 
            // tvRegTree
            // 
            tvRegTree.Dock = DockStyle.Fill;
            tvRegTree.Location = new Point(3, 3);
            tvRegTree.Name = "tvRegTree";
            tvRegTree.Size = new Size(412, 458);
            tvRegTree.TabIndex = 22;
            tvRegTree.AfterSelect += tvRegs_AfterSelect;
            // 
            // btnSaveScript
            // 
            btnSaveScript.Location = new Point(2, 4);
            btnSaveScript.Margin = new Padding(2);
            btnSaveScript.Name = "btnSaveScript";
            btnSaveScript.Size = new Size(75, 23);
            btnSaveScript.TabIndex = 23;
            btnSaveScript.Text = "Save Script";
            btnSaveScript.UseVisualStyleBackColor = true;
            btnSaveScript.Click += btnSaveScript_Click;
            // 
            // btnLoadScript
            // 
            btnLoadScript.Location = new Point(79, 4);
            btnLoadScript.Margin = new Padding(2);
            btnLoadScript.Name = "btnLoadScript";
            btnLoadScript.Size = new Size(79, 23);
            btnLoadScript.TabIndex = 24;
            btnLoadScript.Text = "Load Script";
            btnLoadScript.UseVisualStyleBackColor = true;
            btnLoadScript.Click += btnLoadScript_Click;
            // 
            // lblScriptFileName
            // 
            lblScriptFileName.Location = new Point(162, 8);
            lblScriptFileName.Margin = new Padding(2, 0, 2, 0);
            lblScriptFileName.Name = "lblScriptFileName";
            lblScriptFileName.Size = new Size(183, 19);
            lblScriptFileName.TabIndex = 25;
            lblScriptFileName.Text = "(No script file)";
            // 
            // btnOpenScriptPath
            // 
            btnOpenScriptPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenScriptPath.Location = new Point(347, 4);
            btnOpenScriptPath.Margin = new Padding(2);
            btnOpenScriptPath.Name = "btnOpenScriptPath";
            btnOpenScriptPath.Size = new Size(63, 23);
            btnOpenScriptPath.TabIndex = 26;
            btnOpenScriptPath.Text = "Path";
            btnOpenScriptPath.UseVisualStyleBackColor = true;
            btnOpenScriptPath.Click += btnOpenScriptPath_Click;
            // 
            // lblProject
            // 
            lblProject.AutoSize = true;
            lblProject.Location = new Point(6, 22);
            lblProject.Name = "lblProject";
            lblProject.Size = new Size(44, 15);
            lblProject.TabIndex = 0;
            lblProject.Text = "Project";
            // 
            // btnProtocolSetup
            // 
            btnProtocolSetup.Location = new Point(258, 42);
            btnProtocolSetup.Name = "btnProtocolSetup";
            btnProtocolSetup.Size = new Size(98, 23);
            btnProtocolSetup.TabIndex = 4;
            btnProtocolSetup.Text = "Protocol Setup";
            btnProtocolSetup.UseVisualStyleBackColor = true;
            btnProtocolSetup.Click += btnProtocolSetup_Click;
            // 
            // btnDeviceSetup
            // 
            btnDeviceSetup.Location = new Point(258, 67);
            btnDeviceSetup.Name = "btnDeviceSetup";
            btnDeviceSetup.Size = new Size(98, 23);
            btnDeviceSetup.TabIndex = 7;
            btnDeviceSetup.Text = "Device Setup";
            btnDeviceSetup.UseVisualStyleBackColor = true;
            btnDeviceSetup.Click += btnFtdiSetup_Click;
            // 
            // grpSetup
            // 
            grpSetup.Controls.Add(lblSelectedProject);
            grpSetup.Controls.Add(lblProject);
            grpSetup.Controls.Add(lblProtocol);
            grpSetup.Controls.Add(lblProtocolValue);
            grpSetup.Controls.Add(btnProtocolSetup);
            grpSetup.Controls.Add(lblDevice);
            grpSetup.Controls.Add(lblDeviceValue);
            grpSetup.Controls.Add(btnDeviceSetup);
            grpSetup.Controls.Add(lblStatusTitle);
            grpSetup.Controls.Add(lblStatus);
            grpSetup.Controls.Add(btnConnect);
            grpSetup.Dock = DockStyle.Fill;
            grpSetup.Location = new Point(3, 3);
            grpSetup.Name = "grpSetup";
            grpSetup.Size = new Size(364, 122);
            grpSetup.TabIndex = 35;
            grpSetup.TabStop = false;
            grpSetup.Text = "Setup";
            // 
            // lblSelectedProject
            // 
            lblSelectedProject.Font = new Font("맑은 고딕", 9F);
            lblSelectedProject.Location = new Point(64, 22);
            lblSelectedProject.Name = "lblSelectedProject";
            lblSelectedProject.Size = new Size(179, 15);
            lblSelectedProject.TabIndex = 11;
            lblSelectedProject.Text = "(Not set)";
            // 
            // lblProtocol
            // 
            lblProtocol.AutoSize = true;
            lblProtocol.Location = new Point(6, 46);
            lblProtocol.Name = "lblProtocol";
            lblProtocol.Size = new Size(52, 15);
            lblProtocol.TabIndex = 2;
            lblProtocol.Text = "Protocol";
            // 
            // lblProtocolValue
            // 
            lblProtocolValue.Font = new Font("맑은 고딕", 9F);
            lblProtocolValue.Location = new Point(64, 46);
            lblProtocolValue.Name = "lblProtocolValue";
            lblProtocolValue.Size = new Size(179, 15);
            lblProtocolValue.TabIndex = 3;
            lblProtocolValue.Text = "(Not set)";
            // 
            // lblDevice
            // 
            lblDevice.AutoSize = true;
            lblDevice.Location = new Point(6, 71);
            lblDevice.Name = "lblDevice";
            lblDevice.Size = new Size(43, 15);
            lblDevice.TabIndex = 5;
            lblDevice.Text = "Device";
            // 
            // lblDeviceValue
            // 
            lblDeviceValue.Font = new Font("맑은 고딕", 9F);
            lblDeviceValue.Location = new Point(64, 71);
            lblDeviceValue.Name = "lblDeviceValue";
            lblDeviceValue.Size = new Size(179, 15);
            lblDeviceValue.TabIndex = 6;
            lblDeviceValue.Text = "(Not set)";
            // 
            // lblStatusTitle
            // 
            lblStatusTitle.AutoSize = true;
            lblStatusTitle.Location = new Point(6, 96);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Size = new Size(40, 15);
            lblStatusTitle.TabIndex = 8;
            lblStatusTitle.Text = "Status";
            // 
            // grpRegMap
            // 
            grpRegMap.Controls.Add(tlpRegMap);
            grpRegMap.Dock = DockStyle.Fill;
            grpRegMap.Location = new Point(3, 131);
            grpRegMap.Name = "grpRegMap";
            grpRegMap.Size = new Size(364, 152);
            grpRegMap.TabIndex = 36;
            grpRegMap.TabStop = false;
            grpRegMap.Text = "Register Map";
            // 
            // tlpRegMap
            // 
            tlpRegMap.ColumnCount = 1;
            tlpRegMap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRegMap.Controls.Add(pnlRegMapButtons, 0, 1);
            tlpRegMap.Controls.Add(clbRegMapSheets, 0, 0);
            tlpRegMap.Dock = DockStyle.Fill;
            tlpRegMap.Location = new Point(3, 19);
            tlpRegMap.Name = "tlpRegMap";
            tlpRegMap.RowCount = 2;
            tlpRegMap.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRegMap.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tlpRegMap.Size = new Size(358, 130);
            tlpRegMap.TabIndex = 24;
            // 
            // pnlRegMapButtons
            // 
            pnlRegMapButtons.Controls.Add(btnSelectMapFile);
            pnlRegMapButtons.Controls.Add(btnOpenMapPath);
            pnlRegMapButtons.Controls.Add(lblMapFileName);
            pnlRegMapButtons.Controls.Add(btnLoadSelectedSheets);
            pnlRegMapButtons.Dock = DockStyle.Fill;
            pnlRegMapButtons.Location = new Point(3, 77);
            pnlRegMapButtons.Name = "pnlRegMapButtons";
            pnlRegMapButtons.Size = new Size(352, 50);
            pnlRegMapButtons.TabIndex = 25;
            // 
            // btnOpenMapPath
            // 
            btnOpenMapPath.Location = new Point(297, 25);
            btnOpenMapPath.Name = "btnOpenMapPath";
            btnOpenMapPath.Size = new Size(50, 23);
            btnOpenMapPath.TabIndex = 23;
            btnOpenMapPath.Text = "Path";
            btnOpenMapPath.UseVisualStyleBackColor = true;
            btnOpenMapPath.Click += btnOpenMapPath_Click;
            // 
            // lblMapFileName
            // 
            lblMapFileName.Location = new Point(3, 29);
            lblMapFileName.Name = "lblMapFileName";
            lblMapFileName.Size = new Size(297, 15);
            lblMapFileName.TabIndex = 22;
            lblMapFileName.Text = "(No file)";
            // 
            // grpRegControl
            // 
            grpRegControl.Controls.Add(numRegIndex);
            grpRegControl.Controls.Add(txtRegValueHex);
            grpRegControl.Controls.Add(lblRegName);
            grpRegControl.Controls.Add(flpBitsTop);
            grpRegControl.Controls.Add(flpBitsBottom);
            grpRegControl.Controls.Add(lblRegAddrSummary);
            grpRegControl.Controls.Add(lblRegResetSummary);
            grpRegControl.Controls.Add(btnWrite);
            grpRegControl.Controls.Add(btnWriteAll);
            grpRegControl.Controls.Add(btnReadAll);
            grpRegControl.Controls.Add(btnRead);
            grpRegControl.Dock = DockStyle.Fill;
            grpRegControl.Location = new Point(3, 289);
            grpRegControl.Name = "grpRegControl";
            grpRegControl.Size = new Size(364, 175);
            grpRegControl.TabIndex = 37;
            grpRegControl.TabStop = false;
            grpRegControl.Text = "Register Control";
            // 
            // numRegIndex
            // 
            numRegIndex.Location = new Point(183, 17);
            numRegIndex.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numRegIndex.Name = "numRegIndex";
            numRegIndex.Size = new Size(60, 23);
            numRegIndex.TabIndex = 0;
            // 
            // txtRegValueHex
            // 
            txtRegValueHex.Location = new Point(244, 17);
            txtRegValueHex.Name = "txtRegValueHex";
            txtRegValueHex.Size = new Size(109, 23);
            txtRegValueHex.TabIndex = 1;
            txtRegValueHex.Text = "0x00000000";
            // 
            // lblRegName
            // 
            lblRegName.Location = new Point(3, 19);
            lblRegName.Name = "lblRegName";
            lblRegName.Size = new Size(174, 20);
            lblRegName.TabIndex = 2;
            lblRegName.Text = "(No Register)";
            // 
            // flpBitsTop
            // 
            flpBitsTop.Location = new Point(3, 46);
            flpBitsTop.Name = "flpBitsTop";
            flpBitsTop.Size = new Size(358, 27);
            flpBitsTop.TabIndex = 3;
            flpBitsTop.WrapContents = false;
            // 
            // flpBitsBottom
            // 
            flpBitsBottom.Location = new Point(3, 79);
            flpBitsBottom.Name = "flpBitsBottom";
            flpBitsBottom.Size = new Size(358, 27);
            flpBitsBottom.TabIndex = 4;
            flpBitsBottom.WrapContents = false;
            // 
            // lblRegAddrSummary
            // 
            lblRegAddrSummary.AutoSize = true;
            lblRegAddrSummary.Location = new Point(3, 109);
            lblRegAddrSummary.Name = "lblRegAddrSummary";
            lblRegAddrSummary.Size = new Size(61, 15);
            lblRegAddrSummary.TabIndex = 3;
            lblRegAddrSummary.Text = "Address: -";
            // 
            // lblRegResetSummary
            // 
            lblRegResetSummary.AutoSize = true;
            lblRegResetSummary.Location = new Point(3, 127);
            lblRegResetSummary.Name = "lblRegResetSummary";
            lblRegResetSummary.Size = new Size(81, 15);
            lblRegResetSummary.TabIndex = 4;
            lblRegResetSummary.Text = "Reset Value: -";
            // 
            // btnWrite
            // 
            btnWrite.Location = new Point(3, 146);
            btnWrite.Name = "btnWrite";
            btnWrite.Size = new Size(82, 23);
            btnWrite.TabIndex = 5;
            btnWrite.Text = "Write";
            btnWrite.UseVisualStyleBackColor = true;
            btnWrite.Click += btnWrite_Click;
            // 
            // btnWriteAll
            // 
            btnWriteAll.Location = new Point(91, 146);
            btnWriteAll.Name = "btnWriteAll";
            btnWriteAll.Size = new Size(82, 23);
            btnWriteAll.TabIndex = 6;
            btnWriteAll.Text = "Write All";
            btnWriteAll.UseVisualStyleBackColor = true;
            // 
            // btnReadAll
            // 
            btnReadAll.Location = new Point(271, 146);
            btnReadAll.Name = "btnReadAll";
            btnReadAll.Size = new Size(82, 23);
            btnReadAll.TabIndex = 8;
            btnReadAll.Text = "Read All";
            btnReadAll.UseVisualStyleBackColor = true;
            // 
            // grpRegLog
            // 
            grpRegLog.Controls.Add(dgvRegLog);
            grpRegLog.Dock = DockStyle.Fill;
            grpRegLog.Location = new Point(3, 470);
            grpRegLog.Name = "grpRegLog";
            grpRegLog.Size = new Size(364, 395);
            grpRegLog.TabIndex = 38;
            grpRegLog.TabStop = false;
            grpRegLog.Text = "Register Control Log";
            // 
            // grpRegTree
            // 
            grpRegTree.Controls.Add(tlpRegTree);
            grpRegTree.Dock = DockStyle.Fill;
            grpRegTree.Location = new Point(3, 3);
            grpRegTree.Name = "grpRegTree";
            grpRegTree.Size = new Size(424, 522);
            grpRegTree.TabIndex = 40;
            grpRegTree.TabStop = false;
            grpRegTree.Text = "Register Tree";
            // 
            // tlpRegTree
            // 
            tlpRegTree.ColumnCount = 1;
            tlpRegTree.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRegTree.Controls.Add(tvRegTree, 0, 0);
            tlpRegTree.Controls.Add(pnlScriptButtons, 0, 1);
            tlpRegTree.Dock = DockStyle.Fill;
            tlpRegTree.Location = new Point(3, 19);
            tlpRegTree.Name = "tlpRegTree";
            tlpRegTree.RowCount = 2;
            tlpRegTree.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRegTree.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tlpRegTree.Size = new Size(418, 500);
            tlpRegTree.TabIndex = 27;
            // 
            // pnlScriptButtons
            // 
            pnlScriptButtons.Controls.Add(btnSaveScript);
            pnlScriptButtons.Controls.Add(btnOpenScriptPath);
            pnlScriptButtons.Controls.Add(btnLoadScript);
            pnlScriptButtons.Controls.Add(lblScriptFileName);
            pnlScriptButtons.Dock = DockStyle.Fill;
            pnlScriptButtons.Location = new Point(3, 467);
            pnlScriptButtons.Name = "pnlScriptButtons";
            pnlScriptButtons.Size = new Size(412, 30);
            pnlScriptButtons.TabIndex = 23;
            // 
            // splMain
            // 
            splMain.Dock = DockStyle.Fill;
            splMain.FixedPanel = FixedPanel.Panel2;
            splMain.IsSplitterFixed = true;
            splMain.Location = new Point(0, 0);
            splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            splMain.Panel1.Controls.Add(tlpLeft);
            // 
            // splMain.Panel2
            // 
            splMain.Panel2.Controls.Add(tlpRight);
            splMain.Panel2MinSize = 370;
            splMain.Size = new Size(804, 961);
            splMain.SplitterDistance = 430;
            splMain.TabIndex = 5;
            // 
            // tlpLeft
            // 
            tlpLeft.ColumnCount = 1;
            tlpLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 97.441864F));
            tlpLeft.Controls.Add(grpRunTest, 0, 1);
            tlpLeft.Controls.Add(grpRegTree, 0, 0);
            tlpLeft.Dock = DockStyle.Fill;
            tlpLeft.Location = new Point(0, 0);
            tlpLeft.Name = "tlpLeft";
            tlpLeft.RowCount = 2;
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpLeft.Size = new Size(430, 961);
            tlpLeft.TabIndex = 41;
            // 
            // grpRunTest
            // 
            grpRunTest.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpRunTest.Controls.Add(tlpRunTest);
            grpRunTest.Dock = DockStyle.Fill;
            grpRunTest.Location = new Point(3, 531);
            grpRunTest.Name = "grpRunTest";
            grpRunTest.Size = new Size(424, 427);
            grpRunTest.TabIndex = 41;
            grpRunTest.TabStop = false;
            grpRunTest.Text = "Test Log";
            // 
            // tlpRunTest
            // 
            tlpRunTest.ColumnCount = 1;
            tlpRunTest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRunTest.Controls.Add(panelTestLog, 0, 0);
            tlpRunTest.Controls.Add(panelProgress, 0, 1);
            tlpRunTest.Dock = DockStyle.Fill;
            tlpRunTest.Location = new Point(3, 19);
            tlpRunTest.Name = "tlpRunTest";
            tlpRunTest.RowCount = 2;
            tlpRunTest.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRunTest.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tlpRunTest.Size = new Size(418, 405);
            tlpRunTest.TabIndex = 24;
            // 
            // panelTestLog
            // 
            panelTestLog.Controls.Add(rtbRunTestLog);
            panelTestLog.Dock = DockStyle.Fill;
            panelTestLog.Location = new Point(3, 3);
            panelTestLog.Name = "panelTestLog";
            panelTestLog.Size = new Size(412, 377);
            panelTestLog.TabIndex = 24;
            // 
            // rtbRunTestLog
            // 
            rtbRunTestLog.BackColor = Color.White;
            rtbRunTestLog.Dock = DockStyle.Fill;
            rtbRunTestLog.Font = new Font("Consolas", 9F);
            rtbRunTestLog.ForeColor = Color.Black;
            rtbRunTestLog.Location = new Point(0, 0);
            rtbRunTestLog.Name = "rtbRunTestLog";
            rtbRunTestLog.ReadOnly = true;
            rtbRunTestLog.Size = new Size(412, 377);
            rtbRunTestLog.TabIndex = 4;
            rtbRunTestLog.Text = "";
            // 
            // panelProgress
            // 
            panelProgress.Controls.Add(probarRuntest);
            panelProgress.Dock = DockStyle.Bottom;
            panelProgress.Location = new Point(3, 386);
            panelProgress.Name = "panelProgress";
            panelProgress.Size = new Size(412, 16);
            panelProgress.TabIndex = 24;
            // 
            // probarRuntest
            // 
            probarRuntest.Dock = DockStyle.Fill;
            probarRuntest.Location = new Point(0, 0);
            probarRuntest.Name = "probarRuntest";
            probarRuntest.Size = new Size(412, 16);
            probarRuntest.TabIndex = 5;
            // 
            // tlpRight
            // 
            tlpRight.ColumnCount = 1;
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpRight.Controls.Add(pnlRunTestButtons, 0, 4);
            tlpRight.Controls.Add(grpSetup, 0, 0);
            tlpRight.Controls.Add(grpRegMap, 0, 1);
            tlpRight.Controls.Add(grpRegControl, 0, 2);
            tlpRight.Controls.Add(grpRegLog, 0, 3);
            tlpRight.Dock = DockStyle.Fill;
            tlpRight.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tlpRight.Location = new Point(0, 0);
            tlpRight.Name = "tlpRight";
            tlpRight.RowCount = 5;
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 38.4105949F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 181F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 401F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 93F));
            tlpRight.Size = new Size(370, 961);
            tlpRight.TabIndex = 42;
            // 
            // pnlRunTestButtons
            // 
            pnlRunTestButtons.Controls.Add(btnTestSlot06);
            pnlRunTestButtons.Controls.Add(btnTestSlot07);
            pnlRunTestButtons.Controls.Add(btnTestSlot08);
            pnlRunTestButtons.Controls.Add(btnTestSlot09);
            pnlRunTestButtons.Controls.Add(btnTestSlot10);
            pnlRunTestButtons.Controls.Add(cmbTestCategory);
            pnlRunTestButtons.Controls.Add(cmbTest);
            pnlRunTestButtons.Controls.Add(btnRunTest);
            pnlRunTestButtons.Controls.Add(btnStopTest);
            pnlRunTestButtons.Controls.Add(btnTestSlot01);
            pnlRunTestButtons.Controls.Add(btnTestSlot04);
            pnlRunTestButtons.Controls.Add(btnTestSlot05);
            pnlRunTestButtons.Controls.Add(btnTestSlot02);
            pnlRunTestButtons.Controls.Add(btnTestSlot03);
            pnlRunTestButtons.Dock = DockStyle.Fill;
            pnlRunTestButtons.Location = new Point(3, 871);
            pnlRunTestButtons.Name = "pnlRunTestButtons";
            pnlRunTestButtons.Size = new Size(364, 87);
            pnlRunTestButtons.TabIndex = 5;
            // 
            // btnTestSlot06
            // 
            btnTestSlot06.Location = new Point(5, 58);
            btnTestSlot06.Margin = new Padding(2);
            btnTestSlot06.Name = "btnTestSlot06";
            btnTestSlot06.Size = new Size(68, 23);
            btnTestSlot06.TabIndex = 20;
            btnTestSlot06.Text = "TEST";
            btnTestSlot06.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot07
            // 
            btnTestSlot07.Location = new Point(221, 58);
            btnTestSlot07.Margin = new Padding(2);
            btnTestSlot07.Name = "btnTestSlot07";
            btnTestSlot07.Size = new Size(68, 23);
            btnTestSlot07.TabIndex = 19;
            btnTestSlot07.Text = "TEST";
            btnTestSlot07.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot08
            // 
            btnTestSlot08.Location = new Point(293, 58);
            btnTestSlot08.Margin = new Padding(2);
            btnTestSlot08.Name = "btnTestSlot08";
            btnTestSlot08.Size = new Size(68, 23);
            btnTestSlot08.TabIndex = 23;
            btnTestSlot08.Text = "TEST";
            btnTestSlot08.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot09
            // 
            btnTestSlot09.Location = new Point(77, 59);
            btnTestSlot09.Margin = new Padding(2);
            btnTestSlot09.Name = "btnTestSlot09";
            btnTestSlot09.Size = new Size(68, 23);
            btnTestSlot09.TabIndex = 21;
            btnTestSlot09.Text = "TEST";
            btnTestSlot09.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot10
            // 
            btnTestSlot10.Location = new Point(149, 59);
            btnTestSlot10.Margin = new Padding(2);
            btnTestSlot10.Name = "btnTestSlot10";
            btnTestSlot10.Size = new Size(68, 23);
            btnTestSlot10.TabIndex = 22;
            btnTestSlot10.Text = "TEST";
            btnTestSlot10.UseVisualStyleBackColor = true;
            // 
            // cmbTestCategory
            // 
            cmbTestCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTestCategory.Location = new Point(3, 3);
            cmbTestCategory.Name = "cmbTestCategory";
            cmbTestCategory.Size = new Size(79, 23);
            cmbTestCategory.TabIndex = 0;
            // 
            // cmbTest
            // 
            cmbTest.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTest.Location = new Point(86, 3);
            cmbTest.Name = "cmbTest";
            cmbTest.Size = new Size(126, 23);
            cmbTest.TabIndex = 1;
            // 
            // btnRunTest
            // 
            btnRunTest.Location = new Point(218, 3);
            btnRunTest.Name = "btnRunTest";
            btnRunTest.Size = new Size(73, 23);
            btnRunTest.TabIndex = 2;
            btnRunTest.Text = "Run Test";
            btnRunTest.UseVisualStyleBackColor = true;
            // 
            // btnStopTest
            // 
            btnStopTest.Location = new Point(297, 3);
            btnStopTest.Name = "btnStopTest";
            btnStopTest.Size = new Size(64, 23);
            btnStopTest.TabIndex = 3;
            btnStopTest.Text = "Stop";
            btnStopTest.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot01
            // 
            btnTestSlot01.Location = new Point(5, 31);
            btnTestSlot01.Margin = new Padding(2);
            btnTestSlot01.Name = "btnTestSlot01";
            btnTestSlot01.Size = new Size(68, 23);
            btnTestSlot01.TabIndex = 15;
            btnTestSlot01.Text = "TEST";
            btnTestSlot01.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot04
            // 
            btnTestSlot04.Location = new Point(221, 31);
            btnTestSlot04.Margin = new Padding(2);
            btnTestSlot04.Name = "btnTestSlot04";
            btnTestSlot04.Size = new Size(68, 23);
            btnTestSlot04.TabIndex = 14;
            btnTestSlot04.Text = "TEST";
            btnTestSlot04.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot05
            // 
            btnTestSlot05.Location = new Point(293, 31);
            btnTestSlot05.Margin = new Padding(2);
            btnTestSlot05.Name = "btnTestSlot05";
            btnTestSlot05.Size = new Size(68, 23);
            btnTestSlot05.TabIndex = 18;
            btnTestSlot05.Text = "TEST";
            btnTestSlot05.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot02
            // 
            btnTestSlot02.Location = new Point(77, 31);
            btnTestSlot02.Margin = new Padding(2);
            btnTestSlot02.Name = "btnTestSlot02";
            btnTestSlot02.Size = new Size(68, 23);
            btnTestSlot02.TabIndex = 16;
            btnTestSlot02.Text = "TEST";
            btnTestSlot02.UseVisualStyleBackColor = true;
            // 
            // btnTestSlot03
            // 
            btnTestSlot03.Location = new Point(149, 31);
            btnTestSlot03.Margin = new Padding(2);
            btnTestSlot03.Name = "btnTestSlot03";
            btnTestSlot03.Size = new Size(68, 23);
            btnTestSlot03.TabIndex = 17;
            btnTestSlot03.Text = "TEST";
            btnTestSlot03.UseVisualStyleBackColor = true;
            // 
            // RegisterControlForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(804, 961);
            Controls.Add(splMain);
            MinimumSize = new Size(700, 800);
            Name = "RegisterControlForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Register Controller";
            ((System.ComponentModel.ISupportInitialize)dgvRegLog).EndInit();
            grpSetup.ResumeLayout(false);
            grpSetup.PerformLayout();
            grpRegMap.ResumeLayout(false);
            tlpRegMap.ResumeLayout(false);
            pnlRegMapButtons.ResumeLayout(false);
            grpRegControl.ResumeLayout(false);
            grpRegControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numRegIndex).EndInit();
            grpRegLog.ResumeLayout(false);
            grpRegTree.ResumeLayout(false);
            tlpRegTree.ResumeLayout(false);
            pnlScriptButtons.ResumeLayout(false);
            splMain.Panel1.ResumeLayout(false);
            splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
            splMain.ResumeLayout(false);
            tlpLeft.ResumeLayout(false);
            grpRunTest.ResumeLayout(false);
            tlpRunTest.ResumeLayout(false);
            panelTestLog.ResumeLayout(false);
            panelProgress.ResumeLayout(false);
            tlpRight.ResumeLayout(false);
            pnlRunTestButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private SplitContainer splMain;
        private TableLayoutPanel tlpLeft;
        private TableLayoutPanel tlpRight;
        private Button btnTestSlot05;
        private Button btnTestSlot03;
        private Button btnTestSlot02;
        private Button btnTestSlot01;
        private Button btnTestSlot04;
        private TableLayoutPanel tlpRegTree;
        private Panel pnlScriptButtons;
        private TableLayoutPanel tlpRegMap;
        private Panel pnlRegMapButtons;
        private TableLayoutPanel tlpRunTest;
        private Panel pnlRunTestButtons;
        private Button btnTestSlot06;
        private Button btnTestSlot07;
        private Button btnTestSlot08;
        private Button btnTestSlot09;
        private Button btnTestSlot10;
        private RichTextBox rtbRunTestLog;
        private Label lblSelectedProject;
        private ProgressBar probarRuntest;
        private Panel panelProgress;
        private Panel panelTestLog;
    }
}
