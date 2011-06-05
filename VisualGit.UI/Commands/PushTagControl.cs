using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public partial class PushTagControl : UserControl, IPushControl
    {
        public PushTagControl()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            tagBox.Enabled = !pushAllBox.Checked;
        }

        public string RepositoryPath { get; set; }

        public IVisualGitServiceProvider Context { get; set; }

        public GitPushArgs Args { get; set; }

        public void LoadFromClient(GitClient client)
        {
            tagBox.BeginUpdate();
            tagBox.Items.Clear();

            foreach (var @ref in client.GetRefs(RepositoryPath))
            {
                switch (@ref.Type)
                {
                    case GitRefType.Tag:
                        tagBox.Items.Add(@ref.ShortName);
                        break;
                }
            }

            if (tagBox.Items.Count > 0)
                tagBox.SelectedIndex = 0;

            tagBox.EndUpdate();
        }

        private void pushAllBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private ErrorProvider _errorProvider;
        private ErrorProvider ErrorProvider
        {
            get { return _errorProvider ?? (_errorProvider = ((IHasErrorProvider)FindForm()).ErrorProvider); }
        }

        private void tagBox_Validating(object sender, CancelEventArgs e)
        {
            if (!pushAllBox.Checked && String.Empty.Equals(tagBox.Text.Trim()))
            {
                ErrorProvider.SetError(tagBox, CommandStrings.SelectATag);
                e.Cancel = true;
            }
            else
                ErrorProvider.SetError(tagBox, null);
        }


        public bool FlushArgs(string remote)
        {
            Args.AllTags = pushAllBox.Checked;
            Args.Force = forceBox.Checked;
            Args.Tag = new GitRef(tagBox.Text);

            return true;
        }
    }
}
