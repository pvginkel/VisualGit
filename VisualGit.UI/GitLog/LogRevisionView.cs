using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using VisualGit.UI.VSSelectionControls;
using System.Drawing;

namespace VisualGit.UI.GitLog
{
    class LogRevisionView : ListViewWithSelection<LogRevisionItem>
    {
        readonly List<LogRevisionItem> _items = new List<LogRevisionItem>();
        public LogRevisionView()
        {
            Sorting = SortOrder.None;
            Init();
        }
        public LogRevisionView(IContainer container)
            : this()
        {
            container.Add(this);
        }

        LogDataSource _dataSource;
        public LogDataSource LogSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        SmartColumn _expand;
        SmartColumn _revisionColumn;
        SmartColumn _messageColumn;
        void Init()
        {
            _expand = new SmartColumn(this, "\x02D9", "&Expand Merges", 12, HorizontalAlignment.Left);
            _revisionColumn = new SmartColumn(this, "&Revision", 64, HorizontalAlignment.Right);
            SmartColumn author = new SmartColumn(this, "&Author", 73);
            SmartColumn date = new SmartColumn(this, "&Date", 118);
            SmartColumn issue = new SmartColumn(this, "&Issue", 60);
            _messageColumn = new SmartColumn(this, "&Message", 300);

            _expand.Hideable = false;
            _expand.Sortable = _revisionColumn.Sortable = author.Sortable = date.Sortable = issue.Sortable = _messageColumn.Sortable = false;

            AllColumns.Add(_expand);
            AllColumns.Add(_revisionColumn);
            AllColumns.Add(author);

            AllColumns.Add(date);
            AllColumns.Add(issue);
            AllColumns.Add(_messageColumn);

            // The listview can't align the first column right. We switch their display position
            // to work around this
            Columns.AddRange(
                new ColumnHeader[]
                {
                    _expand,
                    _revisionColumn,
                    author,
                    date,
                    _messageColumn
                });
        }

        protected override void OnRetrieveSelection(RetrieveSelectionEventArgs e)
        {
            e.SelectionItem = new LogItem(e.Item, LogSource.RepositoryRoot);
            base.OnRetrieveSelection(e);
        }

        protected override void OnResolveItem(ResolveItemEventArgs e)
        {
            e.Item = ((LogItem)e.SelectionItem).ListViewItem;
            base.OnResolveItem(e);
        }

        public List<LogRevisionItem> VirtualItems
        {
            get { return _items; }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            OnScrolled(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x115) // WM_VSCROLL
                OnScrolled(EventArgs.Empty);
        }

        public event EventHandler Scrolled;
        private void OnScrolled(EventArgs e)
        {
            if (Scrolled != null)
                Scrolled(this, e);
        }   

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (!DesignMode && _messageColumn != null)
                ResizeColumnsToFit(_messageColumn);
        }
    }
}
