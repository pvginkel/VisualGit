// VisualGit.UI\SccManagement\MultiWorkingCopyCommit.cs
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
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.UI.VSSelectionControls;

namespace VisualGit.UI.SccManagement
{
    public partial class MultiWorkingCopyCommit : VSDialogForm
    {
        public MultiWorkingCopyCommit()
        {
            InitializeComponent();
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            if (!DesignMode)
                ResizeList();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
                ResizeList();
        }

        void ResizeList()
        {
            wcList.ResizeColumnsToFit(columnWorkingCopy, columnRepository);
        }

        public void SetInfo(List<GitWorkingCopy> wcs, List<List<PendingChange>> pcs)
        {
            wcList.Items.Clear();

            for (int i = 0; i < wcs.Count; i++)
            {
                SmartListViewItem lvi = new SmartListViewItem(wcList);
                GitWorkingCopy wc = wcs[i];
                string wcRoot = wc.RepositoryRoot;
                lvi.SetValues(
                    wc.FullPath,
                    pcs[i].Count.ToString(),
                    wcRoot != null ? wcRoot : "");
                lvi.Checked = (i == 0);
                lvi.Tag = new List<PendingChange>(pcs[i]);
                wcList.Items.Add(lvi);
            }
        }

        public ICollection<List<PendingChange>> ChangeGroups
        {
            get
            {
                List<List<PendingChange>> result = new List<List<PendingChange>>();
                foreach (ListViewItem i in wcList.Items)
                {
                    if (i.Checked)
                        result.Add((List<PendingChange>)i.Tag);
                }
                return result;
            }
        }
    }
}
