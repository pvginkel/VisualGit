namespace VisualGit.UI.Commands
{
    partial class PushTagControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PushTagControl));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.pushAllBox = new System.Windows.Forms.CheckBox();
            this.forceBox = new System.Windows.Forms.CheckBox();
            this.tagBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pushAllBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.forceBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tagBox, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
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
            // tagBox
            // 
            resources.ApplyResources(this.tagBox, "tagBox");
            this.tagBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagBox.FormattingEnabled = true;
            this.tagBox.Name = "tagBox";
            this.tagBox.Validating += new System.ComponentModel.CancelEventHandler(this.tagBox_Validating);
            // 
            // PushTagControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PushTagControl";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox pushAllBox;
        private System.Windows.Forms.CheckBox forceBox;
        private System.Windows.Forms.ComboBox tagBox;
    }
}
