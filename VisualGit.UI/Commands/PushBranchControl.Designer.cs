namespace VisualGit.UI.Commands
{
    partial class PushBranchControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PushBranchControl));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.localBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.remoteBox = new System.Windows.Forms.ComboBox();
            this.pushAllBox = new System.Windows.Forms.CheckBox();
            this.forceBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.localBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.remoteBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.pushAllBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.forceBox, 1, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // localBox
            // 
            resources.ApplyResources(this.localBox, "localBox");
            this.localBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.localBox.FormattingEnabled = true;
            this.localBox.Name = "localBox";
            this.localBox.SelectedIndexChanged += new System.EventHandler(this.localBox_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // remoteBox
            // 
            resources.ApplyResources(this.remoteBox, "remoteBox");
            this.remoteBox.FormattingEnabled = true;
            this.remoteBox.Name = "remoteBox";
            // 
            // pushAllBox
            // 
            resources.ApplyResources(this.pushAllBox, "pushAllBox");
            this.pushAllBox.Name = "pushAllBox";
            this.pushAllBox.UseVisualStyleBackColor = true;
            this.pushAllBox.CheckedChanged += new System.EventHandler(this.pushAllBox_CheckedChanged);
            // 
            // forceBox
            // 
            resources.ApplyResources(this.forceBox, "forceBox");
            this.forceBox.Name = "forceBox";
            this.forceBox.UseVisualStyleBackColor = true;
            // 
            // PushBranchControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PushBranchControl";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox localBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox remoteBox;
        private System.Windows.Forms.CheckBox pushAllBox;
        private System.Windows.Forms.CheckBox forceBox;
    }
}
