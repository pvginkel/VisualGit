// VisualGit.UI\OptionsPages\GitCertificateEditor.cs
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
using VisualGit.UI.VSSelectionControls;
using SharpGit;

namespace VisualGit.UI.OptionsPages
{
    public partial class GitCertificateEditor : VSDialogForm
    {
        public GitCertificateEditor()
        {
            InitializeComponent();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            ResizeList();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                ResizeList();
                Refreshlist();
            }
        }

        private void Refreshlist()
        {
            credentialList.Items.Clear();

            ICollection<GitCertificate> items = ConfigurationService.GetAllCertificates();

            foreach (GitCertificate item in items)
            {
                CertificateListItem lvi = new CertificateListItem(credentialList);
                lvi.Certificate = item;
                lvi.Refresh();
                credentialList.Items.Add(lvi);
            }
        }

        void ResizeList()
        {
            if (!DesignMode && credentialList != null)
                credentialList.ResizeColumnsToFit(locationHeader);
        }

        class CertificateListItem : SmartListViewItem
        {
            GitCertificate _item;

            public CertificateListItem(SmartListView listview)
                : base(listview)
            {
            }

            public GitCertificate Certificate
            {
                get { return _item; }
                set { _item = value; }
            }

            public void Refresh()
            {
                SetValues(
                    _item.Path
                );
            }
        }

        private void credentialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (CertificateListItem li in credentialList.SelectedItems)
            {
                if (li != null)
                {
                    removeButton.Enabled = true;
                    return;
                }
            }

            removeButton.Enabled = false;
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            VisualGitMessageBox mb = new VisualGitMessageBox(Context);

            if (DialogResult.OK != mb.Show(OptionsResources.TheSelectedCertificateWillBeRemoved, "", MessageBoxButtons.OKCancel))
                return;

            bool changed = false;
            try
            {
                foreach (CertificateListItem li in credentialList.SelectedItems)
                {
                    ConfigurationService.RemoveCertificate(
                        li.Certificate.Path
                    );

                    changed = true;
                }
            }
            finally
            {
                if (changed)
                    Refreshlist();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ConfigurationService.StoreCertificate(
                    new GitCertificate(openFileDialog1.FileName)
                );

                Refreshlist();
            }
        }
    }
}
