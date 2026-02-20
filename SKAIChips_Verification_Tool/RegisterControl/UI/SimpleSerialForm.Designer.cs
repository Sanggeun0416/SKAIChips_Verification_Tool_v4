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
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.numDevIndex = new System.Windows.Forms.NumericUpDown();
            this.lblDevIdx = new System.Windows.Forms.Label();
            this.cmbProtocol = new System.Windows.Forms.ComboBox();
            this.lblProtocol = new System.Windows.Forms.Label();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.cmbSpiMode = new System.Windows.Forms.ComboBox();
            this.lblSpiMode = new System.Windows.Forms.Label();
            this.txtSlaveAddr = new System.Windows.Forms.TextBox();
            this.lblSlaveAddr = new System.Windows.Forms.Label();
            this.txtSpeed = new System.Windows.Forms.TextBox();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.grpDataIO = new System.Windows.Forms.GroupBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.numReadLen = new System.Windows.Forms.NumericUpDown();
            this.lblReadLen = new System.Windows.Forms.Label();
            this.txtWriteData = new System.Windows.Forms.TextBox();
            this.lblWriteData = new System.Windows.Forms.Label();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDevIndex)).BeginInit();
            this.grpSettings.SuspendLayout();
            this.grpDataIO.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numReadLen)).BeginInit();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpConnection
            // 
            this.grpConnection.Controls.Add(this.btnConnect);
            this.grpConnection.Controls.Add(this.numDevIndex);
            this.grpConnection.Controls.Add(this.lblDevIdx);
            this.grpConnection.Controls.Add(this.cmbProtocol);
            this.grpConnection.Controls.Add(this.lblProtocol);
            this.grpConnection.Location = new System.Drawing.Point(12, 12);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Size = new System.Drawing.Size(460, 70);
            this.grpConnection.TabIndex = 0;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(350, 20);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(95, 30);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // numDevIndex
            // 
            this.numDevIndex.Location = new System.Drawing.Point(240, 25);
            this.numDevIndex.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDevIndex.Name = "numDevIndex";
            this.numDevIndex.Size = new System.Drawing.Size(50, 23);
            this.numDevIndex.TabIndex = 3;
            // 
            // lblDevIdx
            // 
            this.lblDevIdx.AutoSize = true;
            this.lblDevIdx.Location = new System.Drawing.Point(180, 28);
            this.lblDevIdx.Name = "lblDevIdx";
            this.lblDevIdx.Size = new System.Drawing.Size(54, 15);
            this.lblDevIdx.TabIndex = 2;
            this.lblDevIdx.Text = "Dev Idx:";
            // 
            // cmbProtocol
            // 
            this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProtocol.FormattingEnabled = true;
            this.cmbProtocol.Location = new System.Drawing.Point(75, 25);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new System.Drawing.Size(80, 23);
            this.cmbProtocol.TabIndex = 1;
            // 
            // lblProtocol
            // 
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Location = new System.Drawing.Point(15, 28);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new System.Drawing.Size(54, 15);
            this.lblProtocol.TabIndex = 0;
            this.lblProtocol.Text = "Protocol:";
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.cmbSpiMode);
            this.grpSettings.Controls.Add(this.lblSpiMode);
            this.grpSettings.Controls.Add(this.txtSlaveAddr);
            this.grpSettings.Controls.Add(this.lblSlaveAddr);
            this.grpSettings.Controls.Add(this.txtSpeed);
            this.grpSettings.Controls.Add(this.lblSpeed);
            this.grpSettings.Location = new System.Drawing.Point(12, 88);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(460, 70);
            this.grpSettings.TabIndex = 1;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Settings";
            // 
            // cmbSpiMode
            // 
            this.cmbSpiMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpiMode.FormattingEnabled = true;
            this.cmbSpiMode.Location = new System.Drawing.Point(375, 25);
            this.cmbSpiMode.Name = "cmbSpiMode";
            this.cmbSpiMode.Size = new System.Drawing.Size(70, 23);
            this.cmbSpiMode.TabIndex = 5;
            // 
            // lblSpiMode
            // 
            this.lblSpiMode.AutoSize = true;
            this.lblSpiMode.Location = new System.Drawing.Point(305, 28);
            this.lblSpiMode.Name = "lblSpiMode";
            this.lblSpiMode.Size = new System.Drawing.Size(64, 15);
            this.lblSpiMode.TabIndex = 4;
            this.lblSpiMode.Text = "SPI Mode:";
            // 
            // txtSlaveAddr
            // 
            this.txtSlaveAddr.Location = new System.Drawing.Point(220, 25);
            this.txtSlaveAddr.Name = "txtSlaveAddr";
            this.txtSlaveAddr.Size = new System.Drawing.Size(70, 23);
            this.txtSlaveAddr.TabIndex = 3;
            // 
            // lblSlaveAddr
            // 
            this.lblSlaveAddr.AutoSize = true;
            this.lblSlaveAddr.Location = new System.Drawing.Point(150, 28);
            this.lblSlaveAddr.Name = "lblSlaveAddr";
            this.lblSlaveAddr.Size = new System.Drawing.Size(64, 15);
            this.lblSlaveAddr.TabIndex = 2;
            this.lblSlaveAddr.Text = "I2C Addr:";
            // 
            // txtSpeed
            // 
            this.txtSpeed.Location = new System.Drawing.Point(75, 25);
            this.txtSpeed.Name = "txtSpeed";
            this.txtSpeed.Size = new System.Drawing.Size(60, 23);
            this.txtSpeed.TabIndex = 1;
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(15, 28);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(54, 15);
            this.lblSpeed.TabIndex = 0;
            this.lblSpeed.Text = "Speed:";
            // 
            // grpDataIO
            // 
            this.grpDataIO.Controls.Add(this.btnRead);
            this.grpDataIO.Controls.Add(this.btnWrite);
            this.grpDataIO.Controls.Add(this.numReadLen);
            this.grpDataIO.Controls.Add(this.lblReadLen);
            this.grpDataIO.Controls.Add(this.txtWriteData);
            this.grpDataIO.Controls.Add(this.lblWriteData);
            this.grpDataIO.Location = new System.Drawing.Point(12, 164);
            this.grpDataIO.Name = "grpDataIO";
            this.grpDataIO.Size = new System.Drawing.Size(460, 100);
            this.grpDataIO.TabIndex = 2;
            this.grpDataIO.TabStop = false;
            this.grpDataIO.Text = "Data I/O";
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(370, 58);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(75, 30);
            this.btnRead.TabIndex = 5;
            this.btnRead.Text = "Read";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(370, 20);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(75, 30);
            this.btnWrite.TabIndex = 4;
            this.btnWrite.Text = "Write";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // numReadLen
            // 
            this.numReadLen.Location = new System.Drawing.Point(100, 63);
            this.numReadLen.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numReadLen.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numReadLen.Name = "numReadLen";
            this.numReadLen.Size = new System.Drawing.Size(80, 23);
            this.numReadLen.TabIndex = 3;
            this.numReadLen.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblReadLen
            // 
            this.lblReadLen.AutoSize = true;
            this.lblReadLen.Location = new System.Drawing.Point(15, 65);
            this.lblReadLen.Name = "lblReadLen";
            this.lblReadLen.Size = new System.Drawing.Size(75, 15);
            this.lblReadLen.TabIndex = 2;
            this.lblReadLen.Text = "Read Bytes:";
            // 
            // txtWriteData
            // 
            this.txtWriteData.Location = new System.Drawing.Point(100, 25);
            this.txtWriteData.Name = "txtWriteData";
            this.txtWriteData.PlaceholderText = "e.g. 00 01 AB";
            this.txtWriteData.Size = new System.Drawing.Size(260, 23);
            this.txtWriteData.TabIndex = 1;
            // 
            // lblWriteData
            // 
            this.lblWriteData.AutoSize = true;
            this.lblWriteData.Location = new System.Drawing.Point(15, 28);
            this.lblWriteData.Name = "lblWriteData";
            this.lblWriteData.Size = new System.Drawing.Size(75, 15);
            this.lblWriteData.TabIndex = 0;
            this.lblWriteData.Text = "Write Data:";
            // 
            // grpLog
            // 
            this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLog.Controls.Add(this.rtbLog);
            this.grpLog.Location = new System.Drawing.Point(12, 270);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(460, 280);
            this.grpLog.TabIndex = 3;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Log";
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(3, 19);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(454, 258);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // SimpleSerialForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 561);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpDataIO);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpConnection);
            this.Name = "SimpleSerialForm";
            this.Text = "Simple Serial Controller";
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDevIndex)).EndInit();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.grpDataIO.ResumeLayout(false);
            this.grpDataIO.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numReadLen)).EndInit();
            this.grpLog.ResumeLayout(false);
            this.ResumeLayout(false);

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