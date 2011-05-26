using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class PlainPropertyEditor
    {
        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlainPropertyEditor));
			this.valueTextBox = new System.Windows.Forms.TextBox();
			this.plainGroupBox = new System.Windows.Forms.GroupBox();
			this.conflictToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.plainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// valueTextBox
			// 
			this.valueTextBox.AcceptsReturn = true;
			this.valueTextBox.AcceptsTab = true;
			resources.ApplyResources(this.valueTextBox, "valueTextBox");
			this.valueTextBox.Name = "valueTextBox";
			this.valueTextBox.TextChanged += new System.EventHandler(this.valueTextBox_TextChanged);
			// 
			// plainGroupBox
			// 
			this.plainGroupBox.Controls.Add(this.valueTextBox);
			resources.ApplyResources(this.plainGroupBox, "plainGroupBox");
			this.plainGroupBox.Name = "plainGroupBox";
			this.plainGroupBox.TabStop = false;
			// 
			// PlainPropertyEditor
			// 
			this.Controls.Add(this.plainGroupBox);
			this.Name = "PlainPropertyEditor";
			this.plainGroupBox.ResumeLayout(false);
			this.plainGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.GroupBox plainGroupBox;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip conflictToolTip;

    }
}
