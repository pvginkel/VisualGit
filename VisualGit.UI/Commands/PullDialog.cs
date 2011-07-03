// VisualGit.UI\Commands\PullDialog.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
    public partial class PullDialog : PushPullDialog
    {
        GitMergeStrategy _currentStrategy = GitMergeStrategy.Unset;
        string _remoteOfBranches;
        ICollection<GitRef> _allRefs;

        public PullDialog()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        public GitPullArgs Args { get; set; }

        private void PullDialog_Load(object sender, EventArgs e)
        {
            using (var client = Context.GetService<IGitClientPool>().GetNoUIClient())
            {
                localBranchBox.Text = client.GetCurrentBranch(RepositoryPath).ShortName;

                LoadRemotes(client, remoteBox);
                LoadRemoteUris(client, urlBox);

                _allRefs = client.GetRefs(RepositoryPath);
            }
        }

        private void remoteRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void urlRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void mergeRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void rebaseRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void fetchRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            GitMergeStrategy mergeStrategy;

            if (mergeRadioBox.Checked)
                mergeStrategy = GitMergeStrategy.Merge;
            else if (rebaseRadioBox.Checked)
                mergeStrategy = GitMergeStrategy.Rebase;
            else
                mergeStrategy = GitMergeStrategy.Unset;

            if (mergeStrategy != _currentStrategy)
            {
                _currentStrategy = mergeStrategy;

                switch (_currentStrategy)
                {
                    case GitMergeStrategy.Merge:
                        strategyBox.Image = Properties.Resources.MergeGraph;
                        break;

                    case GitMergeStrategy.Rebase:
                        strategyBox.Image = Properties.Resources.RebaseGraph;
                        break;

                    case GitMergeStrategy.Unset:
                        strategyBox.Image = Properties.Resources.FetchGraph;
                        break;
                }
            }

            remoteBox.Enabled = remoteRadioBox.Checked;
            urlBox.Enabled = urlRadioBox.Checked;

            string currentRemote = GetCurrentRemote();

            if (currentRemote != _remoteOfBranches)
            {
                remoteBranchBox.Text = "";
                remoteBranchBox.Items.Clear();

                _remoteOfBranches = currentRemote;
            }
        }

        private string GetCurrentRemote()
        {
            string currentRemote = remoteRadioBox.Checked
                ? remoteBox.Text.Trim()
                : urlBox.Text.Trim();

            return String.Empty.Equals(currentRemote) ? null : currentRemote;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            bool ok = true;

            if (urlRadioBox.Checked && String.Empty.Equals(urlBox.Text.Trim()))
            {
                errorProvider1.SetError(urlBox, CommandStrings.SelectAnUrl);
                ok = false;
            }
            else
                errorProvider1.SetError(urlBox, null);

            if (ok)
            {
                Args.MergeStrategy = _currentStrategy;

                if (remoteRadioBox.Checked && String.Empty.Equals(remoteBox.Text.Trim()))
                    Args.Remote = remoteBox.Text.Trim();
                if (urlRadioBox.Checked)
                    Args.RemoteUri = urlBox.Text.Trim();

                if (!String.Empty.Equals(remoteBranchBox.Text.Trim()))
                    Args.RemoteBranch = GitRef.Create(remoteBranchBox.Text.Trim(), GitRefType.Branch);

                DialogResult = DialogResult.OK;
            }
        }

        private void remoteBranchBox_Enter(object sender, EventArgs e)
        {
            string currentRemote = GetCurrentRemote();

            if (currentRemote != null && _remoteOfBranches != currentRemote)
            {
                _remoteOfBranches = currentRemote;

                remoteBranchBox.BeginUpdate();
                remoteBranchBox.Items.Clear();

                currentRemote += '/';

                foreach (var @rev in _allRefs)
                {
                    if (
                        @rev.Type == GitRefType.RemoteBranch &&
                        @rev.ShortName.StartsWith(currentRemote)
                    )
                        remoteBranchBox.Items.Add(@rev.ShortName.Substring(currentRemote.Length));
                }

                remoteBranchBox.EndUpdate();
            }
        }
    }
}
