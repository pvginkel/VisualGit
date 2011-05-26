using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.RepositoryExplorer;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.UI.SccManagement
{
    public partial class CopyToDialog : VSContainerForm
    {
        public CopyToDialog()
        {
            InitializeComponent();
        }

        Uri _rootUri;
        public Uri RootUri
        {
            get { return _rootUri; }
            set { _rootUri = value; }
        }

        public string LogMessage
        {
            get { return logMessage.Text; }
        }

        private void toUrlBrowse_Click(object sender, EventArgs e)
        {
            using (RepositoryFolderBrowserDialog dlg = new RepositoryFolderBrowserDialog())
            {
                dlg.RootUri = RootUri;
                dlg.EnableNewFolderButton = true;
                Uri r;

                if (Uri.TryCreate(toUrlBox.Text, UriKind.Absolute, out r))
                    dlg.SelectedUri = r;

                if (dlg.ShowDialog(Context) == DialogResult.OK)
                {
                    if (dlg.SelectedUri != null)
                        toUrlBox.Text = dlg.SelectedUri.AbsoluteUri;
                }
            }
        }

        private void toUrlBox_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (SelectedUri != null);
        }

        public Uri SelectedUri
        {
            get
            {
                Uri uri;

                if (!string.IsNullOrEmpty(toUrlBox.Text) &&
                    Uri.TryCreate(toUrlBox.Text, UriKind.Absolute, out uri))
                {
                    return uri;
                }

                return null;
            }
            set
            {
                if (value != null)
                    toUrlBox.Text = value.ToString();
                else
                    toUrlBox.Text = "";
            }
        }        
    }
}
