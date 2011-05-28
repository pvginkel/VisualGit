using System;
using System.Collections.Generic;
using VisualGit.Scc;
using System.Diagnostics;
using VisualGit.Scc.UI;

namespace VisualGit.UI.GitLog
{
    public sealed partial class LogViewerDialog : VSContainerForm, ILogControl
    {
        private GitOrigin _logTarget;

        public LogViewerDialog()
        {
            InitializeComponent();
        }

        public LogViewerDialog(GitOrigin target)
            : this()
        {
            LogTarget = target;
        }

        protected override void OnContextChanged(EventArgs e)
        {
            base.OnContextChanged(e);
            logViewerControl.Context = Context;
        }

        /// <summary>
        /// Gets an instance of the <code>LogControl</code>.
        /// </summary>
        internal LogControl LogControl
        {
            get { return logViewerControl; }
        }

        /// <summary>
        /// The target of the log.
        /// </summary>
        public GitOrigin LogTarget
        {
            [DebuggerStepThrough]
            get { return _logTarget; }
            set { _logTarget = value; }
        }

        public IEnumerable<IGitLogItem> SelectedItems
        {
            get { return logViewerControl.SelectedItems; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;
        
            if (LogTarget == null)
                throw new InvalidOperationException("Log target is null");

            logViewerControl.StartLog(new GitOrigin[] { LogTarget }, null, null);
        }

        #region ILogControl Members

        public bool ShowChangedPaths
        {
            get { return LogControl.ShowChangedPaths; }
            set { LogControl.ShowChangedPaths = value; }
        }

        public bool ShowLogMessage
        {
            get { return LogControl.ShowLogMessage; }
            set { LogControl.ShowLogMessage = value; }
        }

        public bool StrictNodeHistory
        {
            get { return LogControl.StrictNodeHistory; }
            set { LogControl.StrictNodeHistory = value; }
        }

        public bool IncludeMergedRevisions
        {
            get { return LogControl.IncludeMergedRevisions; }
            set { LogControl.IncludeMergedRevisions = value; }
        }

        public void FetchAll()
        {
            LogControl.FetchAll();
        }

        public void Restart()
        {
            LogControl.Restart();
        }

        public IList<GitOrigin> Origins
        {
            get { return new GitOrigin[] { LogTarget }; }
        }

        #endregion
    }
}
