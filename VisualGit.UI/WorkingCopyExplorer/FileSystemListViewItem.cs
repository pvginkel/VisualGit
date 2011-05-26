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
        readonly SvnItem _svnItem;

        public FileSystemListViewItem(SmartListView view, SvnItem item)
            : base(view)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            _svnItem = item;

            ImageIndex = View.IconMapper.GetIcon(item.FullPath);

            RefreshValues();
        }

        FileSystemDetailsView View
        {
            get { return base.ListView as FileSystemDetailsView; }
        }

        public SvnItem SvnItem
        {
            [DebuggerStepThrough]
            get { return _svnItem; }
        }

        PendingChangeStatus _chg;
    
        private void RefreshValues()
        {
            bool exists = SvnItem.Exists;
            string name = string.IsNullOrEmpty(SvnItem.Name) ? SvnItem.FullPath : SvnItem.Name;

            VisualGitStatus status = SvnItem.Status;
            PendingChangeKind kind = PendingChange.CombineStatus(status.LocalContentStatus, status.LocalPropertyStatus, SvnItem.IsTreeConflicted, SvnItem);

            if (_chg == null || _chg.State != kind)
                _chg = new PendingChangeStatus(kind);

            SetValues(
                name,
                Modified.ToString("g"),
                View.Context.GetService<IFileIconMapper>().GetFileType(SvnItem),
                _chg.ExplorerText,
                SvnItem.Status.IsLockedLocal ? VisualGit.UI.PendingChanges.PCStrings.LockedValue : "",
                SvnItem.Status.Revision.ToString(),
                SvnItem.Status.LastChangeTime.ToLocalTime().ToString(),
                SvnItem.Status.LastChangeRevision.ToString(),
                SvnItem.Status.LastChangeAuthor,
                SvnItem.Status.LocalContentStatus.ToString(),
                SvnItem.Status.LocalPropertyStatus.ToString(),
                SvnItem.Status.IsCopied.ToString(),
                SvnItem.IsConflicted.ToString(),
                SvnItem.FullPath
                );

            StateImageIndex = (int)View.StatusMapper.GetStatusImageForSvnItem(SvnItem);
        }

        internal bool IsDirectory
        {
            get { return SvnItem.IsDirectory; }
        }

        internal DateTime Modified
        {
            get { return SvnItem.Modified; }

        }
    }
}
