// VisualGit.UI\UsernamePasswordCredentialsDialog.cs
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

namespace VisualGit.UI
{
    public partial class UsernamePasswordCredentialsDialog : VSDialogForm
    {
        GitCredentialItem _usernameItem;
        GitCredentialItem _passwordItem;

        public UsernamePasswordCredentialsDialog()
        {
            InitializeComponent();
        }

        public GitCredentialItem UsernameItem
        {
            get { return _usernameItem; }
            set { _usernameItem = value; }
        }

        public GitCredentialItem PasswordItem
        {
            get { return _passwordItem; }
            set { _passwordItem = value; }
        }

        public bool RememberPassword
        {
            get { return rememberBox.Checked; }
            set { rememberBox.Checked = value; }
        }

        private void UsernamePasswordCredentialsDialog_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_usernameItem.PromptText))
                promptLabel.Text = _usernameItem.PromptText;
            else if (!String.IsNullOrEmpty(_passwordItem.PromptText))
                promptLabel.Text = _passwordItem.PromptText;

            usernameBox.Text = _usernameItem.Value;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _usernameItem.Value = usernameBox.Text;
            _passwordItem.Value = passwordBox.Text;

            DialogResult = DialogResult.OK;
        }
    }
}
