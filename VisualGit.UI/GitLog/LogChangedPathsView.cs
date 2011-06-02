using System;
using System.Windows.Forms;
using VisualGit.UI.VSSelectionControls;
using SharpSvn;
using VisualGit.Scc;
using System.ComponentModel;
using System.Drawing;
using SharpGit;

namespace VisualGit.UI.GitLog
{
    class LogChangedPathsView : ListViewWithSelection<PathListViewItem>
    {
        public LogChangedPathsView()
        {
            Init();
        }

        public LogChangedPathsView(IContainer container)
            : this()
        {

            container.Add(this);
        }

        LogDataSource _dataSource;
        public LogDataSource DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        void Init()
        {
            SmartColumn action = new SmartColumn(this, "&Action", 60);
            SmartColumn path = new SmartColumn(this, "&Path", 342);
            SmartColumn copy = new SmartColumn(this, "&Copy", 60);
            SmartColumn copyRev = new SmartColumn(this, "Copy &Revision", 60);

            AllColumns.Add(action);
            AllColumns.Add(path);
            AllColumns.Add(copy);
            AllColumns.Add(copyRev);

            Columns.AddRange(
                new ColumnHeader[]
                {
                    action,
                    path,
                    copy,
                    copyRev
                });

            SortColumns.Add(path);
            FinalSortColumn = path;
        }

        protected override void OnRetrieveSelection(RetrieveSelectionEventArgs e)
        {
            e.SelectionItem = new PathItem(e.Item);
            base.OnRetrieveSelection(e);
        }

        protected override void OnResolveItem(ResolveItemEventArgs e)
        {
            e.Item = ((PathItem)e.SelectionItem).ListViewItem;
            base.OnResolveItem(e);
        }
    }

    sealed class PathListViewItem : SmartListViewItem
    {
        readonly IGitLogItem _logItem;
        readonly SvnChangeItem _change;
        readonly bool _isInSelection;
        readonly GitOrigin _origin;

        public PathListViewItem(LogChangedPathsView view, IGitLogItem logItem, SvnChangeItem change, Uri reposRoot, bool isInSelection)
            : base(view)
        {
            if (logItem == null)
                throw new ArgumentNullException("logItem");
            if (change == null)
                throw new ArgumentNullException("change");
            _logItem = logItem;
            _change = change;
            _isInSelection = isInSelection;
            Uri uri;

            string path = change.Path.TrimStart('/');

            if (string.IsNullOrEmpty(path))
                uri = reposRoot;
            else
                uri = SvnTools.AppendPathSuffix(reposRoot, path);

            _origin = new GitOrigin(new SvnUriTarget(uri, logItem.Revision), reposRoot);

            RefreshText();
            UpdateColors();
        }

        public GitOrigin Origin
        {
            get { return _origin; }
        }

        void RefreshText()
        {
            SetValues(
                _change.Action.ToString(),
                NodeKind == SvnNodeKind.Directory ? EnsureEndSlash(_change.Path) : _change.Path,
                _change.CopyFromPath ?? "",
                _change.CopyFromPath != null ? _change.CopyFromRevision.ToString() : ""
            );
        }

        private string EnsureEndSlash(string p)
        {
            if (!p.EndsWith("/", StringComparison.Ordinal))
                return p + "/";

            return p;
        }

        void UpdateColors()
        {
            if (SystemInformation.HighContrast)
                return;
            if (!_isInSelection)
                ForeColor = Color.Gray;
            else
            {
                switch (_change.Action)
                {
                    case SvnChangeAction.Add:
                        ForeColor = Color.FromArgb(100, 0, 100);
                        break;
                    case SvnChangeAction.Delete:
                        ForeColor = Color.DarkRed;
                        break;
                    case SvnChangeAction.Modify:
                        ForeColor = Color.DarkBlue;
                        break;
                }
            }
        }

        internal SvnChangeAction Action
        {
            get { return _change.Action; }
        }

        internal string Path
        {
            get { return _change.Path; }
        }

        internal string CopyFromPath
        {
            get { return _change.CopyFromPath; }
        }

        internal long CopyFromRevision
        {
            get { return _change.CopyFromRevision; }
        }

        internal IGitLogItem LogItem
        {
            get { return _logItem; }
        }

        internal SvnNodeKind NodeKind
        {
            get { return _change.NodeKind; }
        }
    }

    sealed class PathItem : VisualGitPropertyGridItem, IGitLogChangedPathItem
    {
        readonly PathListViewItem _lvi;
        public PathItem(PathListViewItem lvi)
        {
            if (lvi == null)
                throw new ArgumentNullException("lvi");
            _lvi = lvi;
        }

        [Browsable(false)]
        public GitOrigin Origin
        {
            get { return _lvi.Origin; }
        }

        internal PathListViewItem ListViewItem
        {
            get { return _lvi; }
        }

        [Category("Git")]
        [DisplayName("Action")]
        public SvnChangeAction Action
        {
            get { return _lvi.Action; }
        }

        [Category("Origin")]
        [DisplayName("Copied from path")]
        public string CopyFromPath
        {
            get { return _lvi.CopyFromPath; }
        }

        [Category("Origin")]
        [DisplayName("Copied from revision")]
        public long CopyFromRevision
        {
            get { return _lvi.CopyFromRevision; }
        }

        [DisplayName("Name")]
        public string Name
        {
            get { return _lvi.Origin.Target.FileName; }
        }

        [DisplayName("Path")]
        public string Path
        {
            get { return _lvi.Path; }
        }

        [Category("Git")]
        [DisplayName("Url")]
        public Uri Uri
        {
            get { return _lvi.Origin.Uri; }
        }

        [Category("Git")]
        [DisplayName("Last Revision")]
        [Description("Revision number of the Last Commit")]
        public long Revision
        {
            get { return _lvi.LogItem.Revision; }
        }

        [Category("Git")]
        [DisplayName("Last Author")]
        [Description("Author of the Last Commit")]
        public string Author
        {
            get { return _lvi.LogItem.Author; }
        }

        [Category("Git")]
        [DisplayName("Last Committed")]
        [Description("Time of the Last Commit")]
        public DateTime LastCommitted
        {
            get { return _lvi.LogItem.CommitDate.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the light/second name shown in the gridview header
        /// </summary>
        /// <value></value>
        protected override string ClassName
        {
            get { return "Changed Path"; }
        }

        /// <summary>
        /// Gets the bold/first name shown in the gridview header
        /// </summary>
        /// <value></value>
        protected override string ComponentName
        {
            get { return Origin.Target.FileName; }
        }

        SvnRevision IGitRepositoryItem.Revision
        {
            get { return Revision; }
        }

        SvnNodeKind IGitRepositoryItem.NodeKind
        {
            get { return _lvi.NodeKind; }
        }

        GitOrigin IGitRepositoryItem.Origin
        {
            // We don't have a repository item when we are deleted!
            get { return (Action != SvnChangeAction.Delete) ? Origin : null; }
        }

        void IGitRepositoryItem.RefreshItem(bool refreshParent)
        {
        }
    }
}
