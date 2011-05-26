using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PathSelector
{
    public partial class PendingChangeSelector : VSContainerForm
    {
        public PendingChangeSelector()
        {
            InitializeComponent();
        }

        IEnumerable<PendingChange> _changeEnumerator;

        public void LoadChanges(IEnumerable<VisualGit.Scc.PendingChange> changeWalker)
        {
            if (changeWalker == null)
                throw new ArgumentNullException("changeWalker");

            _changeEnumerator = changeWalker;

            if (IsHandleCreated)
                Reload();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (pendingList.Context == null && Context != null)
            {
                pendingList.Context = Context;
                pendingList.SelectionPublishServiceProvider = Context;
            }

            pendingList.ColumnWidthChanged += new ColumnWidthChangedEventHandler(pendingList_ColumnWidthChanged);
            IDictionary<string, int> widths = ConfigurationService.GetColumnWidths(GetType());
            pendingList.SetColumnWidths(widths);

            Reload();
        }

        protected void pendingList_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            IDictionary<string, int> widths = pendingList.GetColumnWidths();
            ConfigurationService.SaveColumnsWidths(GetType(), widths);
        }

        IEnumerable<GitItem> _allItems;
        Predicate<GitItem> _filter;
        Predicate<GitItem> _checkedFilter;

        private void Reload()
        {
            PendingChange.RefreshContext rc = new PendingChange.RefreshContext(Context);
            Dictionary<string, bool> checkedCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (PendingCommitItem pci in pendingList.Items)
            {
                checkedCache[pci.FullPath] = pci.Checked;
            }

            pendingList.ClearItems();
            foreach (GitItem i in _allItems)
            {
                if (_filter != null && !_filter(i))
                    continue;

                PendingChange pc = new PendingChange(rc, i);
                PendingCommitItem pci = new PendingCommitItem(pendingList, pc);

                bool chk;
                if (checkedCache.TryGetValue(i.FullPath, out chk))
                    pci.Checked = chk;
                else if (_checkedFilter != null && !_checkedFilter(i))
                    pci.Checked = false;

                pendingList.Items.Add(pci);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>
        /// true if the keystroke was processed and consumed by the control; otherwise, false to allow further processing.
        /// </returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Return | Keys.Control)
                && okButton.Enabled)
            {
                DialogResult = DialogResult.OK;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public IEnumerable<PendingChange> GetSelection()
        {
            foreach (PendingCommitItem it in pendingList.Items)
            {
                if (it.Checked)
                    yield return it.PendingChange;
            }
        }

        public IEnumerable<GitItem> GetSelectedItems()
        {
            foreach (PendingChange pc in GetSelection())
            {
                yield return pc.GitItem;
            }
        }

        public void LoadItems(IEnumerable<GitItem> allItems, Predicate<GitItem> visibleFilter, Predicate<GitItem> checkedFilter)
        {
            _allItems = allItems;
            _checkedFilter = checkedFilter;
            _filter = visibleFilter;
        }

        public void LoadItems(IEnumerable<GitItem> allItems)
        {
            LoadItems(allItems, PendingChange.IsPending, null);
        }

        private void pendingList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            okButton.Enabled = sender is ListView
                && ((ListView)sender).CheckedItems.Count > 0;
        }
    }
}
