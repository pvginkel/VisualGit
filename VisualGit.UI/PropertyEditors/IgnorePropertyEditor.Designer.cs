using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class IgnorePropertyEditor
    {

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IgnorePropertyEditor));
			this.ignoreGroupBox = new System.Windows.Forms.GroupBox();
			this.ignoreTextBox = new System.Windows.Forms.TextBox();
			this.conflictToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.ignoreGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ignoreGroupBox
			// 
			this.ignoreGroupBox.Controls.Add(this.ignoreTextBox);
			resources.ApplyResources(this.ignoreGroupBox, "ignoreGroupBox");
			this.ignoreGroupBox.Name = "ignoreGroupBox";
			this.ignoreGroupBox.TabStop = false;
			// 
			// ignoreTextBox
			// 
			this.ignoreTextBox.AcceptsReturn = true;
			this.ignoreTextBox.AcceptsTab = true;
			resources.ApplyResources(this.ignoreTextBox, "ignoreTextBox");
			this.ignoreTextBox.Name = "ignoreTextBox";
			this.ignoreTextBox.TextChanged += new System.EventHandler(this.ignoreTextBox_TextChanged);
			// 
			// IgnorePropertyEditor
			// 
			this.Controls.Add(this.ignoreGroupBox);
			this.Name = "IgnorePropertyEditor";
			this.ignoreGroupBox.ResumeLayout(false);
			this.ignoreGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.GroupBox ignoreGroupBox;
        private System.Windows.Forms.ToolTip conflictToolTip;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TextBox ignoreTextBox;
        
    }
}
