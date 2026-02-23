using System.Drawing;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool
{
    partial class MainForm
    {
        
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuExit;
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuRegisterControl;
        private ToolStripMenuItem menuHCIControl;
        private ToolStripMenuItem menuSimpleSerial;
        private ToolStripMenuItem menuSetup;
        private ToolStripMenuItem menuSetupInstrument;




        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }




        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem();
            menuExit = new ToolStripMenuItem();
            menuTools = new ToolStripMenuItem();
            menuRegisterControl = new ToolStripMenuItem();
            menuHCIControl = new ToolStripMenuItem();
            menuSimpleSerial = new ToolStripMenuItem();
            menuSetup = new ToolStripMenuItem();
            menuSetupInstrument = new ToolStripMenuItem();
            getsAllToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuFile, menuTools, menuSetup });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(734, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuFile
            // 
            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuExit });
            menuFile.Name = "menuFile";
            menuFile.Size = new Size(37, 20);
            menuFile.Text = "File";
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(93, 22);
            menuExit.Text = "Exit";
            menuExit.Click += menuExit_Click;
            // 
            // menuTools
            // 
            menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuRegisterControl, menuHCIControl, menuSimpleSerial });
            menuTools.Name = "menuTools";
            menuTools.Size = new Size(47, 20);
            menuTools.Text = "Tools";
            // 
            // menuRegisterControl
            // 
            menuRegisterControl.Name = "menuRegisterControl";
            menuRegisterControl.Size = new Size(201, 22);
            menuRegisterControl.Text = "Register Control";
            menuRegisterControl.Click += menuRegisterControl_Click;
            // 
            // menuHCIControl
            // 
            menuHCIControl.Name = "menuHCIControl";
            menuHCIControl.Size = new Size(201, 22);
            menuHCIControl.Text = "HCI Controller";
            menuHCIControl.Click += menuHCIControl_Click;
            // 
            // menuSimpleSerial
            // 
            menuSimpleSerial.Name = "menuSimpleSerial";
            menuSimpleSerial.Size = new Size(201, 22);
            menuSimpleSerial.Text = "Simple Serial Controller";
            menuSimpleSerial.Click += menuSimpleSerial_Click;
            // 
            // menuSetup
            // 
            menuSetup.DropDownItems.AddRange(new ToolStripItem[] { menuSetupInstrument, getsAllToolStripMenuItem });
            menuSetup.Name = "menuSetup";
            menuSetup.Size = new Size(50, 20);
            menuSetup.Text = "Setup";
            // 
            // menuSetupInstrument
            // 
            menuSetupInstrument.Name = "menuSetupInstrument";
            menuSetupInstrument.Size = new Size(132, 22);
            menuSetupInstrument.Text = "Instrument";
            menuSetupInstrument.Click += menuSetupInstrument_Click;
            // 
            // getsAllToolStripMenuItem
            // 
            getsAllToolStripMenuItem.Name = "getsAllToolStripMenuItem";
            getsAllToolStripMenuItem.Size = new Size(132, 22);
            getsAllToolStripMenuItem.Text = "Gets All";
            getsAllToolStripMenuItem.Click += getsAllToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(734, 1001);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(705, 805);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SKAIChips Verification Tool V4.0.0";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private ToolStripMenuItem getsAllToolStripMenuItem;
    }
}
