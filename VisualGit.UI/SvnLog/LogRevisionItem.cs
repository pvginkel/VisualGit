using System;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.UI.VSSelectionControls;
using System.Globalization;
using System.Drawing;
using VisualGit.Scc;
using SharpSvn.Implementation;
using System.Collections.ObjectModel;
using VisualGit.VS;
using System.Collections.Generic;

namespace VisualGit.UI.SvnLog
{
    class LogRevisionItem : SmartListViewItem
    {
        readonly IVisualGitServiceProvider _context;
        readonly SvnLoggingEventArgs _args;
        public LogRevisionItem(LogRevisionView listView, IVisualGitServiceProvider context, SvnLoggingEventArgs e)
            : base(listView)
        {
            _args = e;
            _context = context;
            RefreshText();
            UpdateColors();
        }

        [Browsable(false)]
        internal string RevisionText
        {
            get { return Revision.ToString(CultureInfo.CurrentCulture); }
        }

        void RefreshText()
        {
            SetValues(
                "", // First column must be "" to work around owner draw issues!
                RevisionText,
                Author,
                Date.ToString(CultureInfo.CurrentCulture),
                GetIssueText(),
                LogMessage);
        }

        private string GetIssueText()
        {
            StringBuilder sb = null;
            ICollection<string> issueList = new List<string>();
            foreach (IssueMarker issue in Issues)
            {
                if (!issueList.Contains(issue.Value))
                {
                    if (sb == null) { sb = new StringBuilder(); }
                    else { sb.Append(","); }
                    sb.Append(issue);
                    issueList.Add(issue.Value);
                }
            }
            return sb != null ? sb.ToString() : "";
        }

        void UpdateColors()
        {
            if (SystemInformation.HighContrast)
                return;

            if (_args.ChangedPaths == null)
                return;

            foreach (SvnChangeItem ci in _args.ChangedPaths)
            {
                if (ci.CopyFromRevision >= 0)
                    ForeColor = Color.DarkBlue;
            }
        }

        internal DateTime Date
        {
            get { return _args.Time.ToLocalTime(); }
        }

        internal string Author
        {
            get { return _args.Author; }
        }

        string _logMessage;
        internal string LogMessage
        {
            get
            {
                if (_logMessage == null && _args.LogMessage != null)
                {
                    _logMessage = _args.LogMessage.Trim().Replace("\r", "").Replace("\n", "\x23CE");
                }
                return _logMessage;
            }
        }

        internal long Revision
        {
            get { return _args.Revision; }
        }

        internal KeyedCollection<string, SvnChangeItem> ChangedPaths
        {
            get { return _args.ChangedPaths; }
        }

        internal SvnLoggingEventArgs RawData
        {
            get { return _args; }
        }

        internal IVisualGitServiceProvider Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Returns IEnumerable for issue ids combining the issues found via associated issue repository and project commit settings.
        /// </summary>
        internal IEnumerable<IssueMarker> Issues
        {
            get
            {
                string logMessage = LogMessage;

                if (string.IsNullOrEmpty(logMessage))
                    yield break;

                IVisualGitIssueService iService = Context.GetService<IVisualGitIssueService>();
                if (iService != null)
                {
                    IEnumerable<IssueMarker> issues;
                    if (iService.TryGetIssues(logMessage, out issues))
                    {
                        foreach (IssueMarker issue in issues)
                        {
                            yield return issue;
                        }
                    }
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    sealed class LogItem : VisualGitPropertyGridItem, IGitLogItem
    {
        readonly LogRevisionItem _lvi;
        public Uri _repositoryRoot;

        public LogItem(LogRevisionItem lvi, Uri repositoryRoot)
        {
            if (lvi == null)
                throw new ArgumentNullException("lvi");

            _lvi = lvi;
            _index = lvi.Index;
            _repositoryRoot = repositoryRoot;
        }

        internal LogRevisionItem ListViewItem
        {
            get { return _lvi; }
        }

        /// <summary>
        /// Gets the repository root.
        /// </summary>
        /// <value>The repository root.</value>
        [Browsable(false)]
        public Uri RepositoryRoot
        {
            get { return _repositoryRoot; }
        }

        readonly int _index;
        [Browsable(false)]
        public int Index
        {
            get 
            {
                return _index; 
            }
        }

        [Category("Git")]
        [DisplayName("Commit date")]
        public DateTime CommitDate
        {
            get { return _lvi.Date.ToLocalTime(); }
        }

        [Category("Git")]
        public string Author
        {
            get { return _lvi.Author; }
        }

        [Category("Git")]
        [DisplayName("Log message")]
        public string LogMessage
        {
            get { return _lvi.RawData.LogMessage; }
        }

        [Category("Git")]
        public long Revision
        {
            get { return _lvi.Revision; }
        }

        [Browsable(false)]
        public SvnChangeItemCollection ChangedPaths
        {
            get { return _lvi.RawData.ChangedPaths; }
        }

        /// <summary>
        /// Gets the light/second name shown in the gridview header
        /// </summary>
        /// <value></value>
        protected override string ClassName
        {
            get { return "Revision Information"; }
        }

        /// <summary>
        /// Gets the bold/first name shown in the gridview header
        /// </summary>
        /// <value></value>
        protected override string ComponentName
        {
            get { return string.Format("r{0}", Revision); }
        }

        /// <summary>
        /// Returns IEnumerable combining the issues found via associated issue repository and project commit settings.
        /// </summary>
        public System.Collections.Generic.IEnumerable<IssueMarker> Issues
        {
            get 
            {
                return _lvi.Issues;
            }
        }
    }
}
