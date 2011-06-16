namespace VisualGit.UI.OptionsPages
{
    partial class EnvironmentSettingsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnvironmentSettingsControl));
            this.authenticationEdit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.proxyEdit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.interactiveMergeOnConflict = new System.Windows.Forms.CheckBox();
            this.autoAddFiles = new System.Windows.Forms.CheckBox();
            this.flashWindowAfterOperation = new System.Windows.Forms.CheckBox();
            this.autoLockFiles = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pcDefaultDoubleClick = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // authenticationEdit
            // 
            resources.ApplyResources(this.authenticationEdit, "authenticationEdit");
            this.authenticationEdit.Name = "authenticationEdit";
            this.authenticationEdit.UseVisualStyleBackColor = true;
            this.authenticationEdit.Click += new System.EventHandler(this.authenticationEdit_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.authenticationEdit, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.proxyEdit, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // interactiveMergeOnConflict
            // 
            resources.ApplyResources(this.interactiveMergeOnConflict, "interactiveMergeOnConflict");
            this.interactiveMergeOnConflict.Name = "interactiveMergeOnConflict";
            this.interactiveMergeOnConflict.UseVisualStyleBackColor = true;
            // 
            // autoAddFiles
            // 
            resources.ApplyResources(this.autoAddFiles, "autoAddFiles");
            this.autoAddFiles.Name = "autoAddFiles";
            this.autoAddFiles.UseVisualStyleBackColor = true;
            // 
            // flashWindowAfterOperation
            // 
            resources.ApplyResources(this.flashWindowAfterOperation, "flashWindowAfterOperation");
            this.flashWindowAfterOperation.Name = "flashWindowAfterOperation";
            this.flashWindowAfterOperation.UseVisualStyleBackColor = true;
            // 
            // autoLockFiles
            // 
            resources.ApplyResources(this.autoLockFiles, "autoLockFiles");
            this.autoLockFiles.Name = "autoLockFiles";
            this.autoLockFiles.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // pcDefaultDoubleClick
            // 
            resources.ApplyResources(this.pcDefaultDoubleClick, "pcDefaultDoubleClick");
            this.pcDefaultDoubleClick.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pcDefaultDoubleClick.FormattingEnabled = true;
            this.pcDefaultDoubleClick.Items.AddRange(new object[] {
            resources.GetString("pcDefaultDoubleClick.Items"),
            resources.GetString("pcDefaultDoubleClick.Items1")});
            this.pcDefaultDoubleClick.Name = "pcDefaultDoubleClick";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.autoAddFiles, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.flashWindowAfterOperation, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.pcDefaultDoubleClick, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.interactiveMergeOnConflict, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.autoLockFiles, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // EnvironmentSettingsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EnvironmentSettingsControl";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button authenticationEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button proxyEdit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox interactiveMergeOnConflict;
        private System.Windows.Forms.CheckBox autoAddFiles;
        private System.Windows.Forms.CheckBox flashWindowAfterOperation;
        private System.Windows.Forms.CheckBox autoLockFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox pcDefaultDoubleClick;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
