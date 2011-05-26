using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI.VSSelectionControls;
using VisualGit.Scc;
using VisualGit.VS;

namespace VisualGit.UI.PendingChanges.Conflicts
{
    class ConflictListItem : SmartListViewItem
    {
        readonly PendingChange _change;
        public ConflictListItem(ConflictListView view, PendingChange change)
            : base(view)
        {
            if (change == null)
                throw new ArgumentNullException("change");

            _change = change;
            RefreshText(view.Context);
        }

        internal PendingChange PendingChange
        {
            get { return _change; }
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
                PendingChange.ChangeList,
                GetDirectory(item),
                PendingChange.FullPath,
                item.IsLocked ? PCStrings.LockedValue : "", // Locked
                SafeDate(item.Modified), // Modified
                PendingChange.Name,
                PendingChange.RelativePath,
                PendingChange.Project,
                context.GetService<IFileIconMapper>().GetFileType(item),
                SafeWorkingCopy(item));
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
        public string FullPath
        {
            get { return _change.FullPath; }
        }
    }
}
