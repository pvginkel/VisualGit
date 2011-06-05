using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI
{
    public partial class UsernamePasswordCredentialsDialog : VSDialogForm
    {
        GitCredentialItem _usernameItem;
        GitCredentialItem _passwordItem;

        public UsernamePasswordCredentialsDialog()
        {
            InitializeComponent();
        }

        public GitCredentialItem UsernameItem
        {
            get { return _usernameItem; }
            set { _usernameItem = value; }
        }

        public GitCredentialItem PasswordItem
        {
            get { return _passwordItem; }
            set { _passwordItem = value; }
        }

        private void UsernamePasswordCredentialsDialog_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_usernameItem.PromptText))
                promptLabel.Text = _usernameItem.PromptText;
            else if (!String.IsNullOrEmpty(_passwordItem.PromptText))
                promptLabel.Text = _passwordItem.PromptText;

            usernameBox.Text = _usernameItem.Value;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _usernameItem.Value = usernameBox.Text;
            _passwordItem.Value = passwordBox.Text;

            DialogResult = DialogResult.OK;
        }
    }
}
