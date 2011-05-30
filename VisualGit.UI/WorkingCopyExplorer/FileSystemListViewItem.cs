using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.VSSelectionControls;
using System.IO;
using System.Diagnostics;
using VisualGit.VS;
using VisualGit.Scc;

namespace VisualGit.UI.WorkingCopyExplorer
{
    class FileSystemListViewItem : SmartListViewItem
    {
        readonly GitItem _gitItem;

        public FileSystemListViewItem(SmartListView view, GitItem item)
            : base(view)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            _gitItem = item;

            ImageIndex = View.IconMapper.GetIcon(item.FullPath);

            RefreshValues();
        }

        FileSystemDetailsView View
        {
            get { return base.ListView as FileSystemDetailsView; }
        }

        public GitItem GitItem
        {
            [DebuggerStepThrough]
            get { return _gitItem; }
        }

        PendingChangeStatus _chg;
    
        private void RefreshValues()
        {
            bool exists = GitItem.Exists;
            string name = string.IsNullOrEmpty(GitItem.Name) ? GitItem.FullPath : GitItem.Name;

            VisualGitStatus status = GitItem.Status;
            PendingChangeKind kind = PendingChange.CombineStatus(status.State, GitItem.IsTreeConflicted, GitItem);

            if (_chg == null || _chg.State != kind)
                _chg = new PendingChangeStatus(kind);

            SetValues(
                name,
                Modified.ToString("g"),
                View.Context.GetService<IFileIconMapper>().GetFileType(GitItem),
                _chg.ExplorerText,
                "",
                GitItem.Status.Revision.ToString(),
                GitItem.Status.LastChangeTime.ToLocalTime().ToString(),
                GitItem.Status.LastChangeRevision.ToString(),
                GitItem.Status.LastChangeAuthor,
                GitItem.Status.State.ToString(),
                "",
                GitItem.Status.IsCopied.ToString(),
                GitItem.IsConflicted.ToString(),
                GitItem.FullPath
                );

            StateImageIndex = (int)View.StatusMapper.GetStatusImageForGitItem(GitItem);
        }

        internal bool IsDirectory
        {
            get { return GitItem.IsDirectory; }
        }

        internal DateTime Modified
        {
            get { return GitItem.Modified; }

        }
    }
}
