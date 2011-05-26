namespace VisualGit.UI.SvnLog
{
    sealed partial class LogControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogControl));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.logRevisionControl1 = new VisualGit.UI.SvnLog.LogRevisionControl(this.components);
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.logChangedPaths1 = new VisualGit.UI.SvnLog.LogChangedPaths(this.components);
			this.logMessageView1 = new VisualGit.UI.SvnLog.LogMessageView(this.components);
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.logRevisionControl1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			// 
			// logRevisionControl1
			// 
			resources.ApplyResources(this.logRevisionControl1, "logRevisionControl1");
			this.logRevisionControl1.LogSource = null;
			this.logRevisionControl1.Name = "logRevisionControl1";
			// 
			// splitContainer2
			// 
			resources.ApplyResources(this.splitContainer2, "splitContainer2");
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.logChangedPaths1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.logMessageView1);
			// 
			// logChangedPaths1
			// 
			resources.ApplyResources(this.logChangedPaths1, "logChangedPaths1");
			this.logChangedPaths1.ItemSource = this.logRevisionControl1;
			this.logChangedPaths1.LogSource = null;
			this.logChangedPaths1.Name = "logChangedPaths1";
			// 
			// logMessageView1
			// 
			resources.ApplyResources(this.logMessageView1, "logMessageView1");
			this.logMessageView1.ItemSource = this.logRevisionControl1;
			this.logMessageView1.Name = "logMessageView1";
			// 
			// LogControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "LogControl";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private LogRevisionControl logRevisionControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private LogChangedPaths logChangedPaths1;
        private LogMessageView logMessageView1;

    }
}
