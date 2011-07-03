// VisualGit.UI\Commands\PushTagControl.cs
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
