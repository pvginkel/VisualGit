using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using SharpGit;

namespace VisualGit.Scc
{
    /// <summary>
    /// Container of Svn/SharpSvn helper tools which should be refactored to a better location
    /// in a future version, but which functionality is required to get file tracking working
    /// </summary>
    sealed class GitSccContext : VisualGitService
    {
        readonly GitClient _client;
        readonly IFileStatusCache _statusCache;
        bool _disposed;

        public GitSccContext(IVisualGitServiceProvider context)
            : base(context)
        {
            _client = context.GetService<IGitClientPool>().GetNoUIClient();
            _statusCache = GetService<IFileStatusCache>();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                ((IDisposable)_client).Dispose();
            }
            base.Dispose(disposing);
        }

        IFileStatusCache StatusCache
        {
            get { return _statusCache; }
        }

        /// <summary>
        /// Gets the working copy entry information on the specified path from its parent
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public GitStatusEventArgs SafeGetEntry(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            // We only have to look in the parent.
            // If the path is the working copy root, the name doesn't matter!

            string dir = GitTools.GetNormalizedDirectoryName(path);

            var sa = new GitStatusArgs();
            sa.Depth = GitDepth.Files;
            sa.RetrieveAllEntries = false;
            sa.RetrieveIgnoredEntries = false;
            sa.ThrowOnError = false;
            sa.ThrowOnCancel = false;

            GitStatusEventArgs entry = null;

            _client.Status(dir, sa, (sender, e) =>
                {
                    if (entry == null && path == e.FullPath)
                    {
                        entry = e;
                        e.Cancel = true;
                    }
                });

            return entry;
        }

        /// <summary>
        /// Tries to get the repository Guid of the specified path when it would be added to Git
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wcGuid"></param>
        /// <returns></returns>
        public bool TryGetRepositoryId(string path, out Guid repositoryId)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            path = GitTools.GetNormalizedFullPath(path);

            if (File.Exists(path))
                path = GitTools.GetNormalizedDirectoryName(path);

            if (Directory.Exists(path))
                path = GitTools.GetTruePath(path); // Resolve casing issues

            string root = Path.GetPathRoot(path);
            repositoryId = Guid.Empty;

            Guid repId = Guid.Empty;

            // Svn does not allow a repository in a path root 
            // (See: canonicalization rules)
            while (!string.IsNullOrEmpty(path))
            {
                if (GitTools.IsManagedPath(path))
                {
                    throw new NotImplementedException();
#if false
                    SvnInfoArgs a = new SvnInfoArgs();
                    a.ThrowOnError = false;
                    a.Depth = SvnDepth.Empty;

                    if (_svnClient.Info(new SvnPathTarget(path),
                        delegate(object sender, SvnInfoEventArgs e)
                        {
                            repId = e.RepositoryId;
                        }))
                    {
                        if (repId != Guid.Empty) // Directory was just added; Guid not updated
                        {
                            repositoryId = repId;
                            return true;
                        }
                    }
                    else
                        return false;
#endif
                }

                path = GitTools.GetNormalizedDirectoryName(path);
            }

            return false;
        }

        /// <summary>
        /// Reverts the specified path if the path specifies a file replaced with itself
        /// </summary>
        /// <param name="path"></param>
        void MaybeRevertReplaced(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            GitItem item = StatusCache[path];
            item.MarkDirty();

            if (!item.IsFile || item.Status.State != GitStatus.Replaced)
                return;

            throw new NotImplementedException();
#if false
            SvnInfoEventArgs info = null;
            SvnInfoArgs ia = new SvnInfoArgs();
            ia.ThrowOnError = false;
            ia.Depth = SvnDepth.Empty;

            if (!_svnClient.Info(new SvnPathTarget(path), ia,
                delegate(object sender, SvnInfoEventArgs e)
                {
                    e.Detach();
                    info = e;
                }))
            {
                return;
            }

            if (info == null)
                return;

            if ((info.CopyFromUri != null) && (info.Uri != info.CopyFromUri))
                return;
            else if (info.CopyFromRevision >= 0 && info.CopyFromRevision != info.Revision)
                return;

            // Ok, the file was copied back to its original location!

            SvnRevertArgs ra = new SvnRevertArgs();
            ra.Depth = SvnDepth.Empty;
            ra.ThrowOnError = false;

            // Our callers should move away the file, but we can't be to sure here
            using (MoveAway(path, true))
            {
                _svnClient.Revert(path, ra);
            }
#endif
        }

        /// <summary>
        /// Safes the wc copy to dir fixup.
        /// </summary>
        /// <param name="files">The files a dictionary mapping the result to the origin</param>
        /// <param name="toDir">To dir.</param>
        /// <returns></returns>
        /// <remarks>
        /// Files can't be renamed by this action!
        /// </remarks>
        public bool SafeWcCopyToDirFixup(IDictionary<string, string> files, string toDir)
        {
            if (files == null)
                throw new ArgumentNullException("files");
            else if (string.IsNullOrEmpty(toDir))
                throw new ArgumentNullException("toDir");

            List<string> from = new List<string>(files.Values);
            List<string> tryMove = new List<string>();

            foreach (string f in from)
            {
                if (!File.Exists(f))
                    tryMove.Add(f);
            }

            throw new NotImplementedException();
#if false
            using (MarkIgnoreFiles(files.Keys))
            using (MarkIgnoreFiles(tryMove))
            using (TempRevertForCopy(tryMove))
            {
                List<string> setReadOnly = null;
                using (MoveAwayFiles(files.Keys, true))
                {
                    EnsureAdded(toDir);

                    List<SvnPathTarget> toCopy = new List<SvnPathTarget>();
                    List<string> toMove = new List<string>();
                    foreach (string f in files.Values)
                    {
                        if (tryMove.Contains(f))
                            toMove.Add(f);
                        else
                            toCopy.Add(f);
                    }

                    if (toCopy.Count > 0)
                    {
                        SvnCopyArgs ca = new SvnCopyArgs();
                        ca.ThrowOnError = false;
                        ca.AlwaysCopyAsChild = true;

                        _svnClient.Copy(toCopy, toDir, ca);
                    }

                    if (toMove.Count > 0)
                    {
                        SvnMoveArgs ma = new SvnMoveArgs();
                        ma.ThrowOnError = false;
                        ma.AlwaysMoveAsChild = true;

                        _svnClient.Move(toMove, toDir, ma);
                    }

                    foreach (string f in files.Keys)
                    {
                        if (File.Exists(f))
                        {
                            if ((int)(File.GetAttributes(f) & FileAttributes.ReadOnly) != 0)
                            {
                                setReadOnly = new List<string>();
                                setReadOnly.Add(f);
                            }

                            MaybeRevertReplaced(f);
                        }
                    }
                }

                if (setReadOnly != null)
                    foreach (string f in setReadOnly)
                        File.SetAttributes(f, File.GetAttributes(f) | FileAttributes.ReadOnly);
            }

            return true;
#endif
        }

        public bool SafeWcCopyFixup(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
                throw new ArgumentNullException("fromPath");
            else if (string.IsNullOrEmpty(toPath))
                throw new ArgumentNullException("toPath");

            if (!File.Exists(fromPath))
                throw new InvalidOperationException();

            throw new NotImplementedException();
#if false
            bool ok;
            bool setReadOnly = false;
            using (MarkIgnoreFile(toPath))
            {
                using (MoveAway(toPath, true))
                {
                    string toDir = GitTools.GetNormalizedDirectoryName(toPath);

                    EnsureAdded(toDir);

                    SvnCopyArgs ca = new SvnCopyArgs();
                    ca.AlwaysCopyAsChild = false;
                    ca.CreateParents = false; // We just did that ourselves. Use Svn for this?
                    ca.ThrowOnError = false;

                    ok = _svnClient.Copy(fromPath, toPath, ca);

                    if (ok && File.Exists(toPath))
                    {
                        setReadOnly = (int)(File.GetAttributes(toPath) & FileAttributes.ReadOnly) != 0;
                    }

                    MaybeRevertReplaced(toPath);
                }

                if (setReadOnly)
                    File.SetAttributes(toPath, File.GetAttributes(toPath) | FileAttributes.ReadOnly);
            }

            return ok;
#endif
        }

        private void EnsureAdded(string toDir)
        {
            throw new NotImplementedException();
#if false
            if (!GitTools.IsManagedPath(toDir))
            {
                SvnAddArgs aa = new SvnAddArgs();
                aa.Depth = SvnDepth.Empty;
                aa.AddParents = true;
                aa.Force = true;
                aa.ThrowOnError = false;

                if (!_svnClient.Add(toDir, aa))
                    throw new InvalidOperationException();
            }

            Debug.Assert(GitTools.IsManagedPath(toDir));
#endif
        }

        public bool WcDelete(string path)
        {
            throw new NotImplementedException();
#if false
            SvnDeleteArgs da = new SvnDeleteArgs();
            da.ThrowOnError = false;
            da.Force = true;

            return _svnClient.Delete(path, da);
#endif
        }

        internal bool SafeWcMoveFixup(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
                throw new ArgumentNullException("fromPath");
            else if (string.IsNullOrEmpty(toPath))
                throw new ArgumentNullException("toPath");

            bool setReadOnly = false;
            bool ok = true;

            using (HandsOff(fromPath))
            using (HandsOff(toPath))
            using (MarkIgnoreFile(fromPath))
            using (MarkIgnoreFile(toPath))
            {
                using (TempFile(fromPath, toPath))
                using (MoveAway(toPath, true))
                {
                    string toDir = GitTools.GetNormalizedDirectoryName(toPath);

                    Debug.Assert(GitTools.IsManagedPath(toDir));

                    GitMoveArgs ma = new GitMoveArgs();
                    ma.ThrowOnError = false;
                    ma.Force = true;

                    ok = _client.Move(fromPath, toPath, ma);

                    if (ok)
                    {
                        setReadOnly = (File.GetAttributes(toPath) & FileAttributes.ReadOnly) != (FileAttributes)0;
                    }

                    MaybeRevertReplaced(toPath);
                }

                if (setReadOnly)
                    File.SetAttributes(toPath, File.GetAttributes(toPath) | FileAttributes.ReadOnly);
            }

            return ok;
        }

        internal void SafeWcDirectoryCopyFixUp(string oldDir, string newDir, bool safeRename)
        {
            if (string.IsNullOrEmpty(oldDir))
                throw new ArgumentNullException("oldDir");
            else if (string.IsNullOrEmpty(newDir))
                throw new ArgumentNullException("newDir");

            using (HandsOffRecursive(newDir))
            using (MarkIgnoreRecursive(newDir))
            {
                RetriedRename(newDir, oldDir);

                if (safeRename)
                    RecursiveCopyWc(oldDir, newDir);
                else
                    RecursiveCopyNotVersioned(oldDir, newDir, false);
            }
        }

        private IDisposable MarkIgnoreRecursive(string newDir)
        {
            IVisualGitOpenDocumentTracker dt = GetService<IVisualGitOpenDocumentTracker>();
            IVsFileChangeEx change = GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));

            if (dt == null || change == null)
                return null;

            ICollection<string> files = dt.GetDocumentsBelow(newDir);

            if (files == null || files.Count == 0)
                return null;

            foreach (string file in files)
            {
                Marshal.ThrowExceptionForHR(change.IgnoreFile(0, file, 1));
            }

            return new DelegateRunner(
                delegate()
                {
                    foreach (string file in files)
                    {
                        change.SyncFile(file);
                        change.IgnoreFile(0, file, 0);
                    }
                });
        }

        private IDisposable HandsOffRecursive(string newDir)
        {
            IVisualGitOpenDocumentTracker dt = GetService<IVisualGitOpenDocumentTracker>();
            IVsTrackProjectDocuments3 tracker = GetService<IVsTrackProjectDocuments3>(typeof(SVsTrackProjectDocuments));

            if (dt == null || tracker == null)
                return null;

            ICollection<string> files = dt.GetDocumentsBelow(newDir);

            if (files == null || files.Count == 0)
                return null;

            string[] fileArray = new List<string>(files).ToArray();

            Marshal.ThrowExceptionForHR(tracker.HandsOffFiles(
                (uint)__HANDSOFFMODE.HANDSOFFMODE_DeleteAccess,
                fileArray.Length, fileArray));

            return new DelegateRunner(
                delegate()
                {
                    tracker.HandsOnFiles(fileArray.Length, fileArray);
                });
        }

        /// <summary>
        /// Creates an unversioned copy of <paramref name="from"/> in <paramref name="to"/>. (Recursive copy skipping administrative directories)
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        private void RecursiveCopyNotVersioned(string from, string to, bool force)
        {
            DirectoryInfo fromDir = new DirectoryInfo(from);
            DirectoryInfo toDir = new DirectoryInfo(to);

            if (!fromDir.Exists)
                return;

            if (!toDir.Exists)
                toDir.Create();

            foreach (FileInfo file in fromDir.GetFiles())
            {
                string toFile = Path.Combine(to, file.Name);
                if (force)
                {
                    // toFile might be read only
                    FileInfo toInfo = new FileInfo(toFile);

                    if (toInfo.Exists && (toInfo.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        toInfo.Attributes &= ~FileAttributes.ReadOnly;
                    }
                }

                File.Copy(file.FullName, toFile, force);
            }

            foreach (DirectoryInfo dir in fromDir.GetDirectories())
            {
                if (!string.Equals(dir.Name, GitConstants.AdministrativeDirectoryName, StringComparison.OrdinalIgnoreCase))
                    RecursiveCopyNotVersioned(dir.FullName, Path.Combine(to, dir.Name), force);
            }
        }

        private void RecursiveCopyWc(string from, string to)
        {
            throw new NotImplementedException();
#if false
            // First, copy the way Git likes it
            SvnCopyArgs ca = new SvnCopyArgs();
            ca.AlwaysCopyAsChild = false;
            ca.CreateParents = false;
            ca.ThrowOnError = false;
            _svnClient.Copy(from, to, ca);

            // Now copy everything unversioned from our local backup back
            // into the new workingcopy, to be 100% sure VS finds what it expects

            RecursiveCopyNotVersioned(from, to, true);
#endif
        }

        public bool SafeDeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            GitDeleteArgs da = new GitDeleteArgs();
            da.Force = true;
            da.KeepLocal = false;
            da.ThrowOnError = false;
            da.KeepLocal = !File.Exists(path); // This will stop the error if the file was already deleted

            return _client.Delete(path, da);
        }

        public bool IsUnversioned(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            GitStatusEventArgs status = SafeGetEntry(name);

            if (status == null)
                return true;

            switch (status.WorkingCopyInfo.Schedule)
            {
                case GitSchedule.Delete:
                    return true; // The item was already deleted
                default:
                    return false;
            }
        }

        sealed class DelegateRunner : IDisposable
        {
            VisualGitAction _runner;
            public DelegateRunner(VisualGitAction runner)
            {
                if (runner == null)
                    throw new ArgumentNullException("runner");
                _runner = runner;
            }

            public void Dispose()
            {
                VisualGitAction runner = _runner;
                _runner = null;
                runner();
            }
        }

        public IDisposable HandsOff(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            IVsTrackProjectDocuments3 tracker = GetService<IVsTrackProjectDocuments3>(typeof(SVsTrackProjectDocuments));

            if (tracker != null)
            {
                string[] paths = new string[] { path, null };

                int hr = tracker.HandsOffFiles(
                    (uint)__HANDSOFFMODE.HANDSOFFMODE_DeleteAccess,
                    1,
                    paths);


                Marshal.ThrowExceptionForHR(hr);

                return new DelegateRunner(
                    delegate()
                    {
                        tracker.HandsOnFiles(1, paths);
                    });
            }
            else
                return null;
        }

        public IDisposable MarkIgnoreFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            IVsFileChangeEx change = GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));

            if (change != null)
            {
                Marshal.ThrowExceptionForHR(change.IgnoreFile(0, path, 1));

                return new DelegateRunner(
                    delegate()
                    {
                        change.SyncFile(path);
                        change.IgnoreFile(0, path, 0);
                    });
            }
            else
                return null;
        }

        public IDisposable MarkIgnoreFiles(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            List<IDisposable> disps = new List<IDisposable>();

            foreach (string path in paths)
            {
                IDisposable d = MarkIgnoreFile(path);

                if (d != null)
                    disps.Add(d);
            }

            if (disps.Count > 0)
                return new DelegateRunner(
                    delegate
                    {
                        foreach (IDisposable d in disps)
                        {
                            d.Dispose();
                        }
                    });
            else
                return null;
        }

        public IDisposable TempRevertForCopy(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            List<string> deletePaths = new List<string>();
            throw new NotImplementedException();
#if false
            SvnRevertArgs ra = new SvnRevertArgs();
            ra.ThrowOnError = false;

            foreach (string p in paths)
            {
                if (_svnClient.Revert(p, ra))
                    deletePaths.Add(p);
            }

            if (deletePaths.Count > 0)
            {
                return new DelegateRunner(
                    delegate
                    {
                        SvnDeleteArgs da = new SvnDeleteArgs();
                        da.ThrowOnError = false;
                        foreach (string p in deletePaths)
                        {
                            GitItem item = StatusCache[p];

                            item.MarkDirty();

                            if (item.Exists && !item.IsDeleteScheduled)
                                _svnClient.Delete(p, da);
                        }
                    });
            }
            else
                return null;
#endif
        }

        public IDisposable MoveAway(string path, bool touch)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            path = GitTools.GetNormalizedFullPath(path);
            bool isFile = true;

            if (!File.Exists(path))
            {
                if (Directory.Exists(path))
                    isFile = false;
                else
                    throw new InvalidOperationException();
            }

            FileAttributes attrs = FileAttributes.Normal;
            if (isFile)
            {
                attrs = File.GetAttributes(path);
                File.SetAttributes(path, FileAttributes.Normal);
            }

            string tmp;
            int n = 0;
            do
            {
                tmp = path + string.Format(".VisualGit.{0}.tmp", n++);
            }
            while (File.Exists(tmp) || Directory.Exists(tmp));

            RetriedRename(path, tmp);

            if (isFile)
                File.SetAttributes(tmp, FileAttributes.ReadOnly);

            return new DelegateRunner(
                delegate()
                {
                    if (isFile && File.Exists(path))
                    {
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }
                    else if (!isFile && Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }

                    if (isFile)
                        File.SetAttributes(tmp, FileAttributes.Normal);

                    RetriedRename(tmp, path);

                    if (isFile)
                    {
                        if (touch)
                            File.SetLastWriteTime(path, DateTime.Now);

                        File.SetAttributes(path, attrs);
                    }
                });
        }

        public IDisposable MoveAwayFiles(IEnumerable<string> paths, bool touch)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            List<IDisposable> disps = new List<IDisposable>();

            foreach (string path in paths)
            {
                IDisposable d = MoveAway(path, touch);

                if (d != null)
                    disps.Add(d);
            }

            if (disps.Count > 0)
                return new DelegateRunner(
                    delegate
                    {
                        foreach (IDisposable d in disps)
                        {
                            d.Dispose();
                        }
                    });
            else
                return null;
        }

        /// <summary>
        /// Performs a few attempts on renaming a directory and only fails if all fail
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tmp"></param>
        internal static void RetriedRename(string path, string tmp)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (string.IsNullOrEmpty(tmp))
                throw new ArgumentNullException("tmp");

            const int retryCount = 5;
            for (int i = 0; i < retryCount; i++)
            {
                // Don't throw an exception on the common case the file is locked
                // The project just renamed the file so a virusscanner or directory scanner (Tortoise, VS itself)
                // Will now look at the file
                if (!NativeMethods.MoveFile(path, tmp))
                {
                    if (i == retryCount - 1)
                    {
                        // Throw an exception after 4 attempts

                        if (Directory.Exists(path))
                            Directory.Move(path, tmp);
                        else
                            File.Move(path, tmp);

                    }
                    else
                        System.Threading.Thread.Sleep(50 * (i + 1));
                }
                else
                    break;
            }
        }

        public IDisposable TempFile(string path, string contentFrom)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (string.IsNullOrEmpty(contentFrom))
                throw new ArgumentNullException("contentFrom");

            IDisposable moveAway = null;

            if (File.Exists(path))
                moveAway = MoveAway(path, false);
            else if (!File.Exists(contentFrom))
                throw new InvalidOperationException("Source does not exist");

            IDisposable directory = TempDirectory(Path.GetDirectoryName(path));

            File.Copy(contentFrom, path);

            return new DelegateRunner(
                delegate()
                {
                    if (File.Exists(path))
                    {
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }

                    if (moveAway != null)
                        moveAway.Dispose();
                    if (directory != null)
                        directory.Dispose();
                });
        }

        public IDisposable TempDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (Directory.Exists(path))
                return null;

            IDisposable parentDirectory = TempDirectory(Path.GetDirectoryName(path));

            Directory.CreateDirectory(path);

            return new DelegateRunner(
                delegate()
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path);

                    if (parentDirectory != null)
                        parentDirectory.Dispose();
                });
        }

        /// <summary>
        /// Check if adding the path might succeed
        /// </summary>
        /// <param name="path"></param>
        /// <returns><c>false</c> when adding the file will fail, <c>true</c> if it could succeed</returns>
        public bool CouldAdd(string path, GitNodeKind nodeKind)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            GitItem item = StatusCache[path];
            string file = item.Name;

            if (!item.Exists || item.IsVersioned)
                return true; // Item already exists.. Fast

            GitItem parent = item.Parent;

            if (BelowAdminDir(item))
                return false;

            if (item.IsFile && parent != null && !parent.IsVersioned)
                return true; // Not in a versioned directory -> Fast out

            // Item does exist; check casing
            string parentDir = GitTools.GetNormalizedDirectoryName(path);
            GitStatusArgs wa = new GitStatusArgs();
            wa.ThrowOnError = false;
            wa.ThrowOnCancel = false;
            wa.Depth = GitDepth.Files;

            bool ok = true;

            using (GitClient client = GetService<IGitClientPool>().GetNoUIClient())
            {
                client.Status(parentDir, wa,
                delegate(object sender, GitStatusEventArgs e)
                {
                    if (string.Equals(e.FullPath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(e.Name, file, StringComparison.Ordinal))
                        {
                            ok = false; // Casing issue
                        }
                    }
                });
            }

            return ok;
        }

        string _adminDir;
        private bool BelowAdminDir(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (_adminDir == null)
            {
                // Caching in this instance should be safe
                _adminDir = '\\' + GitConstants.AdministrativeDirectoryName + '\\';
            }

            if (string.Equals(item.Name, GitConstants.AdministrativeDirectoryName))
                return true;

            return item.FullPath.IndexOf(_adminDir, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool BelowAdminDir(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            return BelowAdminDir(StatusCache[path]);
        }

        /// <summary>
        /// Removes all administrative directories from the specified path recursively
        /// </summary>
        /// <param name="directory"></param>
        public void UnversionRecursive(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("directory");

            DirectoryInfo dir = new DirectoryInfo(directory);

            if (!dir.Exists)
                return;

            foreach (DirectoryInfo subDir in dir.GetDirectories(GitConstants.AdministrativeDirectoryName, SearchOption.AllDirectories))
            {
                RecursiveDelete(subDir);
            }
        }

        public void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Exists)
                RecursiveDelete(dir);
        }

        internal string MakeBackup(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath");

            DirectoryInfo source = new DirectoryInfo(fullPath);
            if (source.Exists)
            {
                string tmp;
                int n = 0;

                do
                {
                    tmp = string.Format("{0}.tmp{1}", fullPath, n++);
                }
                while (Directory.Exists(tmp));

                DirectoryInfo dest = Directory.CreateDirectory(tmp);

                RecursiveCopy(source, dest);

                return tmp;
            }
            else
                return null;
        }

        private void RecursiveCopy(DirectoryInfo source, DirectoryInfo destination)
        {
            foreach (FileInfo sourceFile in source.GetFiles())
            {
                FileInfo destFile = sourceFile.CopyTo(Path.Combine(destination.FullName, sourceFile.Name));
                destFile.Attributes = sourceFile.Attributes;
            }

            foreach (DirectoryInfo subDirSource in source.GetDirectories())
            {
                DirectoryInfo subDirDestination = destination.CreateSubdirectory(subDirSource.Name);
                subDirDestination.Attributes = subDirSource.Attributes;
                RecursiveCopy(subDirSource, subDirDestination);
            }
        }

        private void RecursiveDelete(DirectoryInfo dir)
        {
            if (dir == null)
                throw new ArgumentNullException("dir");

            if (!dir.Exists)
                return;

            foreach (DirectoryInfo sd in dir.GetDirectories())
            {
                RecursiveDelete(sd);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            dir.Attributes = FileAttributes.Normal; // .Net fixes up FileAttributes.Directory
            dir.Delete();
        }

        static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool MoveFile(string src, string dst);



        }
    }
}
