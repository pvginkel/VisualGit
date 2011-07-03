// VisualGit.UI\Commands\PushBranchControl.cs
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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;
using VisualGit.VS;

namespace VisualGit.UI.Commands
{
    public partial class PushBranchControl : UserControl, IPushControl
    {
        public PushBranchControl()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            remoteBox.Enabled = !pushAllBox.Checked;
            localBox.Enabled = !pushAllBox.Checked;
        }

        public string RepositoryPath { get; set; }

        public IVisualGitServiceProvider Context { get; set; }

        public GitPushArgs Args { get; set; }

        public void LoadFromClient(GitClient client)
        {
            localBox.BeginUpdate();
            remoteBox.BeginUpdate();

            localBox.Items.Clear();
            remoteBox.Items.Clear();

            var currentBranch = client.GetCurrentBranch(RepositoryPath);

            foreach (var @ref in client.GetRefs(RepositoryPath))
            {
                if (@ref.Type == GitRefType.Branch)
                {
                    remoteBox.Items.Add(@ref.ShortName);
                    localBox.Items.Add(@ref.ShortName);

                    if (@ref == currentBranch)
                    {
                        remoteBox.SelectedIndex = remoteBox.Items.Count - 1;
                        localBox.SelectedIndex = remoteBox.Items.Count - 1;
                    }
                }
            }

            localBox.EndUpdate();
            remoteBox.EndUpdate();
        }

        private void pushAllBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        ErrorProvider _errorProvider;
        private ErrorProvider ErrorProvider
        {
            get { return _errorProvider ?? (_errorProvider = ((IHasErrorProvider)FindForm()).ErrorProvider); }
        }

        public bool FlushArgs(string remote)
        {
            bool ok = true;

            if (!pushAllBox.Checked && String.Empty.Equals(localBox.Text.Trim()))
            {
                ErrorProvider.SetError(localBox, CommandStrings.SelectALocalBranch);
                ok = false;
            }
            else
                ErrorProvider.SetError(localBox, null);

            if (!pushAllBox.Checked && String.Empty.Equals(remoteBox.Text.Trim()))
            {
                ErrorProvider.SetError(remoteBox, CommandStrings.SelectARemoteBranch);
                ok = false;
            }
            else
                ErrorProvider.SetError(remoteBox, null);

            if (ok && remote != null && !pushAllBox.Checked)
            {
                string remoteBranch = "refs/remotes/" + remote + "/" + remoteBox.Text.Trim();

                using (var client = Context.GetService<IGitClientPool>().GetNoUIClient())
                {
                    bool found = false;

                    foreach (var @ref in client.GetRefs(RepositoryPath))
                    {
                        if (String.Equals(@ref.Name, remoteBranch, StringComparison.Ordinal))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        var result = Context.GetService<IVisualGitDialogOwner>()
                            .MessageBox.Show(CommandStrings.NewRemoteBranch,
                            CommandStrings.PushingSolution, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        ok = result == DialogResult.Yes;
                    }
                }
            }

            if (ok)
            {
                Args.AllBranches = pushAllBox.Checked;
                Args.Force = forceBox.Checked;
                Args.LocalBranch = new GitRef(localBox.Text.Trim() + ":" + remoteBox.Text.Trim());
            }

            return ok;
        }

        private void localBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            remoteBox.Text = localBox.Text;
        }
    }
}
