namespace VisualGit.UI.Commands
{
    partial class SwitchDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SwitchDialog));
            this.switchBox = new System.Windows.Forms.GroupBox();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.toBox = new System.Windows.Forms.GroupBox();
            this.toBranchBox = new System.Windows.Forms.ComboBox();
            this.urlLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.forceBox = new System.Windows.Forms.CheckBox();
            this.switchBox.SuspendLayout();
            this.toBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // switchBox
            // 
            resources.ApplyResources(this.switchBox, "switchBox");
            this.switchBox.Controls.Add(this.pathLabel);
            this.switchBox.Controls.Add(this.pathBox);
            this.switchBox.Name = "switchBox";
            this.switchBox.TabStop = false;
            // 
            // pathLabel
            // 
            resources.ApplyResources(this.pathLabel, "pathLabel");
            this.pathLabel.Name = "pathLabel";
            // 
            // pathBox
            // 
            resources.ApplyResources(this.pathBox, "pathBox");
            this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pathBox.Name = "pathBox";
            this.pathBox.ReadOnly = true;
            // 
            // toBox
            // 
            resources.ApplyResources(this.toBox, "toBox");
            this.toBox.Controls.Add(this.toBranchBox);
            this.toBox.Controls.Add(this.urlLabel);
            this.toBox.Name = "toBox";
            this.toBox.TabStop = false;
            // 
            // toBranchBox
            // 
            resources.ApplyResources(this.toBranchBox, "toBranchBox");
            this.toBranchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.toBranchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toBranchBox.FormattingEnabled = true;
            this.toBranchBox.Name = "toBranchBox";
            this.toBranchBox.Validating += new System.ComponentModel.CancelEventHandler(this.toUrlBox_Validating);
            // 
            // urlLabel
            // 
            resources.ApplyResources(this.urlLabel, "urlLabel");
            this.urlLabel.Name = "urlLabel";
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // forceBox
            // 
            resources.ApplyResources(this.forceBox, "forceBox");
            this.forceBox.Name = "forceBox";
            this.forceBox.UseVisualStyleBackColor = true;
            // 
            // SwitchDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.forceBox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.toBox);
            this.Controls.Add(this.switchBox);
            this.Name = "SwitchDialog";
            this.Shown += new System.EventHandler(this.SwitchDialog_Shown);
            this.switchBox.ResumeLayout(false);
            this.switchBox.PerformLayout();
            this.toBox.ResumeLayout(false);
            this.toBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox switchBox;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.GroupBox toBox;
        private System.Windows.Forms.Label urlLabel;
        private System.Windows.Forms.ComboBox toBranchBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.CheckBox forceBox;
    }
}
