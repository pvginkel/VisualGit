using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public partial class PushDialog : VSDialogForm, IHasErrorProvider
    {
        IPushControl _childControl;

        public PushDialog()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        public PushType Type { get; set; }

        public GitPushArgs Args { get; set; }

        public string RepositoryPath { get; set; }

        public enum PushType
        {
            Tag,
            Branch
        }

        private void PushDialog_Load(object sender, EventArgs e)
        {
            using (var client = Context.GetService<IGitClientPool>().GetNoUIClient())
            {
                LoadChildControl(client);
                LoadRemotes(client);
                LoadRemoteUris(client);
            }
        }

        private void LoadRemoteUris(GitPoolClient client)
        {
            foreach (string value in Config.GetRecentReposUrls())
            {
                if (value != null)
                {
                    if (!urlBox.Items.Contains(value))
                        urlBox.Items.Add(value);
                }
            }
        }

        private void LoadChildControl(GitClient client)
        {
            Text = String.Format(Text, Type);

            switch (Type)
            {
                case PushType.Branch:
                    _childControl = new PushBranchControl();
                    break;

                case PushType.Tag:
                    _childControl = new PushTagControl();
                    break;

                default:
                    throw new NotSupportedException();
            }

            _childControl.Args = Args;
            _childControl.Context = Context;
            _childControl.RepositoryPath = RepositoryPath;
            _childControl.LoadFromClient(client);

            ((Control)_childControl).Dock = DockStyle.Fill;

            containerPanel.Controls.Add((Control)_childControl);
        }

        private void LoadRemotes(GitClient client)
        {
            var config = client.GetConfig(RepositoryPath);

            var currentBranch = client.GetCurrentBranch(RepositoryPath);

            string currentBranchRemote = config.GetString("branch", currentBranch.ShortName, "remote");

            remoteBox.BeginUpdate();
            remoteBox.Items.Clear();

            foreach (string remote in config.GetSubsections("remote"))
            {
                remoteBox.Items.Add(remote);

                if (remote == currentBranchRemote)
                    remoteBox.SelectedIndex = remoteBox.Items.Count - 1;
            }

            remoteBox.EndUpdate();
        }

        private void remoteRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            remoteBox.Enabled = remoteRadioBox.Checked;
            urlBox.Enabled = urlRadioBox.Checked;
        }

        private void urlRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void PushDialog_Shown(object sender, EventArgs e)
        {
            if (remoteRadioBox.Checked)
                remoteBox.Focus();
            else
                urlBox.Focus();
        }

        public ErrorProvider ErrorProvider
        {
            get { return errorProvider1; }
        }

        IVisualGitConfigurationService _config;
        private IVisualGitConfigurationService Config
        {
            get { return _config ?? (_config = GetService<IVisualGitConfigurationService>()); }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            bool ok = true;

            if (remoteRadioBox.Checked && String.Empty.Equals(remoteBox.Text.Trim()))
            {
                ErrorProvider.SetError(remoteBox, CommandStrings.SelectARemote);
                ok = false;
            }
            else
                ErrorProvider.SetError(remoteBox, null);

            if (urlRadioBox.Checked && String.Empty.Equals(urlBox.Text.Trim()))
            {
                ErrorProvider.SetError(urlBox, CommandStrings.SelectAnUrl);
                ok = false;
            }
            else
                ErrorProvider.SetError(urlBox, null);

            if (ok)
            {
                string remote = remoteRadioBox.Checked ? remoteBox.Text.Trim() : null;

                if (_childControl.FlushArgs(remote))
                {
                    if (remoteRadioBox.Checked)
                        Args.Remote = remoteBox.Text.Trim();
                    else
                    {
                        Args.RemoteUri = urlBox.Text.Trim();

                        Config.GetRecentReposUrls().Add(Args.RemoteUri);
                    }

                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
