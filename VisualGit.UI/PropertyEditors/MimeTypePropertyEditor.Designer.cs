using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class MimeTypePropertyEditor
    {

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MimeTypePropertyEditor));
			this.mimeTextBox = new System.Windows.Forms.TextBox();
			this.mimeGroupBox = new System.Windows.Forms.GroupBox();
			this.conflictToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.mimeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// mimeTextBox
			// 
			resources.ApplyResources(this.mimeTextBox, "mimeTextBox");
			this.mimeTextBox.Name = "mimeTextBox";
			this.mimeTextBox.TextChanged += new System.EventHandler(this.mimeTextBox_TextChanged);
			// 
			// mimeGroupBox
			// 
			this.mimeGroupBox.Controls.Add(this.mimeTextBox);
			resources.ApplyResources(this.mimeGroupBox, "mimeGroupBox");
			this.mimeGroupBox.Name = "mimeGroupBox";
			this.mimeGroupBox.TabStop = false;
			// 
			// MimeTypePropertyEditor
			// 
			this.Controls.Add(this.mimeGroupBox);
			this.Name = "MimeTypePropertyEditor";
			this.mimeGroupBox.ResumeLayout(false);
			this.mimeGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TextBox mimeTextBox;
        private System.Windows.Forms.GroupBox mimeGroupBox;
        private System.Windows.Forms.ToolTip conflictToolTip;
        private System.ComponentModel.IContainer components;

    }
}
