namespace VisualGit.UI.Commands
{
    partial class CloneDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloneDialog));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.urlBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.destinationBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tagRadioBox = new System.Windows.Forms.RadioButton();
            this.tagBox = new System.Windows.Forms.ComboBox();
            this.branchBox = new System.Windows.Forms.ComboBox();
            this.branchRadioBox = new System.Windows.Forms.RadioButton();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.urlBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.destinationBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.browseButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // urlBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.urlBox, 2);
            resources.ApplyResources(this.urlBox, "urlBox");
            this.urlBox.FormattingEnabled = true;
            this.urlBox.Name = "urlBox";
            this.urlBox.Leave += new System.EventHandler(this.urlBox_Leave);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // destinationBox
            // 
            this.destinationBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.destinationBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            resources.ApplyResources(this.destinationBox, "destinationBox");
            this.destinationBox.Name = "destinationBox";
            this.destinationBox.SizeChanged += new System.EventHandler(this.destinationBox_SizeChanged);
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 3);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 3);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.tagRadioBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tagBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.branchBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.branchRadioBox, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // tagRadioBox
            // 
            resources.ApplyResources(this.tagRadioBox, "tagRadioBox");
            this.tagRadioBox.Name = "tagRadioBox";
            this.tagRadioBox.UseVisualStyleBackColor = true;
            this.tagRadioBox.CheckedChanged += new System.EventHandler(this.tagRadioBox_CheckedChanged);
            // 
            // tagBox
            // 
            resources.ApplyResources(this.tagBox, "tagBox");
            this.tagBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagBox.FormattingEnabled = true;
            this.tagBox.Name = "tagBox";
            // 
            // branchBox
            // 
            resources.ApplyResources(this.branchBox, "branchBox");
            this.branchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.branchBox.FormattingEnabled = true;
            this.branchBox.Name = "branchBox";
            // 
            // branchRadioBox
            // 
            resources.ApplyResources(this.branchRadioBox, "branchRadioBox");
            this.branchRadioBox.Checked = true;
            this.branchRadioBox.Name = "branchRadioBox";
            this.branchRadioBox.TabStop = true;
            this.branchRadioBox.UseVisualStyleBackColor = true;
            this.branchRadioBox.CheckedChanged += new System.EventHandler(this.branchRadioBox_CheckedChanged);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // CloneDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CloneDialog";
            this.Load += new System.EventHandler(this.CloneDialog_Load);
            this.Validating += new System.ComponentModel.CancelEventHandler(this.CloneDialog_Validating);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox urlBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox destinationBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton tagRadioBox;
        private System.Windows.Forms.ComboBox tagBox;
        private System.Windows.Forms.ComboBox branchBox;
        private System.Windows.Forms.RadioButton branchRadioBox;
    }
}