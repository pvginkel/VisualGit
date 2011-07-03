// VisualGit.UI\Commands\ResetBranchDialog.cs
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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VisualGit.UI.Commands
{
    public partial class ResetBranchDialog : VSDialogForm
    {
        public ResetBranchDialog()
        {
            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        public string RepositoryPath { get; set; }

        private void ResetBranchDialog_Load(object sender, EventArgs e)
        {
            Debug.Assert(Revision != null && RepositoryPath != null);

            revisionBox.Text = Revision.Revision;

            using (var client = GetService<IGitClientPool>().GetNoUIClient())
            {
                branchBox.Text = client.GetCurrentBranch(RepositoryPath).ShortName;

                GitLogArgs args = new GitLogArgs();

                args.Start = Revision;
                args.Limit = 1;
                args.Log += (s, ea) =>
                    {
                        authorBox.Text = ea.Author;
                        commitDateBox.Text = ea.AuthorTime.ToString("g");
                        logMessageBox.Text = Regex.Replace(ea.LogMessage.TrimEnd(), "\\r?\\n", " \u00B6 ");
                    };

                client.Log(RepositoryPath, args);
            }
        }

        public GitResetType ResetType
        {
            get
            {
                if (softRadioBox.Checked)
                    return GitResetType.Soft;
                else if (mixedRadioBox.Checked)
                    return GitResetType.Mixed;
                else
                    return GitResetType.Hard;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (ResetType == GitResetType.Hard)
            {
                VisualGitMessageBox mb = new VisualGitMessageBox(Context);

                var result = mb.Show(CommandStrings.YouAreAboutToDiscardAllChanges, CommandStrings.ResetCurrentBranch,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
