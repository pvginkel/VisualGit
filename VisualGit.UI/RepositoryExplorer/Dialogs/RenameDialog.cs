using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpSvn;

namespace VisualGit.UI.RepositoryExplorer.Dialogs
{
    public partial class RenameDialog : VSContainerForm
    {
        public RenameDialog()
        {
            InitializeComponent();
        }

        public string NewName
        {
            get { return newNameBox.Text; }
            set { newNameBox.Text = value; }
        }

        public string OldName
        {
            get { return oldNameBox.Text; }
            set { oldNameBox.Text = value; }
        }        

        public string LogMessage
        {
            get { return logMessage.Text; }
        }       
    }
}
