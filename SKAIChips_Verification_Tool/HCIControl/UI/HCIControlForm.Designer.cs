namespace SKAIChips_Verification_Tool.HCIControl
{
    partial class HCIControlForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            groupBox1 = new GroupBox();
            PortCloseButton = new Button();
            PortRefreshButton = new Button();
            PortOpenButton = new Button();
            PortTabControl = new TabControl();
            PortSerialTabPage = new TabPage();
            label2 = new Label();
            PortBaudRateComboBox = new ComboBox();
            SerialPortComboBox = new ComboBox();
            label1 = new Label();
            PortUSBtabPage = new TabPage();
            label3 = new Label();
            USBDevicesComboBox = new ComboBox();
            HCISplitContainer = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            label4 = new Label();
            HCICommandTreeView = new TreeView();
            tableLayoutPanel2 = new TableLayoutPanel();
            HciParaGroupBox = new GroupBox();
            HCIParaNameLabel = new Label();
            SendHciCommandButton = new Button();
            HCIParaDataGridView = new DataGridView();
            tabControl1 = new TabControl();
            HciLogTabPage = new TabPage();
            SaveLogButton = new Button();
            HciLogDataGridView = new DataGridView();
            ClearLogButton = new Button();
            HCIScriptTabPage = new TabPage();
            tableLayoutPanel3 = new TableLayoutPanel();
            HCIScriptComboBox = new ComboBox();
            HCIScriptTreeView = new TreeView();
            RunHciScriptButton = new Button();
            tableLayoutPanel4 = new TableLayoutPanel();
            ClearHciScriptButton = new Button();
            SaveAsHciScriptButton = new Button();
            SaveHciScriptButton = new Button();
            DeleteHciScriptButton = new Button();
            InsertHciScriptButton = new Button();
            AddHciScriptButton = new Button();
            ChangeScrDirButton = new Button();
            TreeContextMenuStrip = new ContextMenuStrip(components);
            sendCommandToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            addScriptToolStripMenuItem = new ToolStripMenuItem();
            insertScriptToolStripMenuItem = new ToolStripMenuItem();
            HCIScriptContextMenuStrip = new ContextMenuStrip(components);
            sendCommandToolStripMenuItem1 = new ToolStripMenuItem();
            runScriptToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            changeScriptDirToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            BaseTableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            tabControl_HciControl = new TabControl();
            tabPage_Connection = new TabPage();
            dataGridView_AdvReports = new DataGridView();
            Column_BDAddress = new DataGridViewTextBoxColumn();
            Column_RSSI = new DataGridViewTextBoxColumn();
            Column_EventType = new DataGridViewTextBoxColumn();
            Column_AddrType = new DataGridViewTextBoxColumn();
            Column_ConnHandle = new DataGridViewTextBoxColumn();
            Column_Data = new DataGridViewTextBoxColumn();
            tabPage_Test = new TabPage();
            button_RunModTest = new Button();
            groupBox1.SuspendLayout();
            PortTabControl.SuspendLayout();
            PortSerialTabPage.SuspendLayout();
            PortUSBtabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HCISplitContainer).BeginInit();
            HCISplitContainer.Panel1.SuspendLayout();
            HCISplitContainer.Panel2.SuspendLayout();
            HCISplitContainer.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            HciParaGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HCIParaDataGridView).BeginInit();
            tabControl1.SuspendLayout();
            HciLogTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HciLogDataGridView).BeginInit();
            HCIScriptTabPage.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            TreeContextMenuStrip.SuspendLayout();
            HCIScriptContextMenuStrip.SuspendLayout();
            BaseTableLayoutPanel.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            tabControl_HciControl.SuspendLayout();
            tabPage_Connection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView_AdvReports).BeginInit();
            tabPage_Test.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(PortCloseButton);
            groupBox1.Controls.Add(PortRefreshButton);
            groupBox1.Controls.Add(PortOpenButton);
            groupBox1.Controls.Add(PortTabControl);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(274, 104);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "1. Connection";
            // 
            // PortCloseButton
            // 
            PortCloseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PortCloseButton.Location = new Point(198, 74);
            PortCloseButton.Name = "PortCloseButton";
            PortCloseButton.Size = new Size(70, 24);
            PortCloseButton.TabIndex = 3;
            PortCloseButton.Text = "Close";
            PortCloseButton.UseVisualStyleBackColor = true;
            PortCloseButton.Click += PortCloseButton_Click;
            // 
            // PortRefreshButton
            // 
            PortRefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PortRefreshButton.Location = new Point(198, 18);
            PortRefreshButton.Name = "PortRefreshButton";
            PortRefreshButton.Size = new Size(70, 24);
            PortRefreshButton.TabIndex = 2;
            PortRefreshButton.Text = "Refresh";
            PortRefreshButton.UseVisualStyleBackColor = true;
            PortRefreshButton.Click += PortRefreshButton_Click;
            // 
            // PortOpenButton
            // 
            PortOpenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PortOpenButton.Location = new Point(198, 48);
            PortOpenButton.Name = "PortOpenButton";
            PortOpenButton.Size = new Size(70, 24);
            PortOpenButton.TabIndex = 1;
            PortOpenButton.Text = "Open";
            PortOpenButton.UseVisualStyleBackColor = true;
            PortOpenButton.Click += PortOpenButton_Click;
            // 
            // PortTabControl
            // 
            PortTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PortTabControl.Controls.Add(PortSerialTabPage);
            PortTabControl.Controls.Add(PortUSBtabPage);
            PortTabControl.Location = new Point(6, 18);
            PortTabControl.Name = "PortTabControl";
            PortTabControl.SelectedIndex = 0;
            PortTabControl.Size = new Size(186, 81);
            PortTabControl.TabIndex = 0;
            // 
            // PortSerialTabPage
            // 
            PortSerialTabPage.Controls.Add(label2);
            PortSerialTabPage.Controls.Add(PortBaudRateComboBox);
            PortSerialTabPage.Controls.Add(SerialPortComboBox);
            PortSerialTabPage.Controls.Add(label1);
            PortSerialTabPage.Location = new Point(4, 24);
            PortSerialTabPage.Name = "PortSerialTabPage";
            PortSerialTabPage.Padding = new Padding(3);
            PortSerialTabPage.Size = new Size(178, 53);
            PortSerialTabPage.TabIndex = 0;
            PortSerialTabPage.Text = "UART";
            PortSerialTabPage.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 32);
            label2.Name = "label2";
            label2.Size = new Size(71, 15);
            label2.TabIndex = 4;
            label2.Text = "Baud Rate :";
            // 
            // PortBaudRateComboBox
            // 
            PortBaudRateComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PortBaudRateComboBox.FormattingEnabled = true;
            PortBaudRateComboBox.Location = new Point(81, 29);
            PortBaudRateComboBox.Name = "PortBaudRateComboBox";
            PortBaudRateComboBox.Size = new Size(94, 23);
            PortBaudRateComboBox.TabIndex = 3;
            // 
            // SerialPortComboBox
            // 
            SerialPortComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SerialPortComboBox.FormattingEnabled = true;
            SerialPortComboBox.Location = new Point(81, 3);
            SerialPortComboBox.Name = "SerialPortComboBox";
            SerialPortComboBox.Size = new Size(94, 23);
            SerialPortComboBox.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 2;
            label1.Text = "Select Port :";
            // 
            // PortUSBtabPage
            // 
            PortUSBtabPage.Controls.Add(label3);
            PortUSBtabPage.Controls.Add(USBDevicesComboBox);
            PortUSBtabPage.Location = new Point(4, 24);
            PortUSBtabPage.Name = "PortUSBtabPage";
            PortUSBtabPage.Padding = new Padding(3);
            PortUSBtabPage.Size = new Size(178, 53);
            PortUSBtabPage.TabIndex = 1;
            PortUSBtabPage.Text = "USB";
            PortUSBtabPage.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 5);
            label3.Name = "label3";
            label3.Size = new Size(115, 15);
            label3.TabIndex = 3;
            label3.Text = "Select USB Device :";
            // 
            // USBDevicesComboBox
            // 
            USBDevicesComboBox.Dock = DockStyle.Bottom;
            USBDevicesComboBox.FormattingEnabled = true;
            USBDevicesComboBox.Location = new Point(3, 27);
            USBDevicesComboBox.Name = "USBDevicesComboBox";
            USBDevicesComboBox.Size = new Size(172, 23);
            USBDevicesComboBox.TabIndex = 2;
            // 
            // HCISplitContainer
            // 
            HCISplitContainer.BorderStyle = BorderStyle.Fixed3D;
            HCISplitContainer.Dock = DockStyle.Fill;
            HCISplitContainer.Location = new Point(3, 113);
            HCISplitContainer.Name = "HCISplitContainer";
            // 
            // HCISplitContainer.Panel1
            // 
            HCISplitContainer.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // HCISplitContainer.Panel2
            // 
            HCISplitContainer.Panel2.Controls.Add(tableLayoutPanel2);
            HCISplitContainer.Size = new Size(578, 446);
            HCISplitContainer.SplitterDistance = 247;
            HCISplitContainer.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(HCICommandTreeView, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(243, 442);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Bottom;
            label4.Location = new Point(3, 5);
            label4.Name = "label4";
            label4.Size = new Size(237, 15);
            label4.TabIndex = 2;
            label4.Text = "HCI Commands Tree";
            // 
            // HCICommandTreeView
            // 
            HCICommandTreeView.Dock = DockStyle.Fill;
            HCICommandTreeView.HideSelection = false;
            HCICommandTreeView.Location = new Point(3, 23);
            HCICommandTreeView.Name = "HCICommandTreeView";
            HCICommandTreeView.Size = new Size(237, 396);
            HCICommandTreeView.TabIndex = 1;
            HCICommandTreeView.AfterSelect += HCICommandTreeView_AfterSelect;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(HciParaGroupBox, 0, 0);
            tableLayoutPanel2.Controls.Add(tabControl1, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(323, 442);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // HciParaGroupBox
            // 
            HciParaGroupBox.Controls.Add(HCIParaNameLabel);
            HciParaGroupBox.Controls.Add(SendHciCommandButton);
            HciParaGroupBox.Controls.Add(HCIParaDataGridView);
            HciParaGroupBox.Dock = DockStyle.Fill;
            HciParaGroupBox.Location = new Point(3, 3);
            HciParaGroupBox.Name = "HciParaGroupBox";
            HciParaGroupBox.Size = new Size(317, 89);
            HciParaGroupBox.TabIndex = 2;
            HciParaGroupBox.TabStop = false;
            HciParaGroupBox.Text = "Set HCI Command Parameter";
            // 
            // HCIParaNameLabel
            // 
            HCIParaNameLabel.AutoSize = true;
            HCIParaNameLabel.Location = new Point(3, 25);
            HCIParaNameLabel.Name = "HCIParaNameLabel";
            HCIParaNameLabel.Size = new Size(149, 15);
            HCIParaNameLabel.TabIndex = 5;
            HCIParaNameLabel.Text = "HCI Command Parameter";
            // 
            // SendHciCommandButton
            // 
            SendHciCommandButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SendHciCommandButton.Location = new Point(201, 16);
            SendHciCommandButton.Name = "SendHciCommandButton";
            SendHciCommandButton.Size = new Size(110, 24);
            SendHciCommandButton.TabIndex = 4;
            SendHciCommandButton.Text = "Send Command";
            SendHciCommandButton.UseVisualStyleBackColor = true;
            SendHciCommandButton.Click += SendHciCommandButton_Click;
            // 
            // HCIParaDataGridView
            // 
            HCIParaDataGridView.AllowUserToAddRows = false;
            HCIParaDataGridView.AllowUserToResizeColumns = false;
            HCIParaDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.BottomCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            HCIParaDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            HCIParaDataGridView.ColumnHeadersHeight = 21;
            HCIParaDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            HCIParaDataGridView.Dock = DockStyle.Bottom;
            HCIParaDataGridView.Location = new Point(3, 45);
            HCIParaDataGridView.Name = "HCIParaDataGridView";
            HCIParaDataGridView.RowHeadersWidth = 5;
            HCIParaDataGridView.RowTemplate.Height = 18;
            HCIParaDataGridView.ScrollBars = ScrollBars.None;
            HCIParaDataGridView.Size = new Size(311, 41);
            HCIParaDataGridView.TabIndex = 0;
            HCIParaDataGridView.CellEndEdit += HCIParaDataGridView_CellEndEdit;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(HciLogTabPage);
            tabControl1.Controls.Add(HCIScriptTabPage);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(3, 98);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(317, 341);
            tabControl1.TabIndex = 3;
            // 
            // HciLogTabPage
            // 
            HciLogTabPage.Controls.Add(SaveLogButton);
            HciLogTabPage.Controls.Add(HciLogDataGridView);
            HciLogTabPage.Controls.Add(ClearLogButton);
            HciLogTabPage.Location = new Point(4, 24);
            HciLogTabPage.Name = "HciLogTabPage";
            HciLogTabPage.Padding = new Padding(3);
            HciLogTabPage.Size = new Size(309, 313);
            HciLogTabPage.TabIndex = 0;
            HciLogTabPage.Text = "HCI Log";
            HciLogTabPage.UseVisualStyleBackColor = true;
            // 
            // SaveLogButton
            // 
            SaveLogButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            SaveLogButton.Location = new Point(83, 288);
            SaveLogButton.Name = "SaveLogButton";
            SaveLogButton.Size = new Size(77, 24);
            SaveLogButton.TabIndex = 7;
            SaveLogButton.Text = "Save";
            SaveLogButton.UseVisualStyleBackColor = true;
            SaveLogButton.Click += SaveLogButton_Click;
            // 
            // HciLogDataGridView
            // 
            HciLogDataGridView.AllowUserToAddRows = false;
            HciLogDataGridView.AllowUserToResizeRows = false;
            HciLogDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            HciLogDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            HciLogDataGridView.Location = new Point(0, 0);
            HciLogDataGridView.Name = "HciLogDataGridView";
            HciLogDataGridView.RowHeadersWidth = 5;
            HciLogDataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            HciLogDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
            HciLogDataGridView.RowTemplate.Height = 15;
            HciLogDataGridView.Size = new Size(309, 285);
            HciLogDataGridView.TabIndex = 6;
            // 
            // ClearLogButton
            // 
            ClearLogButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ClearLogButton.Location = new Point(0, 288);
            ClearLogButton.Name = "ClearLogButton";
            ClearLogButton.Size = new Size(77, 24);
            ClearLogButton.TabIndex = 5;
            ClearLogButton.Text = "Clear";
            ClearLogButton.UseVisualStyleBackColor = true;
            ClearLogButton.Click += ClearLogButton_Click;
            // 
            // HCIScriptTabPage
            // 
            HCIScriptTabPage.Controls.Add(tableLayoutPanel3);
            HCIScriptTabPage.Location = new Point(4, 24);
            HCIScriptTabPage.Name = "HCIScriptTabPage";
            HCIScriptTabPage.Size = new Size(309, 313);
            HCIScriptTabPage.TabIndex = 1;
            HCIScriptTabPage.Text = "HCI Script";
            HCIScriptTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(HCIScriptComboBox, 1, 0);
            tableLayoutPanel3.Controls.Add(HCIScriptTreeView, 1, 1);
            tableLayoutPanel3.Controls.Add(RunHciScriptButton, 0, 0);
            tableLayoutPanel3.Controls.Add(tableLayoutPanel4, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(309, 313);
            tableLayoutPanel3.TabIndex = 11;
            // 
            // HCIScriptComboBox
            // 
            HCIScriptComboBox.Dock = DockStyle.Bottom;
            HCIScriptComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            HCIScriptComboBox.FormattingEnabled = true;
            HCIScriptComboBox.Location = new Point(83, 3);
            HCIScriptComboBox.Name = "HCIScriptComboBox";
            HCIScriptComboBox.Size = new Size(223, 23);
            HCIScriptComboBox.TabIndex = 5;
            HCIScriptComboBox.SelectedIndexChanged += HCIScriptComboBox_SelectedIndexChanged;
            // 
            // HCIScriptTreeView
            // 
            HCIScriptTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            HCIScriptTreeView.HideSelection = false;
            HCIScriptTreeView.Location = new Point(83, 31);
            HCIScriptTreeView.Name = "HCIScriptTreeView";
            HCIScriptTreeView.Size = new Size(223, 279);
            HCIScriptTreeView.TabIndex = 0;
            HCIScriptTreeView.AfterSelect += HCIScriptTreeView_AfterSelect;
            // 
            // RunHciScriptButton
            // 
            RunHciScriptButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            RunHciScriptButton.Location = new Point(0, 4);
            RunHciScriptButton.Margin = new Padding(0);
            RunHciScriptButton.Name = "RunHciScriptButton";
            RunHciScriptButton.Size = new Size(80, 24);
            RunHciScriptButton.TabIndex = 4;
            RunHciScriptButton.Text = "Run Script";
            RunHciScriptButton.UseVisualStyleBackColor = true;
            RunHciScriptButton.Click += runScriptToolStripMenuItem_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(ClearHciScriptButton, 0, 11);
            tableLayoutPanel4.Controls.Add(SaveAsHciScriptButton, 0, 10);
            tableLayoutPanel4.Controls.Add(SaveHciScriptButton, 0, 9);
            tableLayoutPanel4.Controls.Add(DeleteHciScriptButton, 0, 6);
            tableLayoutPanel4.Controls.Add(InsertHciScriptButton, 0, 5);
            tableLayoutPanel4.Controls.Add(AddHciScriptButton, 0, 4);
            tableLayoutPanel4.Controls.Add(ChangeScrDirButton, 0, 8);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(0, 28);
            tableLayoutPanel4.Margin = new Padding(0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 12;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tableLayoutPanel4.Size = new Size(80, 285);
            tableLayoutPanel4.TabIndex = 6;
            // 
            // ClearHciScriptButton
            // 
            ClearHciScriptButton.Location = new Point(0, 260);
            ClearHciScriptButton.Margin = new Padding(0);
            ClearHciScriptButton.Name = "ClearHciScriptButton";
            ClearHciScriptButton.Size = new Size(80, 24);
            ClearHciScriptButton.TabIndex = 10;
            ClearHciScriptButton.Text = "Clear";
            ClearHciScriptButton.UseVisualStyleBackColor = true;
            ClearHciScriptButton.Click += ClearHciScriptButton_Click;
            // 
            // SaveAsHciScriptButton
            // 
            SaveAsHciScriptButton.Location = new Point(0, 236);
            SaveAsHciScriptButton.Margin = new Padding(0);
            SaveAsHciScriptButton.Name = "SaveAsHciScriptButton";
            SaveAsHciScriptButton.Size = new Size(80, 24);
            SaveAsHciScriptButton.TabIndex = 11;
            SaveAsHciScriptButton.Text = "Save As";
            SaveAsHciScriptButton.UseVisualStyleBackColor = true;
            SaveAsHciScriptButton.Click += saveAsToolStripMenuItem_Click;
            // 
            // SaveHciScriptButton
            // 
            SaveHciScriptButton.Location = new Point(0, 212);
            SaveHciScriptButton.Margin = new Padding(0);
            SaveHciScriptButton.Name = "SaveHciScriptButton";
            SaveHciScriptButton.Size = new Size(80, 24);
            SaveHciScriptButton.TabIndex = 6;
            SaveHciScriptButton.Text = "Save";
            SaveHciScriptButton.UseVisualStyleBackColor = true;
            SaveHciScriptButton.Click += saveToolStripMenuItem_Click;
            // 
            // DeleteHciScriptButton
            // 
            DeleteHciScriptButton.Location = new Point(0, 142);
            DeleteHciScriptButton.Margin = new Padding(0);
            DeleteHciScriptButton.Name = "DeleteHciScriptButton";
            DeleteHciScriptButton.Size = new Size(80, 24);
            DeleteHciScriptButton.TabIndex = 9;
            DeleteHciScriptButton.Text = "Delete";
            DeleteHciScriptButton.UseVisualStyleBackColor = true;
            DeleteHciScriptButton.Click += deleteToolStripMenuItem_Click;
            // 
            // InsertHciScriptButton
            // 
            InsertHciScriptButton.Location = new Point(0, 118);
            InsertHciScriptButton.Margin = new Padding(0);
            InsertHciScriptButton.Name = "InsertHciScriptButton";
            InsertHciScriptButton.Size = new Size(80, 24);
            InsertHciScriptButton.TabIndex = 8;
            InsertHciScriptButton.Text = "Insert";
            InsertHciScriptButton.UseVisualStyleBackColor = true;
            InsertHciScriptButton.Click += insertScriptToolStripMenuItem_Click;
            // 
            // AddHciScriptButton
            // 
            AddHciScriptButton.Location = new Point(0, 94);
            AddHciScriptButton.Margin = new Padding(0);
            AddHciScriptButton.Name = "AddHciScriptButton";
            AddHciScriptButton.Size = new Size(80, 24);
            AddHciScriptButton.TabIndex = 7;
            AddHciScriptButton.Text = "Add";
            AddHciScriptButton.UseVisualStyleBackColor = true;
            AddHciScriptButton.Click += addScriptToolStripMenuItem_Click;
            // 
            // ChangeScrDirButton
            // 
            ChangeScrDirButton.Location = new Point(0, 188);
            ChangeScrDirButton.Margin = new Padding(0);
            ChangeScrDirButton.Name = "ChangeScrDirButton";
            ChangeScrDirButton.Size = new Size(80, 24);
            ChangeScrDirButton.TabIndex = 12;
            ChangeScrDirButton.Text = "Change Dir.";
            ChangeScrDirButton.UseVisualStyleBackColor = true;
            ChangeScrDirButton.Click += changeScriptDirToolStripMenuItem_Click;
            // 
            // TreeContextMenuStrip
            // 
            TreeContextMenuStrip.Items.AddRange(new ToolStripItem[] { sendCommandToolStripMenuItem, toolStripSeparator1, addScriptToolStripMenuItem, insertScriptToolStripMenuItem });
            TreeContextMenuStrip.Name = "TreeContextMenuStrip";
            TreeContextMenuStrip.Size = new Size(163, 76);
            // 
            // sendCommandToolStripMenuItem
            // 
            sendCommandToolStripMenuItem.Name = "sendCommandToolStripMenuItem";
            sendCommandToolStripMenuItem.Size = new Size(162, 22);
            sendCommandToolStripMenuItem.Text = "Send Command";
            sendCommandToolStripMenuItem.Click += sendCommandToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(159, 6);
            // 
            // addScriptToolStripMenuItem
            // 
            addScriptToolStripMenuItem.Name = "addScriptToolStripMenuItem";
            addScriptToolStripMenuItem.Size = new Size(162, 22);
            addScriptToolStripMenuItem.Text = "Add Script";
            addScriptToolStripMenuItem.Click += addScriptToolStripMenuItem_Click;
            // 
            // insertScriptToolStripMenuItem
            // 
            insertScriptToolStripMenuItem.Name = "insertScriptToolStripMenuItem";
            insertScriptToolStripMenuItem.Size = new Size(162, 22);
            insertScriptToolStripMenuItem.Text = "Insert Script";
            insertScriptToolStripMenuItem.Click += insertScriptToolStripMenuItem_Click;
            // 
            // HCIScriptContextMenuStrip
            // 
            HCIScriptContextMenuStrip.Items.AddRange(new ToolStripItem[] { sendCommandToolStripMenuItem1, runScriptToolStripMenuItem, toolStripSeparator3, deleteToolStripMenuItem, toolStripSeparator2, changeScriptDirToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem });
            HCIScriptContextMenuStrip.Name = "HCIScriptContextMenuStrip";
            HCIScriptContextMenuStrip.Size = new Size(179, 148);
            // 
            // sendCommandToolStripMenuItem1
            // 
            sendCommandToolStripMenuItem1.Name = "sendCommandToolStripMenuItem1";
            sendCommandToolStripMenuItem1.Size = new Size(178, 22);
            sendCommandToolStripMenuItem1.Text = "Send Command";
            sendCommandToolStripMenuItem1.Click += sendCommandToolStripMenuItem1_Click;
            // 
            // runScriptToolStripMenuItem
            // 
            runScriptToolStripMenuItem.Name = "runScriptToolStripMenuItem";
            runScriptToolStripMenuItem.Size = new Size(178, 22);
            runScriptToolStripMenuItem.Text = "Run Script";
            runScriptToolStripMenuItem.Click += runScriptToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(175, 6);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(178, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(175, 6);
            // 
            // changeScriptDirToolStripMenuItem
            // 
            changeScriptDirToolStripMenuItem.Name = "changeScriptDirToolStripMenuItem";
            changeScriptDirToolStripMenuItem.Size = new Size(178, 22);
            changeScriptDirToolStripMenuItem.Text = "Change Script Path";
            changeScriptDirToolStripMenuItem.Click += changeScriptDirToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(178, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(178, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // BaseTableLayoutPanel
            // 
            BaseTableLayoutPanel.ColumnCount = 1;
            BaseTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            BaseTableLayoutPanel.Controls.Add(HCISplitContainer, 0, 1);
            BaseTableLayoutPanel.Controls.Add(tableLayoutPanel6, 0, 0);
            BaseTableLayoutPanel.Dock = DockStyle.Fill;
            BaseTableLayoutPanel.Location = new Point(0, 0);
            BaseTableLayoutPanel.Name = "BaseTableLayoutPanel";
            BaseTableLayoutPanel.RowCount = 2;
            BaseTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            BaseTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            BaseTableLayoutPanel.Size = new Size(584, 562);
            BaseTableLayoutPanel.TabIndex = 4;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel6.Controls.Add(tabControl_HciControl, 1, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(0, 0);
            tableLayoutPanel6.Margin = new Padding(0);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 1;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Size = new Size(584, 110);
            tableLayoutPanel6.TabIndex = 4;
            // 
            // tabControl_HciControl
            // 
            tabControl_HciControl.Controls.Add(tabPage_Connection);
            tabControl_HciControl.Controls.Add(tabPage_Test);
            tabControl_HciControl.Dock = DockStyle.Fill;
            tabControl_HciControl.Location = new Point(283, 3);
            tabControl_HciControl.Margin = new Padding(3, 3, 3, 0);
            tabControl_HciControl.Name = "tabControl_HciControl";
            tabControl_HciControl.SelectedIndex = 0;
            tabControl_HciControl.Size = new Size(298, 107);
            tabControl_HciControl.TabIndex = 2;
            // 
            // tabPage_Connection
            // 
            tabPage_Connection.Controls.Add(dataGridView_AdvReports);
            tabPage_Connection.Location = new Point(4, 24);
            tabPage_Connection.Name = "tabPage_Connection";
            tabPage_Connection.Size = new Size(290, 79);
            tabPage_Connection.TabIndex = 1;
            tabPage_Connection.Text = "Connection";
            tabPage_Connection.UseVisualStyleBackColor = true;
            // 
            // dataGridView_AdvReports
            // 
            dataGridView_AdvReports.AllowUserToAddRows = false;
            dataGridView_AdvReports.AllowUserToDeleteRows = false;
            dataGridView_AdvReports.AllowUserToResizeColumns = false;
            dataGridView_AdvReports.AllowUserToResizeRows = false;
            dataGridView_AdvReports.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_AdvReports.Columns.AddRange(new DataGridViewColumn[] { Column_BDAddress, Column_RSSI, Column_EventType, Column_AddrType, Column_ConnHandle, Column_Data });
            dataGridView_AdvReports.Dock = DockStyle.Fill;
            dataGridView_AdvReports.Location = new Point(0, 0);
            dataGridView_AdvReports.Name = "dataGridView_AdvReports";
            dataGridView_AdvReports.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridView_AdvReports.RowsDefaultCellStyle = dataGridViewCellStyle3;
            dataGridView_AdvReports.RowTemplate.Height = 18;
            dataGridView_AdvReports.Size = new Size(290, 79);
            dataGridView_AdvReports.TabIndex = 4;
            dataGridView_AdvReports.SelectionChanged += dataGridView_AdvReports_SelectionChanged;
            dataGridView_AdvReports.SizeChanged += dataGridView_AdvReports_SizeChanged;
            // 
            // Column_BDAddress
            // 
            Column_BDAddress.HeaderText = "Address";
            Column_BDAddress.Name = "Column_BDAddress";
            Column_BDAddress.ReadOnly = true;
            Column_BDAddress.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_BDAddress.Width = 60;
            // 
            // Column_RSSI
            // 
            Column_RSSI.HeaderText = "RSSI";
            Column_RSSI.Name = "Column_RSSI";
            Column_RSSI.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_RSSI.Width = 45;
            // 
            // Column_EventType
            // 
            Column_EventType.HeaderText = "ET";
            Column_EventType.Name = "Column_EventType";
            Column_EventType.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_EventType.Width = 30;
            // 
            // Column_AddrType
            // 
            Column_AddrType.HeaderText = "AT";
            Column_AddrType.Name = "Column_AddrType";
            Column_AddrType.ReadOnly = true;
            Column_AddrType.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_AddrType.Width = 30;
            // 
            // Column_ConnHandle
            // 
            Column_ConnHandle.HeaderText = "Handle";
            Column_ConnHandle.Name = "Column_ConnHandle";
            Column_ConnHandle.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_ConnHandle.Width = 55;
            // 
            // Column_Data
            // 
            Column_Data.HeaderText = "Data";
            Column_Data.Name = "Column_Data";
            Column_Data.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column_Data.Width = 60;
            // 
            // tabPage_Test
            // 
            tabPage_Test.Controls.Add(button_RunModTest);
            tabPage_Test.Location = new Point(4, 24);
            tabPage_Test.Name = "tabPage_Test";
            tabPage_Test.Padding = new Padding(3);
            tabPage_Test.Size = new Size(290, 79);
            tabPage_Test.TabIndex = 0;
            tabPage_Test.Text = "Test";
            tabPage_Test.UseVisualStyleBackColor = true;
            // 
            // button_RunModTest
            // 
            button_RunModTest.Location = new Point(6, 6);
            button_RunModTest.Name = "button_RunModTest";
            button_RunModTest.Size = new Size(110, 24);
            button_RunModTest.TabIndex = 0;
            button_RunModTest.Text = "Run Modulation";
            button_RunModTest.UseVisualStyleBackColor = true;
            button_RunModTest.Click += button_RunModTest_Click;
            // 
            // HCIControlForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(584, 562);
            Controls.Add(BaseTableLayoutPanel);
            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "HCIControlForm";
            ShowIcon = false;
            Text = "HCI Controller";
            FormClosing += HCIControlForm_FormClosing;
            FormClosed += HCIControlForm_FormClosed;
            Load += HCIControlForm_Load;
            SizeChanged += HCIControlForm_SizeChanged;
            groupBox1.ResumeLayout(false);
            PortTabControl.ResumeLayout(false);
            PortSerialTabPage.ResumeLayout(false);
            PortSerialTabPage.PerformLayout();
            PortUSBtabPage.ResumeLayout(false);
            PortUSBtabPage.PerformLayout();
            HCISplitContainer.Panel1.ResumeLayout(false);
            HCISplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)HCISplitContainer).EndInit();
            HCISplitContainer.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            HciParaGroupBox.ResumeLayout(false);
            HciParaGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)HCIParaDataGridView).EndInit();
            tabControl1.ResumeLayout(false);
            HciLogTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)HciLogDataGridView).EndInit();
            HCIScriptTabPage.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            TreeContextMenuStrip.ResumeLayout(false);
            HCIScriptContextMenuStrip.ResumeLayout(false);
            BaseTableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel6.ResumeLayout(false);
            tabControl_HciControl.ResumeLayout(false);
            tabPage_Connection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView_AdvReports).EndInit();
            tabPage_Test.ResumeLayout(false);
            ResumeLayout(false);

        }


        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button PortOpenButton;
        private System.Windows.Forms.TabControl PortTabControl;
        private System.Windows.Forms.TabPage PortSerialTabPage;
        private System.Windows.Forms.ComboBox SerialPortComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage PortUSBtabPage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox PortBaudRateComboBox;
        private System.Windows.Forms.Button PortRefreshButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox USBDevicesComboBox;
        private System.Windows.Forms.SplitContainer HCISplitContainer;
        private System.Windows.Forms.TreeView HCICommandTreeView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button PortCloseButton;
        private System.Windows.Forms.GroupBox HciParaGroupBox;
        private System.Windows.Forms.ContextMenuStrip TreeContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem sendCommandToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem addScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertScriptToolStripMenuItem;
        private System.Windows.Forms.DataGridView HCIParaDataGridView;
        private System.Windows.Forms.Button SendHciCommandButton;
        private System.Windows.Forms.Label HCIParaNameLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage HciLogTabPage;
        private System.Windows.Forms.TabPage HCIScriptTabPage;
        private System.Windows.Forms.DataGridView HciLogDataGridView;
        private System.Windows.Forms.Button ClearLogButton;
        private System.Windows.Forms.Button SaveLogButton;
        private System.Windows.Forms.TreeView HCIScriptTreeView;
        private System.Windows.Forms.Button RunHciScriptButton;
        private System.Windows.Forms.ComboBox HCIScriptComboBox;
        private System.Windows.Forms.Button SaveHciScriptButton;
        private System.Windows.Forms.Button ClearHciScriptButton;
        private System.Windows.Forms.Button DeleteHciScriptButton;
        private System.Windows.Forms.Button InsertHciScriptButton;
        private System.Windows.Forms.Button AddHciScriptButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button SaveAsHciScriptButton;
        private System.Windows.Forms.ContextMenuStrip HCIScriptContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem sendCommandToolStripMenuItem1;
        private System.Windows.Forms.Button ChangeScrDirButton;
        private System.Windows.Forms.ToolStripMenuItem changeScriptDirToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel BaseTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TabControl tabControl_HciControl;
        private System.Windows.Forms.TabPage tabPage_Test;
        private System.Windows.Forms.Button button_RunModTest;
        private System.Windows.Forms.TabPage tabPage_Connection;
        private System.Windows.Forms.DataGridView dataGridView_AdvReports;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_BDAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_RSSI;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_EventType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_AddrType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_ConnHandle;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Data;
    }
}