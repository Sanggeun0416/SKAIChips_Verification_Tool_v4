namespace SKAIChips_Verification_Tool
{
    partial class SimpleSerialForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpConnection = new GroupBox();
            btnConnect = new Button();
            numDevIndex = new NumericUpDown();
            lblDevIdx = new Label();
            cmbProtocol = new ComboBox();
            lblProtocol = new Label();
            grpSettings = new GroupBox();
            cmbSpiMode = new ComboBox();
            lblSpiMode = new Label();
            txtSlaveAddr = new TextBox();
            lblSlaveAddr = new Label();
            txtSpeed = new TextBox();
            lblSpeed = new Label();
            grpDataIO = new GroupBox();
            btnRead = new Button();
            btnWrite = new Button();
            numReadLen = new NumericUpDown();
            lblReadLen = new Label();
            txtWriteData = new TextBox();
            lblWriteData = new Label();
            grpLog = new GroupBox();
            rtbLog = new RichTextBox();
            grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDevIndex).BeginInit();
            grpSettings.SuspendLayout();
            grpDataIO.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numReadLen).BeginInit();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpConnection
            // 
            grpConnection.Controls.Add(btnConnect);
            grpConnection.Controls.Add(numDevIndex);
            grpConnection.Controls.Add(lblDevIdx);
            grpConnection.Controls.Add(cmbProtocol);
            grpConnection.Controls.Add(lblProtocol);
            grpConnection.Location = new Point(12, 12);
            grpConnection.Name = "grpConnection";
            grpConnection.Size = new Size(460, 70);
            grpConnection.TabIndex = 0;
            grpConnection.TabStop = false;
            grpConnection.Text = "Connection";
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(350, 20);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(95, 30);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // numDevIndex
            // 
            numDevIndex.Location = new Point(240, 25);
            numDevIndex.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numDevIndex.Name = "numDevIndex";
            numDevIndex.Size = new Size(50, 23);
            numDevIndex.TabIndex = 3;
            // 
            // lblDevIdx
            // 
            lblDevIdx.AutoSize = true;
            lblDevIdx.Location = new Point(180, 28);
            lblDevIdx.Name = "lblDevIdx";
            lblDevIdx.Size = new Size(51, 15);
            lblDevIdx.TabIndex = 2;
            lblDevIdx.Text = "Dev Idx:";
            // 
            // cmbProtocol
            // 
            cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProtocol.FormattingEnabled = true;
            cmbProtocol.Location = new Point(75, 25);
            cmbProtocol.Name = "cmbProtocol";
            cmbProtocol.Size = new Size(80, 23);
            cmbProtocol.TabIndex = 1;
            // 
            // lblProtocol
            // 
            lblProtocol.AutoSize = true;
            lblProtocol.Location = new Point(15, 28);
            lblProtocol.Name = "lblProtocol";
            lblProtocol.Size = new Size(55, 15);
            lblProtocol.TabIndex = 0;
            lblProtocol.Text = "Protocol:";
            // 
            // grpSettings
            // 
            grpSettings.Controls.Add(cmbSpiMode);
            grpSettings.Controls.Add(lblSpiMode);
            grpSettings.Controls.Add(txtSlaveAddr);
            grpSettings.Controls.Add(lblSlaveAddr);
            grpSettings.Controls.Add(txtSpeed);
            grpSettings.Controls.Add(lblSpeed);
            grpSettings.Location = new Point(12, 88);
            grpSettings.Name = "grpSettings";
            grpSettings.Size = new Size(460, 70);
            grpSettings.TabIndex = 1;
            grpSettings.TabStop = false;
            grpSettings.Text = "Settings";
            // 
            // cmbSpiMode
            // 
            cmbSpiMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpiMode.FormattingEnabled = true;
            cmbSpiMode.Location = new Point(375, 25);
            cmbSpiMode.Name = "cmbSpiMode";
            cmbSpiMode.Size = new Size(70, 23);
            cmbSpiMode.TabIndex = 5;
            // 
            // lblSpiMode
            // 
            lblSpiMode.AutoSize = true;
            lblSpiMode.Location = new Point(305, 28);
            lblSpiMode.Name = "lblSpiMode";
            lblSpiMode.Size = new Size(62, 15);
            lblSpiMode.TabIndex = 4;
            lblSpiMode.Text = "SPI Mode:";
            // 
            // txtSlaveAddr
            // 
            txtSlaveAddr.Location = new Point(220, 25);
            txtSlaveAddr.Name = "txtSlaveAddr";
            txtSlaveAddr.Size = new Size(70, 23);
            txtSlaveAddr.TabIndex = 3;
            // 
            // lblSlaveAddr
            // 
            lblSlaveAddr.AutoSize = true;
            lblSlaveAddr.Location = new Point(150, 28);
            lblSlaveAddr.Name = "lblSlaveAddr";
            lblSlaveAddr.Size = new Size(58, 15);
            lblSlaveAddr.TabIndex = 2;
            lblSlaveAddr.Text = "I2C Addr:";
            // 
            // txtSpeed
            // 
            txtSpeed.Location = new Point(75, 25);
            txtSpeed.Name = "txtSpeed";
            txtSpeed.Size = new Size(60, 23);
            txtSpeed.TabIndex = 1;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(15, 28);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(43, 15);
            lblSpeed.TabIndex = 0;
            lblSpeed.Text = "Speed:";
            // 
            // grpDataIO
            // 
            grpDataIO.Controls.Add(btnRead);
            grpDataIO.Controls.Add(btnWrite);
            grpDataIO.Controls.Add(numReadLen);
            grpDataIO.Controls.Add(lblReadLen);
            grpDataIO.Controls.Add(txtWriteData);
            grpDataIO.Controls.Add(lblWriteData);
            grpDataIO.Location = new Point(12, 164);
            grpDataIO.Name = "grpDataIO";
            grpDataIO.Size = new Size(460, 100);
            grpDataIO.TabIndex = 2;
            grpDataIO.TabStop = false;
            grpDataIO.Text = "Data I/O";
            // 
            // btnRead
            // 
            btnRead.Location = new Point(370, 58);
            btnRead.Name = "btnRead";
            btnRead.Size = new Size(75, 30);
            btnRead.TabIndex = 5;
            btnRead.Text = "Read";
            btnRead.UseVisualStyleBackColor = true;
            btnRead.Click += btnRead_Click;
            // 
            // btnWrite
            // 
            btnWrite.Location = new Point(370, 20);
            btnWrite.Name = "btnWrite";
            btnWrite.Size = new Size(75, 30);
            btnWrite.TabIndex = 4;
            btnWrite.Text = "Write";
            btnWrite.UseVisualStyleBackColor = true;
            btnWrite.Click += btnWrite_Click;
            // 
            // numReadLen
            // 
            numReadLen.Location = new Point(100, 63);
            numReadLen.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numReadLen.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numReadLen.Name = "numReadLen";
            numReadLen.Size = new Size(80, 23);
            numReadLen.TabIndex = 3;
            numReadLen.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblReadLen
            // 
            lblReadLen.AutoSize = true;
            lblReadLen.Location = new Point(15, 65);
            lblReadLen.Name = "lblReadLen";
            lblReadLen.Size = new Size(68, 15);
            lblReadLen.TabIndex = 2;
            lblReadLen.Text = "Read Bytes:";
            // 
            // txtWriteData
            // 
            txtWriteData.Location = new Point(100, 25);
            txtWriteData.Name = "txtWriteData";
            txtWriteData.PlaceholderText = "e.g. 00 01 AB";
            txtWriteData.Size = new Size(260, 23);
            txtWriteData.TabIndex = 1;
            // 
            // lblWriteData
            // 
            lblWriteData.AutoSize = true;
            lblWriteData.Location = new Point(15, 28);
            lblWriteData.Name = "lblWriteData";
            lblWriteData.Size = new Size(67, 15);
            lblWriteData.TabIndex = 0;
            lblWriteData.Text = "Write Data:";
            // 
            // grpLog
            // 
            grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLog.Controls.Add(rtbLog);
            grpLog.Location = new Point(12, 270);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(460, 280);
            grpLog.TabIndex = 3;
            grpLog.TabStop = false;
            grpLog.Text = "Log";
            // 
            // rtbLog
            // 
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Location = new Point(3, 19);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(454, 258);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "";
            // 
            // SimpleSerialForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 561);
            Controls.Add(grpLog);
            Controls.Add(grpDataIO);
            Controls.Add(grpSettings);
            Controls.Add(grpConnection);
            Name = "SimpleSerialForm";
            ShowIcon = false;
            Text = "Simple Serial Controller";
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numDevIndex).EndInit();
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            grpDataIO.ResumeLayout(false);
            grpDataIO.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numReadLen).EndInit();
            grpLog.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.NumericUpDown numDevIndex;
        private System.Windows.Forms.Label lblDevIdx;
        private System.Windows.Forms.ComboBox cmbProtocol;
        private System.Windows.Forms.Label lblProtocol;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.ComboBox cmbSpiMode;
        private System.Windows.Forms.Label lblSpiMode;
        private System.Windows.Forms.TextBox txtSlaveAddr;
        private System.Windows.Forms.Label lblSlaveAddr;
        private System.Windows.Forms.TextBox txtSpeed;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.GroupBox grpDataIO;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.NumericUpDown numReadLen;
        private System.Windows.Forms.Label lblReadLen;
        private System.Windows.Forms.TextBox txtWriteData;
        private System.Windows.Forms.Label lblWriteData;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}