using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.GitLog;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.UI.SccManagement
{
    public partial class CreateTagDialog : VSContainerForm
    {
        public CreateTagDialog()
        {
            InitializeComponent();

            UpdateEnabled();
        }

        public bool AnnotatedTag
        {
            get { return annotatedTagBox.Checked; }
        }

        public string LogMessage
        {
            get { return logMessage.Text; }
        }

        public string TagName
        {
            get
            {
                string tagName = tagBox.Text.Trim();

                if (String.Empty.Equals(tagName))
                    return null;
                else
                    return tagName;
            }
        }

        private void toUrlBox_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = TagName != null;
        }

        private void annotatedTagBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            logMessage.Enabled = annotatedTagBox.Checked;
        }
    }
}
