using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using VisualGit.Scc.UI;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.UI.SvnLog
{
    public sealed partial class LogToolWindowControl : VisualGitToolWindowControl, ILogControl
    {
        string _originalText;
        IList<GitOrigin> _origins;
        public LogToolWindowControl()
        {
            InitializeComponent();
            LogSource = logControl.LogSource;
        }

        public LogToolWindowControl(IContainer container)
            : this()
        {
            container.Add(this);
        }

        LogDataSource _dataSource;
        internal LogDataSource LogSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            _originalText = Text;

            ToolWindowHost.CommandContext = VisualGitId.LogContextGuid;
            ToolWindowHost.KeyboardContext = VisualGitId.LogContextGuid;
            logControl.Context = Context;
        }

        public IList<GitOrigin> Origins
        {
            get
            {
                if (_origins == null)
                    return null;

                return new List<GitOrigin>(_origins);
            }
        }

        void UpdateTitle()
        {
            Text = _originalText;
            if (_origins == null)
                return;

            StringBuilder sb = new StringBuilder(Text);
            int n = 0;

            foreach (GitOrigin origin in _origins)
            {
                sb.Append((n++ == 0) ? " - " : ", ");
                    
                sb.Append(origin.Target.FileName);
            }

            Text = sb.ToString();
        }

        public void StartLog(GitOrigin target, SvnRevision start, SvnRevision end)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            StartLog(new GitOrigin[] { target }, start, end);
        }

        public void StartLog(ICollection<GitOrigin> targets, SvnRevision start, SvnRevision end)
        {
            if (targets == null)
                throw new ArgumentNullException("targets");

            _origins = new List<GitOrigin>(targets);

            UpdateTitle();

            logControl.StartLog(_origins, start, end);
        }

        public void StartMergesEligible(IVisualGitServiceProvider context, GitItem target, Uri source)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            GitOrigin origin = new GitOrigin(target);
            _origins = new GitOrigin[] { origin };
            UpdateTitle();
            logControl.StartMergesEligible(context, origin, source);
        }

        public void StartMergesMerged(IVisualGitServiceProvider context, GitItem target, Uri source)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            GitOrigin origin = new GitOrigin(target);
            _origins = new GitOrigin[] { origin };
            UpdateTitle();
            logControl.StartMergesMerged(context, origin, source);
        }

        public bool LogMessageVisible
        {
            [DebuggerStepThrough]
            get { return logControl.ShowLogMessage; }
            [DebuggerStepThrough]
            set { logControl.ShowLogMessage = value; }
        }

        public bool ChangedPathsVisible
        {
            [DebuggerStepThrough]
            get { return logControl.ShowChangedPaths; }
            [DebuggerStepThrough]
            set { logControl.ShowChangedPaths = value; }
        }

        public bool IncludeMerged
        {
            [DebuggerStepThrough]
            get { return logControl.IncludeMergedRevisions; }
            [DebuggerStepThrough]
            set { logControl.IncludeMergedRevisions = value; }
        }

        [DebuggerStepThrough]
        public void FetchAll()
        {
            logControl.FetchAll();
        }

        [DebuggerStepThrough]
        public void Restart()
        {
            logControl.Restart();
        }

        #region ILogControl Members

        public bool ShowChangedPaths
        {
            get
            {
                return logControl.ShowChangedPaths;
            }
            set
            {
                logControl.ShowChangedPaths = value;
            }
        }

        public bool ShowLogMessage
        {
            get
            {
                return logControl.ShowLogMessage;
            }
            set
            {
                logControl.ShowLogMessage = value;
            }
        }

        public bool IncludeMergedRevisions
        {
            get { return logControl.IncludeMergedRevisions; }
            set
            {
                if (value != logControl.IncludeMergedRevisions)
                {
                    logControl.IncludeMergedRevisions = value;
                    logControl.Restart();
                }
            }
        }

        public bool StrictNodeHistory
        {
            [DebuggerStepThrough]
            get { return logControl.StrictNodeHistory; }
            set
            {
                if (value != StrictNodeHistory)
                {
                    logControl.StrictNodeHistory = value;
                    logControl.Restart();
                }
            }
        }

        #endregion
    }
}
