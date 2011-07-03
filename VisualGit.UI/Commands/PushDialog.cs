// VisualGit.UI\Commands\PushDialog.cs
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
    public partial class PushDialog : PushPullDialog, IHasErrorProvider
    {
        IPushControl _childControl;

        public PushDialog()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        public PushType Type { get; set; }

        public GitPushArgs Args { get; set; }

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
                LoadRemotes(client, remoteBox);
                LoadRemoteUris(client, urlBox);
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
