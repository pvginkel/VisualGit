// VisualGit.UI\Commands\PushPullDialog.cs
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
