// VisualGit.UI\SccManagement\ProjectCommitDialog.cs
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
using System.ComponentModel.Design;
using System.Windows.Forms;
using VisualGit.Configuration;
using Microsoft.VisualStudio;

using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.UI.PendingChanges.Commits;
using VisualGit.VS;
using System.ComponentModel;

namespace VisualGit.UI.SccManagement
{
    public partial class ProjectCommitDialog : VSContainerForm
    {
        public ProjectCommitDialog()
        {
            InitializeComponent();
            logMessage.PasteSource = pendingList;
        }

        VisualGitConfig Config
        {
            get { return ConfigurationService.Instance; }
        }

        IVisualGitCommandService CommandService
        {
            get { return GetService<IVisualGitCommandService>(); }
        }
        IEnumerable<PendingChange> _changeEnumerator;

        public void LoadChanges(IEnumerable<VisualGit.Scc.PendingChange> changeWalker)
        {
            if (changeWalker == null)
                throw new ArgumentNullException("changeWalker");

            if (pendingList.Context == null && Context != null)
            {
                pendingList.Context = Context;
                pendingList.SelectionPublishServiceProvider = Context;
            }

            _changeEnumerator = changeWalker;
            Reload();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            logMessage.Select();

            pendingList.ColumnWidthChanged += new ColumnWidthChangedEventHandler(pendingList_ColumnWidthChanged);
            IDictionary<string, int> widths = ConfigurationService.GetColumnWidths(GetType());
            pendingList.SetColumnWidths(widths);

            VSCommandHandler.Install(Context, logMessage, new CommandID(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.OPENLINEABOVE), new EventHandler<CommandEventArgs>(OnCommit));
        }

        protected void pendingList_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            IDictionary<string, int> widths = pendingList.GetColumnWidths();
            ConfigurationService.SaveColumnsWidths(GetType(), widths);
        }

        private void Reload()
        {
            Dictionary<PendingChange, PendingCommitItem> chk = null;

            if (pendingList.Items.Count > 0)
            {
                chk = new Dictionary<PendingChange, PendingCommitItem>();

                foreach (PendingCommitItem it in pendingList.Items)
                {
                    chk.Add(it.PendingChange, it);
                    it.Group = null;
                }
            }

            pendingList.ClearItems();

            foreach (PendingChange pc in _changeEnumerator)
            {
                PendingCommitItem pi;
                if (chk != null && chk.TryGetValue(pc, out pi))
                {
                    pendingList.Items.Add(pi);
                    pi.RefreshText(Context);
                }
                else
                    pendingList.Items.Add(new PendingCommitItem(pendingList, pc));
            }
        }

        void OnCommit(object sender, CommandEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public IEnumerable<PendingChange> GetSelection()
        {
            foreach (PendingCommitItem it in pendingList.Items)
            {
                if (it.Checked)
                    yield return it.PendingChange;
            }
        }

        public void FillArgs(PendingChangeCommitArgs pca)
        {
            pca.LogMessage = logMessage.Text;
            pca.AmendLastCommit = amendBox.Checked;
        }

        public string LogMessageText
        {
            get { return logMessage.Text; }
            set { logMessage.Text = value ?? ""; }
        }

        public bool AmendLastCommit
        {
            get { return amendBox.Checked; }
            set { amendBox.Checked = value; }
        }

        class ItemLister : VisualGitService, IEnumerable<PendingChange>
        {
            readonly IEnumerable<GitItem> _items;

            IPendingChangesManager _pcm;
            readonly PendingChange.RefreshContext _rc;
            public ItemLister(IVisualGitServiceProvider context, IEnumerable<GitItem> items)
                : base(context)
            {
                if (items == null)
                    throw new ArgumentNullException("items");
                _items = items;
                _rc = new PendingChange.RefreshContext(context);
            }

            #region IEnumerable<PendingChange> Members

            IPendingChangesManager Manager
            {
                get { return _pcm ?? (_pcm = GetService<IPendingChangesManager>()); }
            }

            readonly Dictionary<string, PendingChange> _pcs = new Dictionary<string, PendingChange>(StringComparer.OrdinalIgnoreCase);

            public IEnumerator<PendingChange> GetEnumerator()
            {
                foreach (GitItem item in _items)
                {
                    if (PendingChange.IsPending(item))
                    {
                        PendingChange pc = Manager[item.FullPath];

                        if (pc == null && !_pcs.TryGetValue(item.FullPath, out pc))
                        {
                            PendingChange.CreateIfPending(_rc, item, out pc);
                        }

                        if (pc == null)
                            yield break; // Not a pending change

                        _pcs[item.FullPath] = pc;

                        yield return pc;
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        public void LoadItems(IEnumerable<GitItem> iEnumerable)
        {
            LoadChanges(new ItemLister(Context, iEnumerable));
        }

        private void pendingList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            okButton.Enabled = sender is ListView
                && ((ListView)sender).CheckedItems.Count > 0;
        }

        private void pendingList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = pendingList.HitTest(e.X, e.Y);

            if (info == null || info.Location == ListViewHitTestLocations.None)
                return;

            if (info.Location == ListViewHitTestLocations.StateImage)
                return; // Just check the item

            if (CommandService != null)
                CommandService.ExecCommand(Config.PCDoubleClickShowsChanges
                    ? VisualGitCommand.ItemShowChanges : VisualGitCommand.ItemOpenVisualStudio, true);
        }
    }
}
