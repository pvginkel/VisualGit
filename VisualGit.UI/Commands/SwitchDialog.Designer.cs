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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pathLabel = new System.Windows.Forms.Label();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.toBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tagBox = new System.Windows.Forms.ComboBox();
            this.trackingBranchBox = new System.Windows.Forms.ComboBox();
            this.localBranchBox = new System.Windows.Forms.ComboBox();
            this.localBranchRadioBox = new System.Windows.Forms.RadioButton();
            this.trackingBranchRadioBox = new System.Windows.Forms.RadioButton();
            this.tagRadioBox = new System.Windows.Forms.RadioButton();
            this.revisionRadioBox = new System.Windows.Forms.RadioButton();
            this.versionBox = new VisualGit.UI.PathSelector.VersionSelector();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.forceBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.switchBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.toBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // switchBox
            // 
            resources.ApplyResources(this.switchBox, "switchBox");
            this.switchBox.Controls.Add(this.tableLayoutPanel1);
            this.switchBox.Name = "switchBox";
            this.switchBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.pathLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pathBox, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // pathLabel
            // 
            resources.ApplyResources(this.pathLabel, "pathLabel");
            this.pathLabel.Name = "pathLabel";
            // 
            // pathBox
            // 
            this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.pathBox, "pathBox");
            this.pathBox.Name = "pathBox";
            this.pathBox.ReadOnly = true;
            // 
            // toBox
            // 
            resources.ApplyResources(this.toBox, "toBox");
            this.toBox.Controls.Add(this.tableLayoutPanel2);
            this.toBox.Name = "toBox";
            this.toBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.tagBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.trackingBranchBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.localBranchBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.localBranchRadioBox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.trackingBranchRadioBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tagRadioBox, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.revisionRadioBox, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.versionBox, 1, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // tagBox
            // 
            this.tagBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            resources.ApplyResources(this.tagBox, "tagBox");
            this.tagBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagBox.FormattingEnabled = true;
            this.tagBox.Name = "tagBox";
            // 
            // trackingBranchBox
            // 
            this.trackingBranchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            resources.ApplyResources(this.trackingBranchBox, "trackingBranchBox");
            this.trackingBranchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trackingBranchBox.FormattingEnabled = true;
            this.trackingBranchBox.Name = "trackingBranchBox";
            // 
            // localBranchBox
            // 
            this.localBranchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            resources.ApplyResources(this.localBranchBox, "localBranchBox");
            this.localBranchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.localBranchBox.FormattingEnabled = true;
            this.localBranchBox.Name = "localBranchBox";
            // 
            // localBranchRadioBox
            // 
            resources.ApplyResources(this.localBranchRadioBox, "localBranchRadioBox");
            this.localBranchRadioBox.Checked = true;
            this.localBranchRadioBox.Name = "localBranchRadioBox";
            this.localBranchRadioBox.TabStop = true;
            this.localBranchRadioBox.UseVisualStyleBackColor = true;
            // 
            // trackingBranchRadioBox
            // 
            resources.ApplyResources(this.trackingBranchRadioBox, "trackingBranchRadioBox");
            this.trackingBranchRadioBox.Name = "trackingBranchRadioBox";
            this.trackingBranchRadioBox.UseVisualStyleBackColor = true;
            // 
            // tagRadioBox
            // 
            resources.ApplyResources(this.tagRadioBox, "tagRadioBox");
            this.tagRadioBox.Name = "tagRadioBox";
            this.tagRadioBox.UseVisualStyleBackColor = true;
            // 
            // revisionRadioBox
            // 
            resources.ApplyResources(this.revisionRadioBox, "revisionRadioBox");
            this.revisionRadioBox.Name = "revisionRadioBox";
            this.revisionRadioBox.UseVisualStyleBackColor = true;
            // 
            // versionBox
            // 
            resources.ApplyResources(this.versionBox, "versionBox");
            this.versionBox.GitOrigin = null;
            this.versionBox.Name = "versionBox";
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
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.switchBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.forceBox, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.toBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // SwitchDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.tableLayoutPanel3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SwitchDialog";
            this.Shown += new System.EventHandler(this.SwitchDialog_Shown);
            this.Validating += new System.ComponentModel.CancelEventHandler(this.SwitchDialog_Validating);
            this.switchBox.ResumeLayout(false);
            this.switchBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toBox.ResumeLayout(false);
            this.toBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox switchBox;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.GroupBox toBox;
        private System.Windows.Forms.ComboBox localBranchBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.CheckBox forceBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox tagBox;
        private System.Windows.Forms.ComboBox trackingBranchBox;
        private System.Windows.Forms.RadioButton localBranchRadioBox;
        private System.Windows.Forms.RadioButton trackingBranchRadioBox;
        private System.Windows.Forms.RadioButton tagRadioBox;
        private System.Windows.Forms.RadioButton revisionRadioBox;
        private PathSelector.VersionSelector versionBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
