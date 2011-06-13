using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;
using System.IO;

namespace VisualGit.UI.Commands
{
    public partial class CloneDialog : VSDialogForm
    {
        private string _retrievedRemotesFor = String.Empty;

        public CloneDialog()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        protected void LoadRemoteUris(ComboBox urlBox)
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

        IVisualGitConfigurationService _config;
        protected IVisualGitConfigurationService Config
        {
            get { return _config ?? (_config = GetService<IVisualGitConfigurationService>()); }
        }

        private void CloneDialog_Load(object sender, EventArgs e)
        {
            LoadRemoteUris(urlBox);
        }

        private void destinationBox_SizeChanged(object sender, EventArgs e)
        {
            browseButton.Height = destinationBox.Height + destinationBox.Margin.Vertical;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                fbd.SelectedPath = destinationBox.Text;
                fbd.Description = "Select the location where you wish to save this project";

                if (fbd.ShowDialog(this) == DialogResult.OK)
                    destinationBox.Text = fbd.SelectedPath;
            }
        }

        private void RetrieveRemoteRefs()
        {
            if (String.Equals(_retrievedRemotesFor, urlBox.Text, StringComparison.OrdinalIgnoreCase))
                return;

            branchBox.BeginUpdate();
            tagBox.BeginUpdate();

            try
            {
                branchBox.Items.Clear();
                tagBox.Items.Clear();

                if (!String.IsNullOrEmpty(urlBox.Text))
                {
                    _retrievedRemotesFor = urlBox.Text;

                    using (var client = GetService<IGitClientPool>().GetNoUIClient())
                    {
                        GitRemoteRefsArgs args = new GitRemoteRefsArgs();
                        args.ThrowOnError = false;
                        args.ThrowOnCancel = false;

                        GitRemoteRefsResult result;

                        if (GetRemoteRefs(urlBox.Text, GitRemoteRefType.All, args, out result))
                        {
                            foreach (var @ref in result.Items)
                            {
                                switch (@ref.Type)
                                {
                                    case GitRefType.Branch:
                                    case GitRefType.RemoteBranch:
                                        branchBox.Items.Add(@ref);

                                        if (String.Equals(@ref.ShortName, "master", StringComparison.OrdinalIgnoreCase))
                                            branchBox.SelectedIndex = branchBox.Items.Count - 1;
                                        break;

                                    case GitRefType.Tag:
                                        tagBox.Items.Add(@ref);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                branchBox.EndUpdate();
                tagBox.EndUpdate();
            }
        }

        private bool GetRemoteRefs(string remote, GitRemoteRefType types, GitRemoteRefsArgs args, out GitRemoteRefsResult result)
        {
            args.AddExpectedError(GitErrorCode.GetRemoteRefsFailed);

            ProgressRunnerArgs pa = new ProgressRunnerArgs();
            pa.CreateLog = true;
            pa.TransportClientArgs = args;

            GitRemoteRefsResult resultTmp = null;

            bool ok = false;

            GetService<IProgressRunner>().RunModal(CommandStrings.ListingRemoteBranches, pa,
                delegate(object sender, ProgressWorkerArgs a)
                {
                    ok = a.Client.GetRemoteRefs(remote, types, args, out resultTmp);
                });

            result = resultTmp;
            return ok;
        }

        public string Remote
        {
            get { return urlBox.Text; }
        }

        public GitRef RemoteRef
        {
            get
            {
                return
                    (branchBox.SelectedItem as GitRef) ??
                    (tagBox.SelectedItem as GitRef);
            }
        }

        public string Destination
        {
            get { return destinationBox.Text; }
        }

        private void urlBox_Leave(object sender, EventArgs e)
        {
            if (!String.Equals(_retrievedRemotesFor, urlBox.Text, StringComparison.OrdinalIgnoreCase))
                branchBox.Items.Clear();

            UpdateEnabled();
        }

        private void CloneDialog_Validating(object sender, CancelEventArgs e)
        {
        }

        private void branchRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void tagRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            branchBox.Enabled = branchRadioBox.Checked && !String.IsNullOrEmpty(urlBox.Text);
            tagBox.Enabled = tagRadioBox.Checked && !String.IsNullOrEmpty(urlBox.Text);

            RetrieveRemoteRefs();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            bool ok = true;

            if (String.IsNullOrEmpty(urlBox.Text))
            {
                errorProvider1.SetError(urlBox, CommandStrings.SelectAnUrl);
                ok = false;
            }
            else
                errorProvider1.SetError(urlBox, null);

            if (branchRadioBox.Checked && (branchBox.SelectedItem as GitRef) == null)
            {
                errorProvider1.SetError(branchBox, CommandStrings.SelectABranch);
                ok = false;
            }
            else
                errorProvider1.SetError(branchBox, null);

            if (tagRadioBox.Checked && (tagBox.SelectedItem as GitRef) == null)
            {
                errorProvider1.SetError(tagBox, CommandStrings.SelectATag);
                ok = false;
            }
            else
                errorProvider1.SetError(tagBox, null);

            if (String.IsNullOrEmpty(destinationBox.Text))
            {
                errorProvider1.SetError(destinationBox, CommandStrings.SelectADestination);
                ok = false;
            }
            else
                errorProvider1.SetError(destinationBox, null);

            if (ok)
            {
            if (!Directory.Exists(destinationBox.Text))
            {
                VisualGitMessageBox mb = new VisualGitMessageBox(Context);

                var result = mb.Show(CommandStrings.DestinationDoesNotExistCreate, CommandStrings.Clone, MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes)
                    CreateRecursive(Path.GetFullPath(destinationBox.Text));
                else
                {
                    errorProvider1.SetError(destinationBox, CommandStrings.DestinationDoesNotExistCreate);
                    return;
                }
            }

            Config.GetRecentReposUrls().Add(urlBox.Text);

            DialogResult = DialogResult.OK;
                }
        }

        private void CreateRecursive(string path)
        {
            string parentPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(parentPath))
                CreateRecursive(parentPath);

            Directory.CreateDirectory(path);
        }
    }
}
