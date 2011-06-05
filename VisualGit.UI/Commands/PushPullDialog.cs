using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public class PushPullDialog : VSDialogForm
    {
        protected void LoadRemoteUris(GitClient client, ComboBox urlBox)
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

        public string RepositoryPath { get; set; }

        IVisualGitConfigurationService _config;
        protected IVisualGitConfigurationService Config
        {
            get { return _config ?? (_config = GetService<IVisualGitConfigurationService>()); }
        }

        protected void LoadRemotes(GitClient client, ComboBox remoteBox)
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
    }
}
