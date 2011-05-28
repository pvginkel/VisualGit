using System;
using System.Collections.Generic;

using SharpSvn;

using VisualGit.Scc;
using VisualGit.Scc.UI;
using VisualGit.UI.GitLog;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeRevisionsSelectionPage : BaseWizardPage, ILogControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeRevisionsSelectionPage()
        {
            IsPageComplete = false;

            Text = MergeStrings.MergeRevisionsSelectionPageTitle;
            this.Message = new WizardMessage(MergeStrings.MergeRevisionsSelectionPageMessage);

            SelectionChanged += new EventHandler<EventArgs>(MergeRevisionsSelectionPage_SelectionChanged);
            InitializeComponent();

            logToolControl1.SelectionChanged += new EventHandler<CurrentItemEventArgs<IGitLogItem>>(logToolControl1_SelectionChanged);

            logToolControl1.StrictNodeHistory = true;
        }

        /// <summary>
        /// Returns an array of revisions, in numerical order, to be merged.
        /// </summary>
        public IEnumerable<SvnRevisionRange> MergeRevisions
        {
            get
            {
                IGitLogItem start = null;
                IGitLogItem end = null;
                int previousIndex = -1;
                List<IGitLogItem> logitems = new List<IGitLogItem>(SelectedRevisions);
                logitems.Sort(delegate(IGitLogItem a, IGitLogItem b) { return a.Index.CompareTo(b.Index); });

                foreach (IGitLogItem item in logitems)
                {
                    if (start == null)
                    {
                        start = item;
                        end = item;
                    }
                    else if (previousIndex + 1 == item.Index)
                    {
                        // range is still contiguous, move end ptr
                        end = item;
                    }
                    else
                    {
                        // The start of a new range because it's no longer contiguous
                        // return the previous range and start a new one
                        yield return new SvnRevisionRange(start.Revision - 1, end.Revision);

                        start = item;
                        end = item;
                    }

                    previousIndex = item.Index;
                }

                // The loop doesn't handle the last range
                if (start != null && end != null)
                {
                    yield return new SvnRevisionRange(start.Revision - 1, end.Revision);
                }
            }
        }

        void MergeRevisionsSelectionPage_SelectionChanged(object sender, EventArgs e)
        {
            IsPageComplete = SelectedRevisions.Count > 0;

            if (IsPageComplete)
                Wizard.MergeRevisions = MergeRevisions;
            else
                Wizard.MergeRevisions = null;
        }

        protected override void OnPageChanged(EventArgs e)
        {
            base.OnPageChanged(e);
        }

        public IList<VisualGit.Scc.IGitLogItem> SelectedRevisions
        {
            get
            {
                return logToolControl1.SelectedItems;
            }
        }

        protected override void OnContextChanged(EventArgs e)
        {
            logToolControl1.Context = Context;
        }

        public event EventHandler<EventArgs> SelectionChanged;

        void logToolControl1_SelectionChanged(object sender, CurrentItemEventArgs<IGitLogItem> e)
        {
            OnSelectionChanged(EventArgs.Empty);
        }

        void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        /// <summary>
        /// Gets or sets the merge source.
        /// </summary>
        /// <value>The merge source.</value>
        public GitOrigin MergeSource
        {
            get { return Wizard.MergeSource; }
        }

        public GitOrigin MergeTarget
        {
            get { return new GitOrigin(Wizard.MergeTarget); }
        }

        protected void PopulateUI()
        {
            switch (Wizard.LogMode)
            {
                case LogMode.MergesEligible:
                    logToolControl1.IncludeMergedRevisions = false;
                    logToolControl1.StartMergesEligible(Context, MergeTarget, MergeSource.Target);
                    break;
                case LogMode.MergesMerged:
                    logToolControl1.IncludeMergedRevisions = true;
                    logToolControl1.StartMergesMerged(Context, MergeTarget, MergeSource.Target);
                    break;
                case LogMode.Log:
                    logToolControl1.StartLog(new GitOrigin[] { new GitOrigin(Context, MergeSource.Target, MergeTarget.RepositoryRoot) }, null, null);
                    break;
            }
        }

        private void WizardDialog_PageChangeEvent(object sender, EventArgs e)
        {
            if (Wizard.CurrentPage == this)
            {
                PopulateUI();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Wizard.PageChanged += new EventHandler(WizardDialog_PageChangeEvent);
        }

        private void logToolControl1_BatchFinished(object sender, BatchFinishedEventArgs e)
        {
            if (e.TotalCount == 0)
            {
                Message = new WizardMessage(MergeStrings.NoLogItems, WizardMessage.MessageType.Error);
            }
            else
            {
                Message = new WizardMessage("", WizardMessage.MessageType.None);
            }
        }

        #region ILogControl Members

        bool ILogControl.ShowChangedPaths
        {
            get { return logToolControl1.ShowChangedPaths; }
            set { logToolControl1.ShowChangedPaths = value; }
        }

        bool ILogControl.ShowLogMessage
        {
            get { return logToolControl1.ShowLogMessage; }
            set { logToolControl1.ShowLogMessage = value; }
        }

        bool ILogControl.StrictNodeHistory
        {
            get { return logToolControl1.StrictNodeHistory; }
            set { logToolControl1.StrictNodeHistory = value; }
        }

        bool ILogControl.IncludeMergedRevisions
        {
            get { return logToolControl1.IncludeMergedRevisions; }
            set { logToolControl1.IncludeMergedRevisions = value; }
        }

        void ILogControl.FetchAll()
        {
            logToolControl1.FetchAll();
        }

        void ILogControl.Restart()
        {
            logToolControl1.Restart();
        }

        IList<GitOrigin> ILogControl.Origins
        {
            get { return new GitOrigin[] { MergeSource }; }
        }

        #endregion
    }
}
