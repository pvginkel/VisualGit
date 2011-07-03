// VisualGit.UI\SccManagement\CreateBranchDialog.cs
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
using VisualGit.UI.GitLog;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.SccManagement
{
    public partial class CreateBranchDialog : VSContainerForm
    {
        public CreateBranchDialog()
        {
            InitializeComponent();

            versionBox.Revision = GitRevision.Head;
        }

        private GitOrigin _gitOrigin;
        public GitOrigin GitOrigin
        {
            get { return _gitOrigin; }
            set
            {
                _gitOrigin = value;

                versionBox.GitOrigin = _gitOrigin;
            }
        }

        protected override void OnContextChanged(EventArgs e)
        {
            base.OnContextChanged(e);

            versionBox.Context = Context;
        }

        public bool SwitchToBranch
        {
            get { return switchBox.Checked; }
            set { switchBox.Checked = value; }
        }

        public string BranchName
        {
            get
            {
                string branchName = branchBox.Text.Trim();

                if (String.Empty.Equals(branchName))
                    return null;
                else
                    return branchName;
            }
        }

        public bool Force
        {
            get { return forceBox.Checked; }
        }

        public GitRevision Revision
        {
            get { return versionBox.Revision; }
        }

        private void toUrlBox_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = BranchName != null;
        }
    }
}
