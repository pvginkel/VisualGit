using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using System.Collections.ObjectModel;
using System.Diagnostics;
using VisualGit.VS;
using VisualGit.Commands;
using System.Windows.Forms;
using VisualGit.UI;
using SharpGit;

namespace VisualGit.Services.PendingChanges
{
    class PendingCommitState : VisualGitService, IDisposable
    {
        GitClient _client;
        HybridCollection<PendingChange> _changes = new HybridCollection<PendingChange>();
        HybridCollection<string> _commitPaths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
        string _logMessage;

        public PendingCommitState(IVisualGitServiceProvider context, IEnumerable<PendingChange> changes)
            : base(context)
        {
            if (changes == null)
                throw new ArgumentNullException("changes");

            _changes.UniqueAddRange(changes);

            foreach (PendingChange pc in _changes)
            {
                if (!_commitPaths.Contains(pc.FullPath))
                    _commitPaths.Add(pc.FullPath);
            }
        }

        public GitClient Client
        {
            get
            {
                if (_client == null)
                    _client = GetService<IGitClientPool>().GetNoUIClient();

                return _client;
            }
        }

        public HybridCollection<PendingChange> Changes
        {
            get { return _changes; }
        }

        public HybridCollection<string> CommitPaths
        {
            get { return _commitPaths; }
        }

        public string LogMessage
        {
            get { return _logMessage; }
            set { _logMessage = value; }
        }

        [DebuggerStepThrough]
        public new T GetService<T>()
            where T : class
        {
            return base.GetService<T>();
        }

        [DebuggerStepThrough]
        public new T GetService<T>(Type serviceType)
            where T : class
        {
            return base.GetService<T>(serviceType);
        }

        VisualGitMessageBox _mb;
        public VisualGitMessageBox MessageBox
        {
            get { return _mb ?? (_mb = new VisualGitMessageBox(this)); }
        }

        IFileStatusCache _cache;
        public IFileStatusCache Cache
        {
            get { return _cache ?? (_cache = GetService<IFileStatusCache>()); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            FlushState();
        }

        #endregion

        bool IsDirectory(GitItem item)
        {
            return item.IsDirectory || item.NodeKind == GitNodeKind.Directory;
        }

        public GitDepth CalculateCommitDepth()
        {
            GitDepth depth = GitDepth.Empty;
            bool requireInfinity = false;
            bool noDepthInfinity = false;
            string dirToDelete = null;

            foreach (string path in CommitPaths)
            {
                GitItem item = Cache[path];

                if (IsDirectory(item))
                {
                    if (item.IsDeleteScheduled)
                    {
                        // Infinity = OK
                        dirToDelete = item.FullPath;
                        requireInfinity = true;
                    }
                    else
                        noDepthInfinity = true;
                }
            }

            if (requireInfinity && !noDepthInfinity)
                depth = GitDepth.Infinity;

            if (requireInfinity && noDepthInfinity)
            {
                // Houston we have a problem.
                // - Directory deletes require depth infinity
                // - There is another directory commit

                string nodeNotToCommit = null;
                string nodeToCommit = null;

                // Let's see if committing with depth infinity would go wrong
                bool hasOther = false;
                using (GitClient cl = GetService<IGitClientPool>().GetNoUIClient())
                {
                    bool cancel = false;
                    GitStatusArgs sa = new GitStatusArgs();
                    sa.ThrowOnError = false;
                    sa.ThrowOnCancel = false;
                    sa.RetrieveIgnoredEntries = false;
                    sa.Depth = GitDepth.Infinity;

                    foreach (string path in CommitPaths)
                    {
                        GitItem item = Cache[path];

                        if (!IsDirectory(item) || item.IsDeleteScheduled)
                            continue; // Only check not to be deleted directories

                        if (!cl.Status(path, sa,
                            delegate(object sender, GitStatusEventArgs ee)
                            {
                                switch (ee.LocalContentStatus)
                                {
                                    case GitStatus.Zero:
                                    case GitStatus.None:
                                    case GitStatus.Normal:
                                    case GitStatus.Ignored:
                                    case GitStatus.NotVersioned:
                                        return;
                                }
                                if (!CommitPaths.Contains(ee.FullPath))
                                {
                                    nodeNotToCommit = ee.FullPath;
                                    nodeToCommit = path;
                                    hasOther = true;
                                    ee.Cancel = true;
                                    cancel = true;
                                }
                            }))
                        {
                            if (cancel)
                                break;
                        }

                        if (hasOther)
                            break;
                    }
                }

                if (!hasOther)
                {
                    // Ok; it is safe to commit with depth infinity; all items that would be committed
                    // with infinity would have been committed anyway

                    depth = GitDepth.Infinity;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(PccStrings.InvalidCommitCombination);
                    sb.AppendLine();
                    sb.AppendLine(PccStrings.DirectoryDeleteAndNodeToKeep);

                    sb.AppendFormat(PccStrings.DirectoryDeleteX, dirToDelete ?? "<null>");
                    sb.AppendLine();
                    sb.AppendFormat(PccStrings.DirectoryToCommit, nodeToCommit ?? "<null>");
                    sb.AppendLine();
                    sb.AppendFormat(PccStrings.ShouldNotCommitX, nodeNotToCommit ?? "<null>");

                    MessageBox.Show(sb.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return GitDepth.Unknown;
                }

            }

            // Returns SvnDepth.Infinity if there are directories scheduled for commit 
            // and all directories scheduled for commit are to be deleted
            //
            // Returns SvnDepth.Empty in all other cases
            return depth;
        }

        internal void FlushState()
        {
            if (_client != null)
            {
                IDisposable cl = _client;
                _client = null;
                cl.Dispose();
            }
        }
    }
}
