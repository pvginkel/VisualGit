using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using SharpSvn;

using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.UI.RepositoryExplorer;
using SharpGit;

namespace VisualGit.UI.GitLog
{
    /// <summary>
    /// 
    /// </summary>
    partial class LogRevisionControl : UserControl, ICurrentItemSource<IGitLogItem>
    {
        readonly Action<GitLogArgs> _logAction;
        readonly object _instanceLock = new object();
        readonly Queue<LogRevisionItem> _logItems = new Queue<LogRevisionItem>();
        LogRequest _currentRequest;
        LogMode _mode;
        BusyOverlay _busyOverlay;

        public LogRevisionControl()
        {
            InitializeComponent();
            _logAction = new Action<GitLogArgs>(DoFetch);
        }
        public LogRevisionControl(IContainer container)
            : this()
        {
            container.Add(this);
        }

        LogDataSource _dataSource;
        public LogDataSource LogSource
        {
            get { return _dataSource; }
            set { _dataSource = value; logView.LogSource = value; }
        }

        IVisualGitServiceProvider _context;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set
            {
                _context = value;
                OnContextChanged();
            }
        }

        private void OnContextChanged()
        {
            logView.SelectionPublishServiceProvider = Context;
        }

        public void Start(LogMode mode)
        {
            lock (_instanceLock)
            {
                _mode = mode;
                GitLogArgs args = new GitLogArgs();
                args.Start = LogSource.Start;
                args.End = LogSource.End;

                // If we have EndRevision set, we want all items until End
                if (args.End == null || args.End.RevisionType == GitRevisionType.None)
                    args.Limit = 10;

                args.StrictNodeHistory = LogSource.StrictNodeHistory;
                args.RetrieveMergedRevisions = LogSource.IncludeMergedRevisions;

                StartFetch(args);
            }
        }

        void StartFetch(GitLogArgs args)
        {
            fetchCount += args.Limit;
            _logAction.BeginInvoke(args, null, null);
        }

        public void Reset()
        {
            lock (_instanceLock)
            {
                LogRequest rq = _currentRequest;
                if (rq != null)
                {
                    _currentRequest = null;
                    rq.Cancel = true;
                }
                _running = false;
            }

            _logItems.Clear();
            logView.Items.Clear();
            _lastRevision = null;
            fetchCount = 0;

        }

        int fetchCount;
        bool _running;
        void DoFetch(GitLogArgs args)
        {
            LogRequest rq = _currentRequest = null;
            ShowBusyIndicator();
            try
            {
                using (GitClient client = _context.GetService<IGitClientPool>().GetClient())
                {
                    GitOrigin single = EnumTools.GetSingle(LogSource.Targets);
                    if (single != null)
                    {
                        // TODO: Use peg information
                    }
                    List<Uri> uris = new List<Uri>();
                    foreach (GitOrigin o in LogSource.Targets)
                    {
                        uris.Add(o.Uri);
                    }

                    switch (_mode)
                    {
                        case LogMode.Log:
                            GitLogArgs la = new GitLogArgs();
                            la.Start = args.Start;
                            la.End = args.End;
                            la.Limit = args.Limit;
                            la.StrictNodeHistory = args.StrictNodeHistory;
                            la.RetrieveMergedRevisions = args.RetrieveMergedRevisions;

                            _currentRequest = rq = new LogRequest(la, OnReceivedItem);
                            client.Log(uris, la);
                            break;
                        case LogMode.MergesEligible:
                            throw new NotImplementedException();
#if false
                            SvnMergesEligibleArgs meArgs = new SvnMergesEligibleArgs();
                            meArgs.AddExpectedError(
                                SvnErrorCode.SVN_ERR_CLIENT_UNRELATED_RESOURCES, // File not there, prevent exception
                                SvnErrorCode.SVN_ERR_UNSUPPORTED_FEATURE); // Merge info from 1.4 server
                            meArgs.RetrieveChangedPaths = true;

                            _currentRequest = rq = new LogRequest(meArgs, OnReceivedItem);
                            client.ListMergesEligible(LogSource.MergeTarget.Target, single.Target, meArgs, null);
                            break;
#endif
                        case LogMode.MergesMerged:
                            throw new NotImplementedException();
#if false
                            SvnMergesMergedArgs mmArgs = new SvnMergesMergedArgs();
                            mmArgs.AddExpectedError(
                                SvnErrorCode.SVN_ERR_CLIENT_UNRELATED_RESOURCES, // File not there, prevent exception
                                SvnErrorCode.SVN_ERR_UNSUPPORTED_FEATURE); // Merge info from 1.4 server
                            mmArgs.RetrieveChangedPaths = true;
                            _currentRequest = rq = new LogRequest(mmArgs, OnReceivedItem);
                            client.ListMergesMerged(LogSource.MergeTarget.Target, single.Target, mmArgs, null);
                            break;
#endif
                    }
                }
            }
            finally
            {
                // Don't lock here, we can be called from within a lock
                if (rq == _currentRequest)
                {
                    _running = false;
                    OnBatchDone(rq);
                }
                HideBusyIndicator();
            }
        }

        void OnReceivedItem(object sender, GitLoggingEventArgs e)
        {
            if (sender != _currentRequest)
                return;

            LogRevisionItem lri = new LogRevisionItem(logView, _context, e);
            bool post;

            lock (_logItems)
            {
                post = (_logItems.Count == 0);

                _logItems.Enqueue(lri);
            }

            if (post)
            {
                if (IsHandleCreated)
                    BeginInvoke(new VisualGitAction(OnShowItems));
            }
        }

        string _lastRevision;
        void OnShowItems()
        {
            Debug.Assert(!InvokeRequired);

            LogRevisionItem[] items;
            lock (_logItems)
            {
                items = _logItems.Count > 0 ? _logItems.ToArray() : null;

                _logItems.Clear();
            }

            if (items != null)
            {
                logView.Items.AddRange(items);
                _lastRevision = items[items.Length - 1].Revision;
            }
        }

        void OnBatchDone(LogRequest rq)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<LogRequest>(OnBatchDone), rq);
                return;
            }

            if (_fetchAll)
                FetchAll();

            if (BatchDone != null)
                BatchDone(this, new BatchFinishedEventArgs(rq, logView.Items.Count, _logItems.Count));

            ExtendList();
        }

        internal event EventHandler<BatchFinishedEventArgs> BatchDone;

        void ShowBusyIndicator()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new VisualGitAction(ShowBusyIndicator));
                return;
            }

            if (_busyOverlay == null)
                _busyOverlay = new BusyOverlay(logView, AnchorStyles.Bottom | AnchorStyles.Right);
            _busyOverlay.Show();
        }

        void HideBusyIndicator()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new VisualGitAction(HideBusyIndicator));
                return;
            }
            
            if (_busyOverlay != null)
                _busyOverlay.Hide();
        }

        private void logView_Scrolled(object sender, EventArgs e)
        {
            ExtendList();
        }

        private void logView_KeyUp(object sender, KeyEventArgs e)
        {
            ExtendList();
        }

        bool _extending;
        void ExtendList()
        {
            if (logView.VScrollPos < logView.VScrollMax - 30)
                return;

            if (!_extending)
            {
                try
                {
                    _extending = true;

                    BeginInvoke(new VisualGitAction(DoExtendList));
                }
                catch
                {
                    _extending = false;
                }
            }
        }

        void DoExtendList()
        {
            try
            {
                lock (_instanceLock)
                {
                    if (!_running && logView.Items.Count == fetchCount)
                    {
                        _running = true;

                        GitLogArgs args = new GitLogArgs();
                        args.Start = (GitRevision)_lastRevision - 1;
                        args.End = LogSource.End;
                        args.Limit = 20;
                        args.StrictNodeHistory = LogSource.StrictNodeHistory;
                        args.RetrieveMergedRevisions = LogSource.IncludeMergedRevisions;

                        StartFetch(args);
                    }
                }
            }
            finally
            {
                _extending = false;
            }
        }

        bool _fetchAll;
        internal void FetchAll()
        {
            lock (_instanceLock)
            {
                if (_running)
                {
                    _fetchAll = true;
                    return;
                }
                _fetchAll = false;


                GitLogArgs args = new GitLogArgs();
                if (_lastRevision != null)
                {
                    args.Start = (GitRevision)_lastRevision - 1;
                }
                else
                {
                    lock (_logItems)
                    {
                        if (_logItems.Count > 0)
                        {
                            LogRevisionItem[] items = _logItems.ToArray();
                            var revision = (GitRevision)items[items.Length - 1].Revision - 1;
                            // revision should not be < 0
                            args.Start = revision == null ? GitRevision.Zero : revision;
                        }
                    }
                }
                args.End = LogSource.End;
                args.StrictNodeHistory = LogSource.StrictNodeHistory;
                args.RetrieveMergedRevisions = LogSource.IncludeMergedRevisions;

                StartFetch(args);
            }
        }

        #region ICurrentItemSource<IGitLogItem> Members
        public event EventHandler<CurrentItemEventArgs<IGitLogItem>> SelectionChanged;

        public event EventHandler<CurrentItemEventArgs<IGitLogItem>> FocusChanged;

        public IGitLogItem FocusedItem
        {
            get
            {
                if (logView.FocusedItem == null)
                    return null;

                return new LogItem((LogRevisionItem)logView.FocusedItem, LogSource.RepositoryRoot);
            }
        }

        readonly IList<IGitLogItem> _selectedItems = new List<IGitLogItem>();
        public IList<IGitLogItem> SelectedItems
        {
            get
            {
                return _selectedItems;
            }
        }

        #endregion

        private void logRevisionControl1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (FocusChanged != null)
                FocusChanged(this, new CurrentItemEventArgs<IGitLogItem>(this));

            FireSelectionChanged();

            ExtendList();
        }

        void FireSelectionChanged()
        {
            _selectedItems.Clear();
            foreach (int i in logView.SelectedIndices)
                _selectedItems.Add(new LogItem((LogRevisionItem)logView.Items[i], LogSource.RepositoryRoot));

            OnSelectionChanged(new CurrentItemEventArgs<IGitLogItem>(this));
        }
        protected virtual void OnSelectionChanged(CurrentItemEventArgs<IGitLogItem> e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void logRevisionControl1_ShowContextMenu(object sender, MouseEventArgs e)
        {
            if (Context == null)
                return;

            Point screen;

            bool headerContextMenu = false;

            if (e.X == -1 && e.Y == -1)
            {
                if (logView.SelectedItems.Count > 0)
                {
                    screen = logView.PointToScreen(logView.SelectedItems[logView.SelectedItems.Count - 1].Position);
                }
                else
                {
                    headerContextMenu = true;
                    screen = logView.PointToScreen(new Point(1, 1));
                }
            }
            else
            {
                headerContextMenu = (logView.PointToClient(e.Location).Y < logView.HeaderHeight);
                screen = e.Location;
            }

            IVisualGitCommandService cs = Context.GetService<IVisualGitCommandService>();
            cs.ShowContextMenu(headerContextMenu ? VisualGitCommandMenu.ListViewHeader : VisualGitCommandMenu.LogViewerContextMenu, screen);
        }        
    }

    public sealed class BatchFinishedEventArgs : EventArgs
    {
        readonly int _batchCount;
        readonly int _totalCount;
        readonly LogRequest _rq;
        internal BatchFinishedEventArgs(LogRequest rq, int totalCount, int batchCount)
        {
            _rq = rq;
            _totalCount = totalCount;
            _batchCount = batchCount;
        }

        public int TotalCount
        {
            get { return _totalCount; }
        }

        public int BatchCount
        {
            get { return _batchCount; }
        }

        internal LogRequest Request
        {
            get { return _rq; }
        }
    }

    public enum LogMode
    {
        Log,
        MergesEligible,
        MergesMerged
    }
}
