using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace LiveSplit.LCGoL
{
	public class LogForm : Form
    {
        private TextBox _tbLog;

        public LogForm()
        {
            InitializeComponent();
        }

        public void AddMessage(string message)
        {
            _tbLog.AppendText(Environment.NewLine + DateTime.Now.ToShortTimeString() + "\t" + message);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void InitializeComponent()
        {
            _tbLog = new TextBox();
            SuspendLayout();
            _tbLog.Dock = DockStyle.Fill;
            _tbLog.Location = new System.Drawing.Point(0, 0);
            _tbLog.Multiline = true;
            _tbLog.Name = "_tbLog";
            _tbLog.Size = new System.Drawing.Size(477, 93);
            _tbLog.TabIndex = 0;
            _tbLog.TabStop = false;
            _tbLog.Text = "IL PBs will be logged here. You'll also hear a sound when you get an IL PB. If you don't want to hear this sound, mute LiveSplit in the Windows audio mixer or bug me until I add a setting.";
            AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(477, 93);
            Controls.Add(_tbLog);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "LogForm";
            Text = "Lara Croft: GoL - IL PB Log";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
