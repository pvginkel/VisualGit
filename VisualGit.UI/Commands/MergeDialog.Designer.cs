namespace VisualGit.UI.Commands
{
    partial class MergeDialog
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.mergeStrategyBox = new System.Windows.Forms.ComboBox();
            this.doNotCommitBox = new System.Windows.Forms.CheckBox();
            this.squashCommitsBox = new System.Windows.Forms.CheckBox();
            this.currentBranchBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.fastForwardRadioBox = new System.Windows.Forms.RadioButton();
            this.alwaysCreateCommitRadioBox = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tagBox = new System.Windows.Forms.ComboBox();
            this.trackingBranchBox = new System.Windows.Forms.ComboBox();
            this.localBranchBox = new System.Windows.Forms.ComboBox();
            this.localBranchRadioBox = new System.Windows.Forms.RadioButton();
            this.trackingBranchRadioBox = new System.Windows.Forms.RadioButton();
            this.tagRadioBox = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.strategyBox = new System.Windows.Forms.PictureBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strategyBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
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
            this.panel3.Size = new System.Drawing.Size(581, 360);
            this.panel3.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.currentBranchBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(565, 344);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(3, 215);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 3, 8, 8);
            this.groupBox1.Size = new System.Drawing.Size(559, 97);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&Advanced settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.mergeStrategyBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.doNotCommitBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.squashCommitsBox, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(543, 73);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 27);
            this.label3.TabIndex = 0;
            this.label3.Text = "&Merge strategy:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mergeStrategyBox
            // 
            this.mergeStrategyBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mergeStrategyBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mergeStrategyBox.FormattingEnabled = true;
            this.mergeStrategyBox.Location = new System.Drawing.Point(89, 3);
            this.mergeStrategyBox.Name = "mergeStrategyBox";
            this.mergeStrategyBox.Size = new System.Drawing.Size(451, 21);
            this.mergeStrategyBox.TabIndex = 1;
            // 
            // doNotCommitBox
            // 
            this.doNotCommitBox.AutoSize = true;
            this.doNotCommitBox.Location = new System.Drawing.Point(89, 53);
            this.doNotCommitBox.Name = "doNotCommitBox";
            this.doNotCommitBox.Size = new System.Drawing.Size(94, 17);
            this.doNotCommitBox.TabIndex = 3;
            this.doNotCommitBox.Text = "Do &not commit";
            this.doNotCommitBox.UseVisualStyleBackColor = true;
            // 
            // squashCommitsBox
            // 
            this.squashCommitsBox.AutoSize = true;
            this.squashCommitsBox.Location = new System.Drawing.Point(89, 30);
            this.squashCommitsBox.Name = "squashCommitsBox";
            this.squashCommitsBox.Size = new System.Drawing.Size(103, 17);
            this.squashCommitsBox.TabIndex = 2;
            this.squashCommitsBox.Text = "S&quash commits";
            this.squashCommitsBox.UseVisualStyleBackColor = true;
            // 
            // currentBranchBox
            // 
            this.currentBranchBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.currentBranchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.currentBranchBox.Location = new System.Drawing.Point(89, 6);
            this.currentBranchBox.Margin = new System.Windows.Forms.Padding(3, 6, 26, 6);
            this.currentBranchBox.Name = "currentBranchBox";
            this.currentBranchBox.ReadOnly = true;
            this.currentBranchBox.Size = new System.Drawing.Size(450, 13);
            this.currentBranchBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Current branch:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.groupBox2.Size = new System.Drawing.Size(559, 70);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Merge strategy";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.fastForwardRadioBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.alwaysCreateCommitRadioBox, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(543, 46);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // fastForwardRadioBox
            // 
            this.fastForwardRadioBox.AutoSize = true;
            this.fastForwardRadioBox.Checked = true;
            this.fastForwardRadioBox.Location = new System.Drawing.Point(3, 3);
            this.fastForwardRadioBox.Name = "fastForwardRadioBox";
            this.fastForwardRadioBox.Size = new System.Drawing.Size(260, 17);
            this.fastForwardRadioBox.TabIndex = 0;
            this.fastForwardRadioBox.TabStop = true;
            this.fastForwardRadioBox.Text = "&Keep a single branch line if possible (fast forward).";
            this.fastForwardRadioBox.UseVisualStyleBackColor = true;
            // 
            // alwaysCreateCommitRadioBox
            // 
            this.alwaysCreateCommitRadioBox.AutoSize = true;
            this.alwaysCreateCommitRadioBox.Location = new System.Drawing.Point(3, 26);
            this.alwaysCreateCommitRadioBox.Name = "alwaysCreateCommitRadioBox";
            this.alwaysCreateCommitRadioBox.Size = new System.Drawing.Size(194, 17);
            this.alwaysCreateCommitRadioBox.TabIndex = 1;
            this.alwaysCreateCommitRadioBox.Text = "&Always create a new merge commit.";
            this.alwaysCreateCommitRadioBox.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(403, 315);
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
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(84, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.AutoSize = true;
            this.groupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox3, 2);
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 28);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(8, 3, 8, 8);
            this.groupBox3.Size = new System.Drawing.Size(559, 105);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Merge &with";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.tagBox, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.trackingBranchBox, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.localBranchBox, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.localBranchRadioBox, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.trackingBranchRadioBox, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.tagRadioBox, 0, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(8, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(543, 81);
            this.tableLayoutPanel4.TabIndex = 8;
            // 
            // tagBox
            // 
            this.tagBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.tagBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagBox.FormattingEnabled = true;
            this.tagBox.Location = new System.Drawing.Point(115, 57);
            this.tagBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.tagBox.Name = "tagBox";
            this.tagBox.Size = new System.Drawing.Size(402, 21);
            this.tagBox.TabIndex = 5;
            // 
            // trackingBranchBox
            // 
            this.trackingBranchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.trackingBranchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackingBranchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trackingBranchBox.FormattingEnabled = true;
            this.trackingBranchBox.Location = new System.Drawing.Point(115, 30);
            this.trackingBranchBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.trackingBranchBox.Name = "trackingBranchBox";
            this.trackingBranchBox.Size = new System.Drawing.Size(402, 21);
            this.trackingBranchBox.TabIndex = 3;
            // 
            // localBranchBox
            // 
            this.localBranchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.localBranchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localBranchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.localBranchBox.FormattingEnabled = true;
            this.localBranchBox.Location = new System.Drawing.Point(115, 3);
            this.localBranchBox.Margin = new System.Windows.Forms.Padding(3, 3, 26, 3);
            this.localBranchBox.Name = "localBranchBox";
            this.localBranchBox.Size = new System.Drawing.Size(402, 21);
            this.localBranchBox.TabIndex = 1;
            // 
            // localBranchRadioBox
            // 
            this.localBranchRadioBox.AutoSize = true;
            this.localBranchRadioBox.Checked = true;
            this.localBranchRadioBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.localBranchRadioBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.localBranchRadioBox.Location = new System.Drawing.Point(3, 3);
            this.localBranchRadioBox.Name = "localBranchRadioBox";
            this.localBranchRadioBox.Size = new System.Drawing.Size(90, 21);
            this.localBranchRadioBox.TabIndex = 0;
            this.localBranchRadioBox.TabStop = true;
            this.localBranchRadioBox.Text = "&Local branch:";
            this.localBranchRadioBox.UseVisualStyleBackColor = true;
            this.localBranchRadioBox.CheckedChanged += new System.EventHandler(this.localBranchRadioBox_CheckedChanged);
            // 
            // trackingBranchRadioBox
            // 
            this.trackingBranchRadioBox.AutoSize = true;
            this.trackingBranchRadioBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.trackingBranchRadioBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.trackingBranchRadioBox.Location = new System.Drawing.Point(3, 30);
            this.trackingBranchRadioBox.Name = "trackingBranchRadioBox";
            this.trackingBranchRadioBox.Size = new System.Drawing.Size(106, 21);
            this.trackingBranchRadioBox.TabIndex = 2;
            this.trackingBranchRadioBox.Text = "Trac&king branch:";
            this.trackingBranchRadioBox.UseVisualStyleBackColor = true;
            this.trackingBranchRadioBox.CheckedChanged += new System.EventHandler(this.trackingBranchRadioBox_CheckedChanged);
            // 
            // tagRadioBox
            // 
            this.tagRadioBox.AutoSize = true;
            this.tagRadioBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.tagRadioBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tagRadioBox.Location = new System.Drawing.Point(3, 57);
            this.tagRadioBox.Name = "tagRadioBox";
            this.tagRadioBox.Size = new System.Drawing.Size(47, 21);
            this.tagRadioBox.TabIndex = 4;
            this.tagRadioBox.Text = "&Tag:";
            this.tagRadioBox.UseVisualStyleBackColor = true;
            this.tagRadioBox.CheckedChanged += new System.EventHandler(this.tagRadioBox_CheckedChanged);
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
            this.panel1.Size = new System.Drawing.Size(94, 360);
            this.panel1.TabIndex = 2;
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
            this.panel2.Size = new System.Drawing.Size(93, 360);
            this.panel2.TabIndex = 0;
            // 
            // strategyBox
            // 
            this.strategyBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.strategyBox.Location = new System.Drawing.Point(8, 8);
            this.strategyBox.Name = "strategyBox";
            this.strategyBox.Size = new System.Drawing.Size(77, 344);
            this.strategyBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.strategyBox.TabIndex = 0;
            this.strategyBox.TabStop = false;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // MergeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(675, 360);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MergeDialog";
            this.Text = "Merge";
            this.Load += new System.EventHandler(this.MergeDialog_Load);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.strategyBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox mergeStrategyBox;
        private System.Windows.Forms.CheckBox doNotCommitBox;
        private System.Windows.Forms.CheckBox squashCommitsBox;
        private System.Windows.Forms.TextBox currentBranchBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.RadioButton fastForwardRadioBox;
        private System.Windows.Forms.RadioButton alwaysCreateCommitRadioBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox strategyBox;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ComboBox tagBox;
        private System.Windows.Forms.ComboBox trackingBranchBox;
        private System.Windows.Forms.ComboBox localBranchBox;
        private System.Windows.Forms.RadioButton localBranchRadioBox;
        private System.Windows.Forms.RadioButton trackingBranchRadioBox;
        private System.Windows.Forms.RadioButton tagRadioBox;
    }
}