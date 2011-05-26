using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class EolStylePropertyEditor
    {
        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EolStylePropertyEditor));
			this.nativeRadioButton = new System.Windows.Forms.RadioButton();
			this.lfRadioButton = new System.Windows.Forms.RadioButton();
			this.crRadioButton = new System.Windows.Forms.RadioButton();
			this.crlfRdioButton = new System.Windows.Forms.RadioButton();
			this.eolStyleGroupBox = new System.Windows.Forms.GroupBox();
			this.conflictToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.eolStyleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// nativeRadioButton
			// 
			this.nativeRadioButton.Checked = true;
			resources.ApplyResources(this.nativeRadioButton, "nativeRadioButton");
			this.nativeRadioButton.Name = "nativeRadioButton";
			this.nativeRadioButton.TabStop = true;
			this.nativeRadioButton.Tag = "native";
			this.nativeRadioButton.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
			// 
			// lfRadioButton
			// 
			resources.ApplyResources(this.lfRadioButton, "lfRadioButton");
			this.lfRadioButton.Name = "lfRadioButton";
			this.lfRadioButton.Tag = "LF";
			this.lfRadioButton.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
			// 
			// crRadioButton
			// 
			resources.ApplyResources(this.crRadioButton, "crRadioButton");
			this.crRadioButton.Name = "crRadioButton";
			this.crRadioButton.Tag = "CR";
			this.crRadioButton.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
			// 
			// crlfRdioButton
			// 
			resources.ApplyResources(this.crlfRdioButton, "crlfRdioButton");
			this.crlfRdioButton.Name = "crlfRdioButton";
			this.crlfRdioButton.Tag = "CRLF";
			this.crlfRdioButton.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
			// 
			// eolStyleGroupBox
			// 
			this.eolStyleGroupBox.Controls.Add(this.nativeRadioButton);
			this.eolStyleGroupBox.Controls.Add(this.crlfRdioButton);
			this.eolStyleGroupBox.Controls.Add(this.lfRadioButton);
			this.eolStyleGroupBox.Controls.Add(this.crRadioButton);
			resources.ApplyResources(this.eolStyleGroupBox, "eolStyleGroupBox");
			this.eolStyleGroupBox.Name = "eolStyleGroupBox";
			this.eolStyleGroupBox.TabStop = false;
			// 
			// EolStylePropertyEditor
			// 
			this.Controls.Add(this.eolStyleGroupBox);
			this.Name = "EolStylePropertyEditor";
			this.eolStyleGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.RadioButton nativeRadioButton;
        private System.Windows.Forms.RadioButton lfRadioButton;
        private System.Windows.Forms.RadioButton crRadioButton;
        private System.Windows.Forms.RadioButton crlfRdioButton;
        private System.Windows.Forms.GroupBox eolStyleGroupBox;
        private System.Windows.Forms.ToolTip conflictToolTip;
        private System.ComponentModel.IContainer components;
    }
}
