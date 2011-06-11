using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SharpSvn;
using System.Diagnostics;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.GitLog
{
    sealed partial class LogControl : UserControl, ICurrentItemSource<IGitLogItem>, ICurrentItemDestination<IGitLogItem>
    {
        public LogControl()
        {
            InitializeComponent();
            ItemSource = logRevisionControl1;
            logRevisionControl1.BatchDone += logRevisionControl1_BatchDone;

            LogSource = new LogDataSource();
            LogSource.Synchronizer = this;

            logChangedPaths1.LogSource = LogSource;
            logRevisionControl1.LogSource = LogSource;
        }

        LogDataSource _dataSource;
        public LogDataSource LogSource
        {
            get { return _dataSource; }
            private set { _dataSource = value; }
        }

        void logRevisionControl1_BatchDone(object sender, BatchFinishedEventArgs e)
        {
            if (BatchFinished != null)
                BatchFinished(sender, e);
        }

        public void FetchAll()
        {
            logRevisionControl1.FetchAll();
        }

        public LogControl(IContainer container)
            : this()
        {
            container.Add(this);
        }

        IVisualGitServiceProvider _context;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set
            {
                _context = value;

                logRevisionControl1.Context = value;
                logChangedPaths1.Context = value;
            }
        }

        LogMode _mode;
        //[Obsolete]
        public LogMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public void StartLog(ICollection<GitOrigin> targets, GitRevision start, GitRevision end)
        {
            if (targets == null)
                throw new ArgumentNullException("targets");

            LogSource.Targets = targets;
            LogSource.Start = start;
            LogSource.End = end;

            logRevisionControl1.Reset();
            logChangedPaths1.Reset();
            logMessageView1.Reset();
            logRevisionControl1.Start(LogMode.Log);
        }


        /// <summary>
        /// Starts the merges eligible logger. Checking whick revisions of source (Commonly Uri) 
        /// are eligeable for mergeing to target (Commonly path)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public void StartMergesEligible(IVisualGitServiceProvider context, GitOrigin target, GitTarget source)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");

            LogSource.Targets = new GitOrigin[] { new GitOrigin(context, source, target.RepositoryRoot) }; // Must be from the same repository!
            LogSource.MergeTarget = target;
            logRevisionControl1.Reset();
            logChangedPaths1.Reset();
            logMessageView1.Reset();
            logRevisionControl1.Start(LogMode.MergesEligible);
        }

        public void StartMergesMerged(IVisualGitServiceProvider context, GitOrigin target, GitTarget source)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");

            LogSource.Targets = new GitOrigin[] { new GitOrigin(context, source, target.RepositoryRoot) }; // Must be from the same repository!
            LogSource.MergeTarget = target;
            logRevisionControl1.Reset();
            logChangedPaths1.Reset();
            logMessageView1.Reset();
            logRevisionControl1.Start(LogMode.MergesMerged);
        }

        public void Restart()
        {
            logRevisionControl1.Reset();
            logChangedPaths1.Reset();
            logMessageView1.Reset();
            logRevisionControl1.Start(Mode);
        }

        [DefaultValue(false)]
        public bool IncludeMergedRevisions
        {
            get { return LogSource.IncludeMergedRevisions; }
            set { LogSource.IncludeMergedRevisions = value; }
        }

        [DefaultValue(false)]
        public bool StrictNodeHistory
        {
            get { return LogSource.StrictNodeHistory; }
            set { LogSource.StrictNodeHistory = value; }
        }

        bool _logMessageHidden;
        [DefaultValue(true)]
        public bool ShowLogMessage
        {
            get { return !_logMessageHidden; }
            set
            {
                _logMessageHidden = !value;
                UpdateSplitPanels();
            }
        }
        bool _changedPathsHidden;
        [DefaultValue(true)]
        public bool ShowChangedPaths
        {
            get { return !_changedPathsHidden; }
            set
            {
                _changedPathsHidden = !value;
                UpdateSplitPanels();
            }
        }

        void UpdateSplitPanels()
        {
            splitContainer1.Panel2Collapsed = !ShowChangedPaths && !ShowLogMessage;
            splitContainer2.Panel1Collapsed = !ShowChangedPaths;
            splitContainer2.Panel2Collapsed = !ShowLogMessage;
        }

        public event EventHandler<BatchFinishedEventArgs> BatchFinished;

        public event EventHandler<CurrentItemEventArgs<IGitLogItem>> SelectionChanged;

        public event EventHandler<CurrentItemEventArgs<IGitLogItem>> FocusChanged;

        public IGitLogItem FocusedItem
        {
            get { return ItemSource == null ? null : ItemSource.FocusedItem; }
        }

        public IList<IGitLogItem> SelectedItems
        {
            get { return ItemSource == null ? null : ItemSource.SelectedItems; }
        }

        #region ICurrentItemDestination<IGitLogItem> Members

        ICurrentItemSource<IGitLogItem> _itemSource;
        public ICurrentItemSource<IGitLogItem> ItemSource
        {
            [DebuggerStepThrough]
            get { return _itemSource; }
            set
            {
                _itemSource = value;
                value.FocusChanged += OnFocusChanged;
                value.SelectionChanged += OnSelectionChanged;
            }
        }

        void OnFocusChanged(object sender, CurrentItemEventArgs<IGitLogItem> e)
        {
            if (FocusChanged != null)
                FocusChanged(sender, e);
        }

        void OnSelectionChanged(object sender, CurrentItemEventArgs<IGitLogItem> e)
        {
            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        #endregion
    }
}
