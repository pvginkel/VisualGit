namespace VisualGit.UI.OptionsPages
{
    partial class UserToolSettingsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserToolSettingsControl));
            this.diffExeBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.diffBrowseBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mergeExeBox = new System.Windows.Forms.ComboBox();
            this.mergeBrowseBtn = new System.Windows.Forms.Button();
            this.patchBrowseBtn = new System.Windows.Forms.Button();
            this.patchExeBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // diffExeBox
            // 
            resources.ApplyResources(this.diffExeBox, "diffExeBox");
            this.diffExeBox.DisplayMember = "DisplayName";
            this.diffExeBox.Name = "diffExeBox";
            this.diffExeBox.SelectionChangeCommitted += new System.EventHandler(this.tool_selectionCommitted);
            this.diffExeBox.TextChanged += new System.EventHandler(this.diffExeBox_TextChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // diffBrowseBtn
            // 
            resources.ApplyResources(this.diffBrowseBtn, "diffBrowseBtn");
            this.diffBrowseBtn.Name = "diffBrowseBtn";
            this.diffBrowseBtn.UseVisualStyleBackColor = true;
            this.diffBrowseBtn.Click += new System.EventHandler(this.diffBrowseBtn_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // mergeExeBox
            // 
            resources.ApplyResources(this.mergeExeBox, "mergeExeBox");
            this.mergeExeBox.DisplayMember = "DisplayName";
            this.mergeExeBox.Name = "mergeExeBox";
            this.mergeExeBox.ValueMember = "ToolTemplate";
            this.mergeExeBox.SelectionChangeCommitted += new System.EventHandler(this.tool_selectionCommitted);
            // 
            // mergeBrowseBtn
            // 
            resources.ApplyResources(this.mergeBrowseBtn, "mergeBrowseBtn");
            this.mergeBrowseBtn.Name = "mergeBrowseBtn";
            this.mergeBrowseBtn.UseVisualStyleBackColor = true;
            this.mergeBrowseBtn.Click += new System.EventHandler(this.mergeBrowseBtn_Click);
            // 
            // patchBrowseBtn
            // 
            resources.ApplyResources(this.patchBrowseBtn, "patchBrowseBtn");
            this.patchBrowseBtn.Name = "patchBrowseBtn";
            this.patchBrowseBtn.UseVisualStyleBackColor = true;
            this.patchBrowseBtn.Click += new System.EventHandler(this.patchBrowseBtn_Click);
            // 
            // patchExeBox
            // 
            resources.ApplyResources(this.patchExeBox, "patchExeBox");
            this.patchExeBox.DisplayMember = "DisplayName";
            this.patchExeBox.Name = "patchExeBox";
            this.patchExeBox.ValueMember = "ToolTemplate";
            this.patchExeBox.SelectedIndexChanged += new System.EventHandler(this.tool_selectionCommitted);
            this.patchExeBox.SelectionChangeCommitted += new System.EventHandler(this.tool_selectionCommitted);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.patchBrowseBtn, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.patchExeBox, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.mergeBrowseBtn, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.mergeExeBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.diffBrowseBtn, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.diffExeBox, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // UserToolSettingsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "UserToolSettingsControl";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox diffExeBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button diffBrowseBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox mergeExeBox;
        private System.Windows.Forms.Button mergeBrowseBtn;
        private System.Windows.Forms.Button patchBrowseBtn;
        private System.Windows.Forms.ComboBox patchExeBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
