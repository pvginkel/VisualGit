using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI.VSSelectionControls;
using SharpSvn;
using VisualGit.VS;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.PendingChanges.Synchronize
{
    class SynchronizeListItem : SmartListViewItem
    {
        GitItem _item;
        GitStatusEventArgs _status;
        PendingChangeKind _localChange;
        PendingChangeStatus _localStatus;

        public SynchronizeListItem(SynchronizeListView list, GitItem item, GitStatusEventArgs status)
            : base(list)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _item = item;
            _status = status;

            _localChange = PendingChange.CombineStatus(status.LocalContentStatus, item.IsTreeConflicted, item);
            _localStatus = new PendingChangeStatus(_localChange);

            UpdateText();
        }

        IVisualGitServiceProvider Context
        {
            get { return ((SynchronizeListView)ListView).Context; }
        }

        private void UpdateText()
        {
            IFileIconMapper mapper = Context.GetService<IFileIconMapper>();

            ImageIndex = GetIcon(mapper);

            StateImageIndex = mapper.GetStateIcon(GetIcon(_status));

            SetValues(
                _item.Status.ChangeList,
                _item.Directory,
                _item.FullPath,
                _localStatus.PendingCommitText,
                "", // Locked
                SafeDate(_item.Modified),
                _item.Name,
                GetRelativePath(_item),
                GetProject(_item),
                "",
                Context.GetService<IFileIconMapper>().GetFileType(_item),
                SafeWorkingCopy(_item));
        }

        private int GetIcon(IFileIconMapper mapper)
        {
            if (GitItem.Exists)
                return mapper.GetIcon(_item.FullPath);
            else if (_status.NodeKind == GitNodeKind.Directory)
                return mapper.DirectoryIcon;
            else
                return mapper.GetIconForExtension(_item.Extension);
        }

        private StateIcon GetIcon(GitStatusEventArgs status)
        {
            // TODO: Handle more special cases
            GitStatus st = status.LocalContentStatus;

            bool localModified = IsMod(status.LocalContentStatus);

            if (localModified)
                return StateIcon.Outgoing;
            else
                return StateIcon.Blank;
        }

        private bool IsMod(GitStatus svnStatus)
        {
            switch (svnStatus)
            {
                case GitStatus.None:
                case GitStatus.Normal:
                    return false;
                default:
                    return true;
            }
        }

        static string SafeWorkingCopy(GitItem item)
        {
            if (item != null && item.WorkingCopy != null)
                return item.WorkingCopy.FullPath;

            return "";
        }

        static string GetProject(GitItem _item)
        {
            return "";
        }

        string GetRelativePath(GitItem item)
        {
            IVisualGitSolutionSettings ss = Context.GetService<IVisualGitSolutionSettings>();

            string path = ss.ProjectRootWithSeparator;

            if (!string.IsNullOrEmpty(path) && item.FullPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                return item.FullPath.Substring(path.Length).Replace('\\', '/');

            return item.FullPath;
        }

        static string SafeDate(DateTime dateTime)
        {
            if (dateTime.Ticks == 0 || dateTime.Ticks == 1)
                return "";

            DateTime n = dateTime.ToLocalTime();

            if (n < DateTime.Now - new TimeSpan(24, 0, 0))
                return n.ToString("d");
            else
                return n.ToString("T");
        }

        static string CombineChange(GitStatus svnStatus, GitStatus svnStatus_2)
        {
            return svnStatus.ToString() + " " + svnStatus_2.ToString();
        }

        public GitItem GitItem
        {
            get { return _item; }
        }
    }
}
