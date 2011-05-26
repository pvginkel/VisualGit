namespace VisualGit.UI.SccManagement
{
    partial class AddProjectToGit
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddProjectToGit));
			this.markAsManaged = new System.Windows.Forms.CheckBox();
			this.writeUrlInSolution = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// bodyPanel
			// 
			resources.ApplyResources(this.bodyPanel, "bodyPanel");
			// 
			// markAsManaged
			// 
			resources.ApplyResources(this.markAsManaged, "markAsManaged");
			this.markAsManaged.Checked = true;
			this.markAsManaged.CheckState = System.Windows.Forms.CheckState.Checked;
			this.markAsManaged.Name = "markAsManaged";
			this.markAsManaged.UseVisualStyleBackColor = true;
			// 
			// writeUrlInSolution
			// 
			resources.ApplyResources(this.writeUrlInSolution, "writeUrlInSolution");
			this.writeUrlInSolution.Checked = true;
			this.writeUrlInSolution.CheckState = System.Windows.Forms.CheckState.Checked;
			this.writeUrlInSolution.Name = "writeUrlInSolution";
			this.writeUrlInSolution.UseVisualStyleBackColor = true;
			// 
			// AddProjectToGit
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.writeUrlInSolution);
			this.Controls.Add(this.markAsManaged);
			this.Name = "AddProjectToGit";
			this.Controls.SetChildIndex(this.bodyPanel, 0);
			this.Controls.SetChildIndex(this.markAsManaged, 0);
			this.Controls.SetChildIndex(this.writeUrlInSolution, 0);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox markAsManaged;
        private System.Windows.Forms.CheckBox writeUrlInSolution;
    }
}
