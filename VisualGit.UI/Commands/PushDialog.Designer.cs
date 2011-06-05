namespace VisualGit.UI.Commands
{
    partial class PushDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PushDialog));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.remoteRadioBox = new System.Windows.Forms.RadioButton();
            this.urlRadioBox = new System.Windows.Forms.RadioButton();
            this.remoteBox = new System.Windows.Forms.ComboBox();
            this.urlBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.containerPanel = new System.Windows.Forms.Panel();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.remoteRadioBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.urlRadioBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.remoteBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.urlBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // remoteRadioBox
            // 
            resources.ApplyResources(this.remoteRadioBox, "remoteRadioBox");
            this.remoteRadioBox.Checked = true;
            this.remoteRadioBox.Name = "remoteRadioBox";
            this.remoteRadioBox.TabStop = true;
            this.remoteRadioBox.UseVisualStyleBackColor = true;
            this.remoteRadioBox.CheckedChanged += new System.EventHandler(this.remoteRadioBox_CheckedChanged);
            // 
            // urlRadioBox
            // 
            resources.ApplyResources(this.urlRadioBox, "urlRadioBox");
            this.urlRadioBox.Name = "urlRadioBox";
            this.urlRadioBox.UseVisualStyleBackColor = true;
            this.urlRadioBox.CheckedChanged += new System.EventHandler(this.urlRadioBox_CheckedChanged);
            // 
            // remoteBox
            // 
            resources.ApplyResources(this.remoteBox, "remoteBox");
            this.remoteBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.remoteBox.FormattingEnabled = true;
            this.remoteBox.Name = "remoteBox";
            // 
            // urlBox
            // 
            resources.ApplyResources(this.urlBox, "urlBox");
            this.urlBox.FormattingEnabled = true;
            this.urlBox.Name = "urlBox";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // cancelButton
            // 
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.containerPanel);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // containerPanel
            // 
            resources.ApplyResources(this.containerPanel, "containerPanel");
            this.containerPanel.Name = "containerPanel";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // PushDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PushDialog";
            this.Load += new System.EventHandler(this.PushDialog_Load);
            this.Shown += new System.EventHandler(this.PushDialog_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton remoteRadioBox;
        private System.Windows.Forms.RadioButton urlRadioBox;
        private System.Windows.Forms.ComboBox remoteBox;
        private System.Windows.Forms.ComboBox urlBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel containerPanel;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}