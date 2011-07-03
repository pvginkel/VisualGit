// VisualGit.UI\SccManagement\CreateTagDialog.cs
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

namespace VisualGit.UI.SccManagement
{
    public partial class CreateTagDialog : VSContainerForm
    {
        public CreateTagDialog()
        {
            InitializeComponent();

            UpdateEnabled();
        }

        public bool AnnotatedTag
        {
            get { return annotatedTagBox.Checked; }
        }

        public string LogMessage
        {
            get { return logMessage.Text; }
        }

        public string TagName
        {
            get
            {
                string tagName = tagBox.Text.Trim();

                if (String.Empty.Equals(tagName))
                    return null;
                else
                    return tagName;
            }
        }

        private void toUrlBox_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = TagName != null;
        }

        private void annotatedTagBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            logMessage.Enabled = annotatedTagBox.Checked;
        }
    }
}
