namespace VisualGit.UI.PendingChanges
{
    partial class PendingIssuesPage
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PendingIssuesPage));
			this.pleaseConfigureLabel = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// pleaseConfigureLabel
			// 
			this.pleaseConfigureLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.pleaseConfigureLabel, "pleaseConfigureLabel");
			this.pleaseConfigureLabel.Name = "pleaseConfigureLabel";
			this.pleaseConfigureLabel.TabStop = true;
			this.pleaseConfigureLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.pleaseConfigureLabel_LinkClicked);
			// 
			// PendingIssuesPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pleaseConfigureLabel);
			this.Name = "PendingIssuesPage";
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.LinkLabel pleaseConfigureLabel;


	}
}
