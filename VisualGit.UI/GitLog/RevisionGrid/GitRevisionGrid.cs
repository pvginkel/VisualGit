using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SharpGit;
using VisualGit.Scc;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    public class GitRevisionGrid : Control, ICurrentItemSource<VisualGit.Scc.IGitLogItem>
    {
        private const int DefaultInitialLimit = 100;

        private RevisionGridControl _control;

        public GitRevisionGrid()
        {
            _control = new RevisionGridControl();

            _control.Dock = DockStyle.Fill;

            Controls.Add(_control);

            _control.SelectionChanged += new EventHandler(_control_SelectionChanged);
            _control.BatchDone += new EventHandler<BatchFinishedEventArgs>(_control_BatchDone);
        }

        void _control_BatchDone(object sender, BatchFinishedEventArgs e)
        {
            OnBatchDone(new BatchFinishedEventArgs(e.TotalCount));
        }

        void _control_SelectionChanged(object sender, EventArgs e)
        {
            OnFocusChanged(new CurrentItemEventArgs<Scc.IGitLogItem>(this));
            OnSelectionChanged(new CurrentItemEventArgs<Scc.IGitLogItem>(this));
        }

        [Browsable(false)]
        public ICollection<string> LastSelectedRows
        {
            get { return _control.LastSelectedRows; }
        }

        [Browsable(false)]
        public string CurrentCheckout
        {
            get { return _control.CurrentCheckout; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GitClient Client
        {
            get { return _control.Client; }
            set { _control.Client = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GitLogArgs Args
        {
            get { return _control.Args; }
            set { _control.Args = value; }
        }

        internal void Start(LogMode logMode)
        {
            if (_control.Client == null)
            {
                if (Context == null)
                    throw new InvalidOperationException("Context is required to be able to start");

                _control.Client = Context.GetService<IGitClientPool>().GetNoUIClient();
            }

            LogMode = logMode;

            PrepareParameters();

            _control.BeginRefresh();
        }

        internal void FetchAll()
        {
            _control.Clear();

            PrepareParameters();

            _control.Args.Limit = 0;

            _control.BeginRefresh();
        }

        private void PrepareParameters()
        {
            _control.RepositoryPath = GitTools.GetAbsolutePath(LogSource.RepositoryRoot);

            _control.Args = new GitLogArgs();

            _control.Args.Start = LogSource.Start;
            _control.Args.End = LogSource.End;

            // If we have EndRevision set, we want all items until End
            if (_control.Args.End == null || _control.Args.End.RevisionType == GitRevisionType.None)
                _control.Args.Limit = DefaultInitialLimit;

            _control.Args.StrictNodeHistory = LogSource.StrictNodeHistory;
            _control.Args.RetrieveMergedRevisions = LogSource.IncludeMergedRevisions;

            GitOrigin single = EnumTools.GetSingle(LogSource.Targets);

            if (single != null)
            {
                // TODO: Use peg information
            }

            _control.Uris = new List<Uri>();

            foreach (GitOrigin o in LogSource.Targets)
            {
                _control.Uris.Add(o.Uri);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_control != null)
                {
                    if (_control.Client != null)
                    {
                        _control.Client.Dispose();
                        _control.Client = null;
                    }

                    _control.Dispose();
                    _control = null;
                }
            }

            base.Dispose(disposing);
        }

        internal void Reset()
        {
            _control.Clear();
        }

        public IVisualGitServiceProvider Context { get; set; }

        internal LogDataSource LogSource { get; set; }

        public event EventHandler<BatchFinishedEventArgs> BatchDone;

        public event EventHandler<CurrentItemEventArgs<Scc.IGitLogItem>> SelectionChanged;

        public event EventHandler<CurrentItemEventArgs<Scc.IGitLogItem>> FocusChanged;

        protected virtual void OnBatchDone(BatchFinishedEventArgs e)
        {
            var ev = BatchDone;

            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnFocusChanged(CurrentItemEventArgs<Scc.IGitLogItem> e)
        {
            var ev = FocusChanged;

            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnSelectionChanged(CurrentItemEventArgs<Scc.IGitLogItem> e)
        {
            var ev = SelectionChanged;

            if (ev != null)
                ev(this, e);
        }

        public Scc.IGitLogItem FocusedItem
        {
            get
            {
                var selectedData = _control.SelectedData;

                if (selectedData == null || selectedData.Length == 0)
                    return null;
                else
                    return (VisualGit.Scc.IGitLogItem)selectedData[0];
            }
        }

        public IList<Scc.IGitLogItem> SelectedItems
        {
            get
            {
                var result = new List<Scc.IGitLogItem>();

                var selectedData = _control.SelectedData;

                if (selectedData != null)
                {
                    foreach (var item in selectedData)
                    {
                        result.Add((Scc.IGitLogItem)item);
                    }
                }

                return result;
            }
        }

        public LogMode LogMode { get; private set; }
    }
}
