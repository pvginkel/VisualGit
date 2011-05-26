using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI.PropertyEditors
{
    partial class KeywordsPropertyEditor
    {
        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeywordsPropertyEditor));
			this.keywordList = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// keywordList
			// 
			this.keywordList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.keywordList, "keywordList");
			this.keywordList.FormattingEnabled = true;
			this.keywordList.Items.AddRange(new object[] {
            resources.GetString("keywordList.Items")});
			this.keywordList.Name = "keywordList";
			this.keywordList.Sorted = true;
			this.keywordList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
			// 
			// KeywordsPropertyEditor
			// 
			this.Controls.Add(this.keywordList);
			this.Name = "KeywordsPropertyEditor";
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.CheckedListBox keywordList;
    }
}
