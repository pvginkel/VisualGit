using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.UI.VSSelectionControls;

namespace VisualGit.UI.PendingChanges.Commits
{
    class PendingCommitItem : SmartListViewItem
    {
        readonly PendingChange _change;
        string _lastChangeList;

        public PendingCommitItem(PendingCommitsView view, PendingChange change)
            : base(view)
        {
            if (change == null)
                throw new ArgumentNullException("change");

            _change = change;

            //initially check only if this change does not belong to an "ignore" change list
            Checked = !change.IgnoreOnCommit;

            RefreshText(view.Context);
        }

        public void RefreshText(IVisualGitServiceProvider context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            IFileStatusCache cache = context.GetService<IFileStatusCache>();

            ImageIndex = PendingChange.IconIndex;
            GitItem item = cache[FullPath];

            if (item == null)
                throw new InvalidOperationException(); // Item no longer valued

            PendingChangeStatus pcs = PendingChange.Change ?? new PendingChangeStatus(PendingChangeKind.None);

            SetValues(
                pcs.PendingCommitText,
                _lastChangeList = PendingChange.ChangeList,
                GetDirectory(item),
                PendingChange.FullPath,
                item.IsLocked ? PCStrings.LockedValue : "", // Locked
                SafeDate(item.Modified), // Modified
                PendingChange.Name,
                PendingChange.RelativePath,
                PendingChange.Project,
                GetRevision(PendingChange),
                PendingChange.FileType,
                SafeWorkingCopy(item));

            if (!SystemInformation.HighContrast)
            {
                System.Drawing.Color clr = System.Drawing.Color.Black;

                if (item.IsConflicted || PendingChange.Kind == PendingChangeKind.WrongCasing)
                    clr = System.Drawing.Color.Red;
                else if (item.IsDeleteScheduled)
                    clr = System.Drawing.Color.DarkRed;
                else if (item.Status.IsCopied || item.Status.CombinedStatus == SharpSvn.SvnStatus.Added)
                    clr = System.Drawing.Color.FromArgb(100, 0, 100);
                else if (!item.IsVersioned)
                {
                    if (item.InSolution && !item.IsIgnored)
                        clr = System.Drawing.Color.FromArgb(100, 0, 100); // Same as added+copied
                    else
                        clr = System.Drawing.Color.Black;
                }
                else if (item.IsModified)
                    clr = System.Drawing.Color.DarkBlue;

                ForeColor = clr;
            }
        }

        private string GetRevision(PendingChange PendingChange)
        {
            if (PendingChange.Revision.HasValue)
                return PendingChange.Revision.ToString();
            else
                return "";
        }

        private string SafeDate(DateTime dateTime)
        {
            if (dateTime.Ticks == 0 || dateTime.Ticks == 1)
                return "";

            DateTime n = dateTime.ToLocalTime();

            if (n < DateTime.Now - new TimeSpan(24, 0, 0))
                return n.ToString("d");
            else
                return n.ToString("T");
        }

        private string GetDirectory(GitItem gitItem)
        {
            if (gitItem.IsDirectory)
                return gitItem.FullPath;
            else
                return gitItem.Directory;
        }

        static string SafeWorkingCopy(GitItem gitItem)
        {
            GitWorkingCopy wc = gitItem.WorkingCopy;
            if (wc == null)
                return "";

            return wc.FullPath;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public PendingChange PendingChange
        {
            get { return _change; }
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _change.FullPath; }
        }


        /// <summary>
        /// Gets change list at the time of the last refresh
        /// </summary>
        /// <value>The last change list.</value>
        /// <remarks>Used for checking for changelist changes</remarks>
        internal string LastChangeList
        {
            get { return _lastChangeList; }
        }
    }
}
