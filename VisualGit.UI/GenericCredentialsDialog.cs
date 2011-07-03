// VisualGit.UI\GenericCredentialsDialog.cs
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
    public partial class GenericCredentialsDialog : VSDialogForm
    {
        private GitCredentialItem _item;

        public GenericCredentialsDialog()
        {
            InitializeComponent();
        }

        public GitCredentialItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public bool RememberPassword
        {
            get { return rememberBox.Checked; }
            set { rememberBox.Checked = value; }
        }

        private void GenericCredentialsDialog_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_item.PromptText))
                promptLabel.Text = _item.PromptText;

            credentialBox.Text = _item.Value;

            if (Item.Type != GitCredentialsType.Username)
                credentialBox.UseSystemPasswordChar = true;
            else
                rememberBox.Visible = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _item.Value = credentialBox.Text;

            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
