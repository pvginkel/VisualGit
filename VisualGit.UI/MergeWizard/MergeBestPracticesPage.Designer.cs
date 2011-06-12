namespace VisualGit.UI.MergeWizard
{
    partial class MergeBestPracticesPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeBestPracticesPage));
            this.noUncommitedModificationsLabel = new System.Windows.Forms.Label();
            this.noUncommittedModificationsDescriptionLabel = new System.Windows.Forms.Label();
            this.noUncommitedModificationsPictureBox = new System.Windows.Forms.PictureBox();
            this.noUncommitedModificationsPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.noUncommitedModificationsPictureBox)).BeginInit();
            this.noUncommitedModificationsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // noUncommitedModificationsLabel
            // 
            resources.ApplyResources(this.noUncommitedModificationsLabel, "noUncommitedModificationsLabel");
            this.noUncommitedModificationsLabel.Name = "noUncommitedModificationsLabel";
            // 
            // noUncommittedModificationsDescriptionLabel
            // 
            resources.ApplyResources(this.noUncommittedModificationsDescriptionLabel, "noUncommittedModificationsDescriptionLabel");
            this.noUncommittedModificationsDescriptionLabel.Name = "noUncommittedModificationsDescriptionLabel";
            // 
            // noUncommitedModificationsPictureBox
            // 
            resources.ApplyResources(this.noUncommitedModificationsPictureBox, "noUncommitedModificationsPictureBox");
            this.noUncommitedModificationsPictureBox.Name = "noUncommitedModificationsPictureBox";
            this.noUncommitedModificationsPictureBox.TabStop = false;
            // 
            // noUncommitedModificationsPanel
            // 
            resources.ApplyResources(this.noUncommitedModificationsPanel, "noUncommitedModificationsPanel");
            this.noUncommitedModificationsPanel.Controls.Add(this.noUncommitedModificationsPictureBox);
            this.noUncommitedModificationsPanel.Controls.Add(this.noUncommittedModificationsDescriptionLabel);
            this.noUncommitedModificationsPanel.Controls.Add(this.noUncommitedModificationsLabel);
            this.noUncommitedModificationsPanel.Name = "noUncommitedModificationsPanel";
            // 
            // MergeBestPracticesPage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.noUncommitedModificationsPanel);
            this.Name = "MergeBestPracticesPage";
            ((System.ComponentModel.ISupportInitialize)(this.noUncommitedModificationsPictureBox)).EndInit();
            this.noUncommitedModificationsPanel.ResumeLayout(false);
            this.noUncommitedModificationsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label noUncommitedModificationsLabel;
        private System.Windows.Forms.Label noUncommittedModificationsDescriptionLabel;
        private System.Windows.Forms.PictureBox noUncommitedModificationsPictureBox;
        private System.Windows.Forms.Panel noUncommitedModificationsPanel;
    }
}
