namespace VisualGit.UI.MergeWizard
{
    partial class MergeRevisionsSelectionPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeRevisionsSelectionPage));
            this.logToolControl1 = new VisualGit.UI.GitLog.LogControl(this.components);
            this.SuspendLayout();
            // 
            // logToolControl1
            // 
            this.logToolControl1.ShowChangedPaths = true;
            this.logToolControl1.Context = null;
            resources.ApplyResources(this.logToolControl1, "logToolControl1");
            this.logToolControl1.IncludeMergedRevisions = false;
            this.logToolControl1.ShowLogMessage = true;
            this.logToolControl1.Mode = VisualGit.UI.GitLog.LogMode.Log;
            this.logToolControl1.Name = "logToolControl1";
            this.logToolControl1.StrictNodeHistory = false;
            this.logToolControl1.BatchFinished += new System.EventHandler<VisualGit.UI.GitLog.BatchFinishedEventArgs>(this.logToolControl1_BatchFinished);
            // 
            // MergeRevisionsSelectionPageControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.logToolControl1);
            this.Name = "MergeRevisionsSelectionPageControl";
            this.ResumeLayout(false);

        }

        #endregion

        private VisualGit.UI.GitLog.LogControl logToolControl1;
    }
}
