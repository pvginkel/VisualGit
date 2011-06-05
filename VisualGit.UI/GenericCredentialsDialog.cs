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
    public partial class GenericCredentialsDialog : VSDialogForm
    {
        private GitCredentialItem _item;

        public GenericCredentialsDialog()
        {
            InitializeComponent();
        }

        public GitCredentialItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        private void GenericCredentialsDialog_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_item.PromptText))
                promptLabel.Text = _item.PromptText;

            credentialBox.Text = _item.Value;

            if (Item.Type != GitCredentialsType.Username)
                credentialBox.UseSystemPasswordChar = true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _item.Value = credentialBox.Text;

            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
