namespace VisualGit.UI.Commands
{
    partial class PullDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.strategyBox = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.remoteBox = new System.Windows.Forms.ComboBox();
            this.remoteRadioBox = new System.Windows.Forms.RadioButton();
            this.urlRadioBox = new System.Windows.Forms.RadioButton();
            this.urlBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.localBranchBox = new System.Windows.Forms.TextBox();
            this.remoteBranchBox = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.mergeRadioBox = new System.Windows.Forms.RadioButton();
            this.rebaseRadioBox = new System.Windows.Forms.RadioButton();
            this.fetchRadioBox = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strategyBox)).BeginInit();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 1, 0);
            this.panel1.Size = new System.Drawing.Size(94, 294);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.strategyBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(8);
            this.panel2.Size = new System.Drawing.Size(93, 294);
            this.panel2.TabIndex = 0;
            // 
            // strategyBox
            // 
            this.strategyBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.strategyBox.Location = new System.Drawing.Point(8, 8);
            this.strategyBox.Name = "strategyBox";
            this.strategyBox.Size = new System.Drawing.Size(77, 278);
            this.strategyBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.strategyBox.TabIndex = 0;
            this.strategyBox.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.Controls.Add(this.tableLayoutPanel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(94, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(8);
            this.panel3.Size = new System.Drawing.Size(464, 294);
            this.panel3.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.remoteBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.remoteRadioBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.urlRadioBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.urlBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 278);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // remoteBox
            // 
            this.remoteBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.remoteBox.FormattingEnabled = true;
            this.remoteBox.Location = new System.Drawing.Point(74, 3);
            this.remoteBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.remoteBox.Name = "remoteBox";
            this.remoteBox.Size = new System.Drawing.Size(348, 21);
            this.remoteBox.TabIndex = 1;
            // 
            // remoteRadioBox
            // 
            this.remoteRadioBox.AutoSize = true;
            this.remoteRadioBox.Checked = true;
            this.remoteRadioBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteRadioBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.remoteRadioBox.Location = new System.Drawing.Point(3, 3);
            this.remoteRadioBox.Name = "remoteRadioBox";
            this.remoteRadioBox.Size = new System.Drawing.Size(65, 21);
            this.remoteRadioBox.TabIndex = 0;
            this.remoteRadioBox.TabStop = true;
            this.remoteRadioBox.Text = "Re&mote:";
            this.remoteRadioBox.UseVisualStyleBackColor = true;
            this.remoteRadioBox.CheckedChanged += new System.EventHandler(this.remoteRadioBox_CheckedChanged);
            // 
            // urlRadioBox
            // 
            this.urlRadioBox.AutoSize = true;
            this.urlRadioBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.urlRadioBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.urlRadioBox.Location = new System.Drawing.Point(3, 30);
            this.urlRadioBox.Name = "urlRadioBox";
            this.urlRadioBox.Size = new System.Drawing.Size(65, 21);
            this.urlRadioBox.TabIndex = 2;
            this.urlRadioBox.Text = "&Url:";
            this.urlRadioBox.UseVisualStyleBackColor = true;
            this.urlRadioBox.CheckedChanged += new System.EventHandler(this.urlRadioBox_CheckedChanged);
            // 
            // urlBox
            // 
            this.urlBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.urlBox.FormattingEnabled = true;
            this.urlBox.Location = new System.Drawing.Point(74, 30);
            this.urlBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.urlBox.Name = "urlBox";
            this.urlBox.Size = new System.Drawing.Size(348, 21);
            this.urlBox.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(3, 57);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 3, 8, 8);
            this.groupBox1.Size = new System.Drawing.Size(442, 76);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Branch";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.localBranchBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.remoteBranchBox, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(426, 52);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Local branch:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 27);
            this.label2.TabIndex = 2;
            this.label2.Text = "R&emote branch:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // localBranchBox
            // 
            this.localBranchBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.localBranchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localBranchBox.Location = new System.Drawing.Point(92, 6);
            this.localBranchBox.Margin = new System.Windows.Forms.Padding(3, 6, 26, 6);
            this.localBranchBox.Name = "localBranchBox";
            this.localBranchBox.ReadOnly = true;
            this.localBranchBox.Size = new System.Drawing.Size(308, 13);
            this.localBranchBox.TabIndex = 1;
            // 
            // remoteBranchBox
            // 
            this.remoteBranchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteBranchBox.FormattingEnabled = true;
            this.remoteBranchBox.Location = new System.Drawing.Point(92, 28);
            this.remoteBranchBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.remoteBranchBox.Name = "remoteBranchBox";
            this.remoteBranchBox.Size = new System.Drawing.Size(308, 21);
            this.remoteBranchBox.TabIndex = 3;
            this.remoteBranchBox.Enter += new System.EventHandler(this.remoteBranchBox_Enter);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 2);
            this.groupBox2.Controls.Add(this.tableLayoutPanel3);
            this.groupBox2.Location = new System.Drawing.Point(3, 139);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(8, 3, 8, 8);
            this.groupBox2.Size = new System.Drawing.Size(442, 107);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Merge strategy";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.mergeRadioBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.rebaseRadioBox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.fetchRadioBox, 0, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(426, 83);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // mergeRadioBox
            // 
            this.mergeRadioBox.AutoSize = true;
            this.mergeRadioBox.Checked = true;
            this.mergeRadioBox.Location = new System.Drawing.Point(3, 3);
            this.mergeRadioBox.Name = "mergeRadioBox";
            this.mergeRadioBox.Size = new System.Drawing.Size(213, 17);
            this.mergeRadioBox.TabIndex = 0;
            this.mergeRadioBox.TabStop = true;
            this.mergeRadioBox.Text = "&Merge remote branch to current branch.";
            this.mergeRadioBox.UseVisualStyleBackColor = true;
            this.mergeRadioBox.CheckedChanged += new System.EventHandler(this.mergeRadioBox_CheckedChanged);
            // 
            // rebaseRadioBox
            // 
            this.rebaseRadioBox.AutoEllipsis = true;
            this.rebaseRadioBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rebaseRadioBox.Location = new System.Drawing.Point(3, 26);
            this.rebaseRadioBox.Name = "rebaseRadioBox";
            this.rebaseRadioBox.Size = new System.Drawing.Size(420, 31);
            this.rebaseRadioBox.TabIndex = 1;
            this.rebaseRadioBox.Text = "&Rebase remote branch to current branch creating a linear history. It is recommen" +
    "ded to choose a remote branch when using rebase. Use with caution!";
            this.rebaseRadioBox.UseVisualStyleBackColor = true;
            this.rebaseRadioBox.CheckedChanged += new System.EventHandler(this.rebaseRadioBox_CheckedChanged);
            // 
            // fetchRadioBox
            // 
            this.fetchRadioBox.AutoSize = true;
            this.fetchRadioBox.Location = new System.Drawing.Point(3, 63);
            this.fetchRadioBox.Name = "fetchRadioBox";
            this.fetchRadioBox.Size = new System.Drawing.Size(217, 17);
            this.fetchRadioBox.TabIndex = 2;
            this.fetchRadioBox.Text = "Do not merge. Only &fetch remote branch.";
            this.fetchRadioBox.UseVisualStyleBackColor = true;
            this.fetchRadioBox.CheckedChanged += new System.EventHandler(this.fetchRadioBox_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(286, 249);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 29);
            this.flowLayoutPanel1.TabIndex = 6;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(3, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(84, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // PullDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(558, 294);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "PullDialog";
            this.Text = "Pull";
            this.Load += new System.EventHandler(this.PullDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.strategyBox)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox strategyBox;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox remoteBox;
        private System.Windows.Forms.RadioButton remoteRadioBox;
        private System.Windows.Forms.RadioButton urlRadioBox;
        private System.Windows.Forms.ComboBox urlBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox localBranchBox;
        private System.Windows.Forms.ComboBox remoteBranchBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.RadioButton mergeRadioBox;
        private System.Windows.Forms.RadioButton rebaseRadioBox;
        private System.Windows.Forms.RadioButton fetchRadioBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}