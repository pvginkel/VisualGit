using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class ExecutablePropertyEditor
    {

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecutablePropertyEditor));
			this.conflictToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.executableTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// executableTextBox
			// 
			resources.ApplyResources(this.executableTextBox, "executableTextBox");
			this.executableTextBox.Name = "executableTextBox";
			this.executableTextBox.ReadOnly = true;
			// 
			// ExecutablePropertyEditor
			// 
			this.Controls.Add(this.executableTextBox);
			this.Name = "ExecutablePropertyEditor";
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ToolTip conflictToolTip;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TextBox executableTextBox;
    }
}
