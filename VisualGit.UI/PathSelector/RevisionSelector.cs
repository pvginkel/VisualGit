// VisualGit.UI\PathSelector\RevisionSelector.cs
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
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using System.Globalization;
using VisualGit.UI.GitLog;

namespace VisualGit.UI.PathSelector
{
    partial class RevisionSelector : UserControl
    {
        public RevisionSelector()
        {
            InitializeComponent();
        }

        IVisualGitServiceProvider _context;
        GitOrigin _origin;

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set { _context = value; EnableBrowse(); }
        }

        /// <summary>
        /// Gets or sets the SVN origin.
        /// </summary>
        /// <value>The SVN origin.</value>
        public GitOrigin GitOrigin
        {
            get { return _origin; }
            set { _origin = value; EnableBrowse(); }
        }

        void EnableBrowse()
        {
            browseButton.Enabled = (GitOrigin != null) && (Context != null);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (LogViewerDialog lvd = new LogViewerDialog(GitOrigin))
            {
                if (DialogResult.OK != lvd.ShowDialog(Context))
                    return;

                IGitLogItem li = EnumTools.GetSingle(lvd.SelectedItems);

                if (li == null)
                    return;

                Revision = li.Revision;
            }
        }

        public event EventHandler Changed;

        public string Revision
        {
            get
            {
                string text = revisionBox.Text;

                if (String.IsNullOrEmpty(text))
                    return null;
                else
                    return text.Trim();
            }
            set
            {
                revisionBox.Text = value;
            }
        }

        private void revisionBox_TextChanged(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        private void revisionBox_SizeChanged(object sender, EventArgs e)
        {
            browseButton.Height = revisionBox.Height + revisionBox.Margin.Vertical;
        }
    }
}
