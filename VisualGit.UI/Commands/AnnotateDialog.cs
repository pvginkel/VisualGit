// VisualGit.UI\Commands\AnnotateDialog.cs
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
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using SharpGit;
using System.Diagnostics;

namespace VisualGit.UI.Commands
{
    public partial class AnnotateDialog : VSDialogForm
    {
        public AnnotateDialog()
        {
            InitializeComponent();
        }

        private void AnnotateDialog_Load(object sender, EventArgs e)
        {
            whitespaceBox.Items.Add(CommandStrings.WhitespaceCompare);
            whitespaceBox.Items.Add(CommandStrings.WhitespaceIgnoreChanges);
            whitespaceBox.Items.Add(CommandStrings.WhitespaceIgnoreAllWhitespace);
            whitespaceBox.SelectedIndex = 1;
        }

        public void SetTargets(IEnumerable<GitItem> targets)
        {
            List<GitOrigin> origins = new List<GitOrigin>();

            foreach (GitItem i in targets)
                origins.Add(new GitOrigin(i));

            SetTargets(origins);
        }

        public void SetTargets(List<GitOrigin> origins)
        {
            foreach (GitOrigin i in origins)
                targetBox.Items.Add(i);

            Debug.Assert(targetBox.Items.Count > 0);

            targetBox.SelectedIndex = 0;
        }

        public GitOrigin SelectedTarget
        {
            get { return targetBox.SelectedItem as GitOrigin; }
        }

        public GitRevision EndRevision
        {
            get { return toRevision.Revision; }
            set { toRevision.Revision = value; }
        }

        private void targetBox_SelectedValueChanged(object sender, EventArgs e)
        {
            toRevision.GitOrigin = SelectedTarget;
        }

        public GitIgnoreSpacing IgnoreSpacing
        {
            get
            {
                switch (whitespaceBox.SelectedIndex)
                {
                    default:
                    case 0:
                        return GitIgnoreSpacing.None;
                    case 1:
                        return GitIgnoreSpacing.IgnoreSpace;
                    case 2:
                        return GitIgnoreSpacing.IgnoreAll;
                }
            }
        }
    }
}
