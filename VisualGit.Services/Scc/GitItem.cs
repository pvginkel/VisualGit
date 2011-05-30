using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using SharpSvn;

using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.Selection;

namespace VisualGit
{
    public interface IGitItemUpdate
    {
        void RefreshTo(VisualGitStatus status);
        void RefreshTo(NoSccStatus status, SvnNodeKind nodeKind);
        void RefreshTo(GitItem lead);
        void TickItem();
        void UntickItem();
        bool IsItemTicked();
        bool ShouldRefresh();
        bool IsStatusClean();

        bool ShouldClean();

        void SetState(GitItemState set, GitItemState unset);
        void SetDirty(GitItemState dirty);
        bool TryGetState(GitItemState get, out GitItemState value);
    }

    /// <summary>
    /// Represents a version controlled path on disk, caching its status
    /// </summary>
    [DebuggerDisplay("Path={FullPath}")]
    public sealed partial class GitItem : IGitItemUpdate, IGitWcReference, IEquatable<GitItem>
    {
        readonly IFileStatusCache _context;
        readonly string _fullPath;

        enum XBool : sbyte
        {
            None = 0, // The three fastest values to check for most CPU's
            True = -1,
            False = 1
        }

        VisualGitStatus _status;
        bool _enqueued;

        static readonly Queue<GitItem> _stateChanged = new Queue<GitItem>();
        static bool _scheduled;

        IGitWcReference _workingCopy;
        XBool _statusDirty; // updating, dirty, dirty 
        bool _ticked;
        int _cookie;
        DateTime _modified;

        public GitItem(IFileStatusCache context, string fullPath, VisualGitStatus status)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath");
            else if (status == null)
                throw new ArgumentNullException("status");

            _context = context;
            _fullPath = fullPath;
            _status = status;

            _enqueued = true;
            RefreshTo(status);
            _enqueued = false;
        }

        public GitItem(IFileStatusCache context, string fullPath, NoSccStatus status, SvnNodeKind nodeKind)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath");

            _context = context;
            _fullPath = fullPath;
            _enqueued = true;
            RefreshTo(status, nodeKind);
            _enqueued = false;
        }

        void InitializeFromKind(SvnNodeKind nodeKind)
        {
            switch (nodeKind) // We assume the caller checked this for us
            {
                case SvnNodeKind.File:
                    SetState(GitItemState.IsDiskFile | GitItemState.Exists, GitItemState.IsDiskFolder);
                    break;
                case SvnNodeKind.Directory:
                    SetState(GitItemState.IsDiskFolder | GitItemState.Exists, GitItemState.IsDiskFile | GitItemState.ReadOnly);
                    break;
            }
        }

        IFileStatusCache StatusCache
        {
            [DebuggerStepThrough]
            get { return _context; }
        }

        void RefreshTo(NoSccStatus status, SvnNodeKind nodeKind)
        {
            _cookie = NextCookie();
            _statusDirty = XBool.False;

            GitItemState set = GitItemState.None;
            GitItemState unset = GitItemState.Modified | GitItemState.Added | GitItemState.HasCopyOrigin
                | GitItemState.Deleted | GitItemState.ContentConflicted | GitItemState.Ignored
                | GitItemState.Obstructed | GitItemState.Replaced | GitItemState.Versioned
                | GitItemState.GitDirty | GitItemState.Obstructed | GitItemState.IsNested
                | GitItemState.HasCopyOrigin | GitItemState.TreeConflicted;

            switch (status)
            {
                case NoSccStatus.NotExisting:
                    SetState(set, GitItemState.Exists | GitItemState.ReadOnly | GitItemState.IsDiskFile | GitItemState.IsDiskFolder | GitItemState.Versionable | unset);
                    _status = VisualGitStatus.NotExisting;
                    break;

                case NoSccStatus.NotVersioned:
                    SetState(GitItemState.Exists | set, GitItemState.None | unset);
                    _status = VisualGitStatus.NotVersioned;
                    break;
                case NoSccStatus.Unknown:
                default:
                    SetDirty(set | unset);
                    _statusDirty = XBool.True;
                    break;
            }

            InitializeFromKind(nodeKind);
        }

        void IGitItemUpdate.RefreshTo(NoSccStatus status, SvnNodeKind nodeKind)
        {
            Debug.Assert(status == NoSccStatus.NotExisting || status == NoSccStatus.NotVersioned);
            _ticked = false;
            RefreshTo(status, nodeKind);
        }

        void RefreshTo(VisualGitStatus status)
        {
            if (status == null)
                throw new ArgumentNullException("status");

            if (status.State == SvnStatus.External)
            {
                // When iterating the status of an external in it's parent directory
                // We get an external status and no really usefull information

                SetState(GitItemState.Exists | GitItemState.Versionable | GitItemState.IsDiskFolder,
                            GitItemState.IsDiskFile | GitItemState.ReadOnly | GitItemState.IsTextFile);

                if (_statusDirty != XBool.False)
                    _statusDirty = XBool.True; // Walk the path itself to get the data you want

                return;
            }
            else if (MightBeNestedWorkingCopy(status) && IsDirectory)
            {
                // A not versioned directory might be a working copy by itself!

                if (_statusDirty == XBool.False)
                    return; // No need to remove valid cache entries

                if (SvnTools.IsManagedPath(FullPath))
                {
                    _statusDirty = XBool.True; // Walk the path itself to get the data

                    // Extract useful information we got anyway

                    SetState(GitItemState.Exists | GitItemState.Versionable | GitItemState.IsDiskFolder,
                                GitItemState.IsDiskFile | GitItemState.ReadOnly | GitItemState.IsTextFile);

                    return;
                }
                // Fall through
            }

            _cookie = NextCookie();
            _statusDirty = XBool.False;
            _status = status;

            const GitItemState unset = GitItemState.Modified | GitItemState.Added |
                GitItemState.HasCopyOrigin | GitItemState.Deleted | GitItemState.ContentConflicted |
                GitItemState.Ignored | GitItemState.Obstructed | GitItemState.Replaced;

            const GitItemState managed = GitItemState.Versioned;


            // Let's assume status is more recent than our internal property cache
            // Set all caching properties we can

            bool svnDirty = true;
            bool exists = true;
            bool provideDiskInfo = true;
            switch (status.State)
            {
                case SvnStatus.None:
                    SetState(GitItemState.None, managed | unset);
                    svnDirty = false;
                    exists = false;
                    provideDiskInfo = false;
                    break;
                case SvnStatus.NotVersioned:
                    // Node exists but is not managed by us in this directory
                    // (Might be from an other location as in the nested case)
                    SetState(GitItemState.None, unset | managed);
                    svnDirty = false;
                    break;
                case SvnStatus.Ignored:
                    // Node exists but is not managed by us in this directory
                    // (Might be from an other location as in the nested case)
                    SetState(GitItemState.Ignored, unset | managed);
                    svnDirty = false;
                    break;
                case SvnStatus.Modified:
                    SetState(managed | GitItemState.Modified, unset);
                    break;
                case SvnStatus.Added:
                    if (status.IsCopied)
                        SetState(managed | GitItemState.Added | GitItemState.HasCopyOrigin, unset);
                    else
                        SetState(managed | GitItemState.Added, unset);
                    break;
                case SvnStatus.Replaced:
                    if (status.IsCopied)
                        SetState(managed | GitItemState.Replaced | GitItemState.HasCopyOrigin, unset);
                    else
                        SetState(managed | GitItemState.Replaced, unset);
                    break;
                case SvnStatus.Conflicted:
                    SetState(managed | GitItemState.ContentConflicted, unset);
                    break;
                case SvnStatus.Obstructed: // node exists but is of the wrong type
                    SetState(GitItemState.None, managed | unset);
                    provideDiskInfo = false; // Info is wrong
                    break;
                case SvnStatus.Missing:
                    exists = false;
                    provideDiskInfo = false; // Info is wrong
                    SetState(managed, unset);
                    break;
                case SvnStatus.Deleted:
                    SetState(managed | GitItemState.Deleted, unset);
                    exists = false;
                    provideDiskInfo = false; // File/folder might still exist
                    break;
                case SvnStatus.External:
                    // Should be handled above
                    throw new InvalidOperationException();
                case SvnStatus.Incomplete:
                    SetState(managed, unset);
                    break;
                default:
                    Trace.WriteLine(string.Format("Ignoring undefined status {0} in GitItem.Refresh()", status.State));
                    provideDiskInfo = false; // Can't trust an unknown status
                    goto case SvnStatus.Normal;
                case SvnStatus.Normal:
                    SetState(managed | GitItemState.Exists, unset);
                    svnDirty = false;
                    break;
            }

            if (exists)
                SetState(GitItemState.Versionable, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.Versionable);

            if (status.HasTreeConflict)
                SetState(GitItemState.TreeConflicted, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.TreeConflicted);

            if (svnDirty)
                SetState(GitItemState.GitDirty, GitItemState.None);
            else
                SetState(GitItemState.None, GitItemState.GitDirty);

            if (provideDiskInfo)
            {
                if (exists) // Behaviour must match updating from UpdateAttributeInfo()
                    switch (status.NodeKind)
                    {
                        case SvnNodeKind.Directory:
                            SetState(GitItemState.IsDiskFolder | GitItemState.Exists, GitItemState.ReadOnly| GitItemState.IsTextFile | GitItemState.IsDiskFile);
                            break;
                        case SvnNodeKind.File:
                            SetState(GitItemState.IsDiskFile | GitItemState.Exists, GitItemState.IsDiskFolder);
                            break;
                    }
                else
                    SetState(GitItemState.None, GitItemState.Exists);
            }
        }

        static bool MightBeNestedWorkingCopy(VisualGitStatus status)
        {
            switch (status.State)
            {
                case SvnStatus.NotVersioned:
                case SvnStatus.Ignored:
                    return true;

                // TODO: Handle obstructed and tree conflicts!
                // Obstructed can be directory on file location
                // Tree conflict can apply on non versioned item
                default:
                    return false;
            }
        }

        void ScheduleUpdateNotify()
        {
            if (_scheduled)
                return;

            IVisualGitCommandService cs = _context.GetService<IVisualGitCommandService>();

            if (cs != null)
                cs.PostTickCommand(ref _scheduled, VisualGitCommand.TickRefreshGitItems);
        }

        void IGitItemUpdate.RefreshTo(VisualGitStatus status)
        {
            _ticked = false;
            RefreshTo(status);
        }

        /// <summary>
        /// Copies all information from other.
        /// </summary>
        /// <param name="other"></param>
        /// <remarks>When this method is called the other item will eventually replace this item</remarks>
        void IGitItemUpdate.RefreshTo(GitItem lead)
        {
            if (lead == null)
                throw new ArgumentNullException("lead");

            _status = lead._status;
            _statusDirty = lead._statusDirty;

            GitItemState current = lead._currentState;
            GitItemState valid = lead._validState;

            SetState(current & valid, (~current) & valid);
            _ticked = false;
            _modified = lead._modified;
            _cookie = NextCookie(); // Status 100% the same, but changed... Cookies are free ;)
        }

        public void MarkDirty()
        {
            Debug.Assert(_statusDirty != XBool.None, "MarkDirty called while updating status");

            _statusDirty = XBool.True;

            _validState = GitItemState.None;
            _cookie = NextCookie();
            _workingCopy = null;
            _modified = new DateTime();
        }

        bool IGitItemUpdate.IsStatusClean()
        {
            return _statusDirty == XBool.False;
        }

        /// <summary>
        /// 
        /// </summary>
        void IGitItemUpdate.TickItem()
        {
            _ticked = true; // Will be updated soon
        }

        /// <summary>
        /// 
        /// </summary>
        void IGitItemUpdate.UntickItem()
        {
            _ticked = false;
        }

        public bool ShouldClean()
        {
            return _ticked || (_statusDirty == XBool.False && _status == VisualGitStatus.NotExisting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IGitItemUpdate.IsItemTicked()
        {
            return _ticked;
        }

        bool IGitItemUpdate.ShouldRefresh()
        {
            return _ticked || _statusDirty != XBool.False;
        }

        /// <summary>
        /// Gets a value which is incremented everytime the status was changed.
        /// </summary>
        /// <remarks>The cookie is provided globally over all <see cref="GitItem"/> instances.  External users can be sure
        /// the status is 100% the same if the cookie did not change</remarks>
        public int ChangeCookie
        {
            get { return _cookie; }
        }

        /// <summary>
        /// Gets the (cached) modification time of the file/directory
        /// </summary>
        /// <value>The modified.</value>
        public DateTime Modified
        {
            get
            {
                if (_modified.Ticks == 0 && Exists)
                {
                    try
                    {
                        _modified = File.GetLastWriteTimeUtc(FullPath);
                    }
                    catch { }
                }
                return _modified;
            }
        }

        /// <summary>
        /// Gets the full normalized path of the item
        /// </summary>
        public string FullPath
        {
            [DebuggerStepThrough]
            get { return _fullPath; }
        }

        string _name;
        /// <summary>
        /// Gets the filename (including extension) of the item
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name ?? (_name = Path.GetFileName(FullPath)); }
        }

        /// <summary>
        /// Gets the SVN status of the item; retrieves a placeholder if the status is unknown
        /// </summary>
        public VisualGitStatus Status
        {
            get
            {
                EnsureClean();

                return _status;
            }
        }

        /// <summary>
        /// Gets the node kind of the file in Git
        /// </summary>
        public SvnNodeKind NodeKind
        {
            get { return Status.NodeKind; }
        }

        bool TryGetState(GitItemState mask, out GitItemState result)
        {
            if ((mask & _validState) != mask)
            {
                result = GitItemState.None;
                return false;
            }

            result = _currentState & mask;
            return true;
        }

        /// <summary>
        /// Gets a boolean indicating whether the item (on disk) is a file
        /// </summary>
        /// <remarks>Use <see cref="Status"/>.<see cref="VisualGitStatus.SvnNodeKind"/> to retrieve the svn type</remarks>
        public bool IsFile
        {
            get { return GetState(GitItemState.IsDiskFile) == GitItemState.IsDiskFile; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the item (on disk) is a directory
        /// </summary>
        /// <remarks>Use <see cref="Status"/>.<see cref="VisualGitStatus.SvnNodeKind"/> to retrieve the svn type</remarks>
        public bool IsDirectory
        {
            get { return GetState(GitItemState.IsDiskFolder) == GitItemState.IsDiskFolder; }
        }


        /// <summary>
        /// Gets a boolean indicating wether a copy of this file has history
        /// </summary>
        public bool HasCopyableHistory
        {
            get
            {
                if (!IsVersioned)
                    return false;

                if (GetState(GitItemState.Added | GitItemState.Replaced) != 0)
                    return GetState(GitItemState.HasCopyOrigin) != 0;
                else
                    return true;
            }
        }

        void RefreshStatus()
        {
            _statusDirty = XBool.None;
            IFileStatusCache statusCache = StatusCache;

            try
            {
                statusCache.RefreshItem(this, IsFile ? SvnNodeKind.File : SvnNodeKind.Directory); // We can check this less expensive than the statuscache!
            }
            finally
            {
                Debug.Assert(_statusDirty == XBool.False, "No longer dirty after refresh", string.Format("Path = {0}", FullPath));
                _statusDirty = XBool.False;
            }
        }

        /// <summary>
        /// Is this item versioned?
        /// </summary>
        public bool IsVersioned
        {
            get { return 0 != GetState(GitItemState.Versioned); }
        }

        static bool GetIsVersioned(VisualGitStatus status)
        {
            switch (status.State)
            {
                case SvnStatus.Added:
                case SvnStatus.Conflicted:
                case SvnStatus.Merged:
                case SvnStatus.Modified:
                case SvnStatus.Normal:
                case SvnStatus.Replaced:
                case SvnStatus.Deleted:
                case SvnStatus.Incomplete:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is this resource modified; implies the item is versioned
        /// </summary>
        public bool IsModified
        {
            get
            {
                return GetState(GitItemState.GitDirty) != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is diff available.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is diff available; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocalDiffAvailable
        {
            get
            {
                if (!IsVersioned)
                    return false;
                if (!IsModified)
                    return IsDocumentDirty;

                switch (Status.State)
                {
                    case SvnStatus.Normal:
                        // Probably property modified
                        return IsDocumentDirty;
                    case SvnStatus.Added:
                    case SvnStatus.Replaced:
                        return HasCopyableHistory;
                    case SvnStatus.Deleted:
                        // To be replaced
                        return Exists;
                    case SvnStatus.NotVersioned:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// Whether the item is potentially versionable.
        /// </summary>
        public bool IsVersionable
        {
            get { return GetState(GitItemState.Versionable) != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the item is (inside) the administrative area.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is administrative area; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdministrativeArea
        {
            get { return GetState(GitItemState.IsAdministrativeArea) != 0; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> specifies a readonly file
        /// </summary>
        public bool IsReadOnly
        {
            get { return GetState(GitItemState.ReadOnly) != 0; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> exists on disk
        /// </summary>
        public bool Exists
        {
            get { return GetState(GitItemState.Exists) != 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAdded
        {
            get { return GetState(GitItemState.Added) != 0; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> is obstructed by an invalid node
        /// </summary>
        public bool IsObstructed
        {
            get { return GetState(GitItemState.Obstructed) != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a managed binary file.
        /// </summary>
        public bool IsTextFile
        {
            get { return GetState(GitItemState.IsTextFile) != 0; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> is in conflict state
        /// </summary>
        public bool IsConflicted
        {
            get { return 0 != GetState(GitItemState.ContentConflicted | GitItemState.TreeConflicted); }
        }

        /// <summary>
        /// Gets a value indicating whether this node is tree conflicted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is tree conflicted; otherwise, <c>false</c>.
        /// </value>
        public bool IsTreeConflicted
        {
            get { return 0 != GetState(GitItemState.TreeConflicted); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is casing conflicted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is casing conflicted; otherwise, <c>false</c>.
        /// </value>
        public bool IsCasingConflicted
        {
            get { return IsVersioned && Status.State == SvnStatus.Missing && Status.NodeKind == SvnNodeKind.File && IsFile && Exists; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> is scheduled for delete
        /// </summary>
        public bool IsDeleteScheduled
        {
            get { return 0 != GetState(GitItemState.Deleted); }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> is scheduled for replacement
        /// </summary>
        public bool IsReplaced
        {
            get { return 0 != GetState(GitItemState.Replaced); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="GitItem"/> is in one of the projects in the solution
        /// </summary>
        /// <value><c>true</c> if the file is in one of the projects of the solution; otherwise, <c>false</c>.</value>
        public bool InSolution
        {
            get { return GetState(GitItemState.InSolution) != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this file is dirty in an open editor
        /// </summary>
        public bool IsDocumentDirty
        {
            get { return GetState(GitItemState.DocumentDirty) != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this node is a nested working copy.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is nested working copy; otherwise, <c>false</c>.
        /// </value>
        public bool IsNestedWorkingCopy
        {
            get { return GetState(GitItemState.IsNested) != 0; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the <see cref="GitItem"/> is explicitly ignored
        /// </summary>
        public bool IsIgnored
        {
            get
            {
                if (GetState(GitItemState.Ignored) != 0)
                    return true;
                else if (IsVersioned)
                    return false;
                else if (!Exists)
                    return false;

                GitItem parent = Parent;
                if (parent != null)
                    return parent.IsIgnored;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets the full path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// Retrieves a collection of paths from all provided SvnItems.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static ICollection<string> GetPaths(IEnumerable<GitItem> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            List<String> paths = new List<string>();
            foreach (GitItem item in items)
            {
                Debug.Assert(item != null, "GitItem should not be null");

                if (item != null)
                {
                    paths.Add(item.FullPath);
                }
            }

            return paths;
        }

        /// <summary>
        /// Gets the common parent of a list of SvnItems
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static GitItem GetCommonParent(IEnumerable<GitItem> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            GitItem parent = null;

            foreach (GitItem i in items)
            {
                string p = i.FullPath;

                if (parent == null)
                {
                    parent = i;
                    continue;
                }

                GitItem j;
                if (i.FullPath.Length < parent.FullPath.Length)
                {
                    j = parent;
                    parent = i;
                }
                else
                {
                    j = i;
                }

                while (parent != null && !j.IsBelowPath(parent.FullPath))
                    parent = parent.Parent;

                if (j == null)
                    return null;
            }

            return parent;
        }

        void EnsureClean()
        {
            Debug.Assert(_statusDirty != XBool.None, "Recursive refresh call");

            if (_statusDirty == XBool.True)
                this.RefreshStatus();
        }

        public void Dispose()
        {
            // TODO: Mark item as no longer valid
        }

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <value>The directory.</value>
        public string Directory
        {
            get { return SvnTools.GetNormalizedDirectoryName(FullPath); }
        }

        /// <summary>
        /// Gets the extension of the item
        /// </summary>
        /// <value>The extension.</value>
        /// <remarks>By definition directories do not have an extension</remarks>
        public string Extension
        {
            get { return IsDirectory ? "" : Path.GetExtension(Name); }
        }

        /// <summary>
        /// Gets the name of the file without its extension.
        /// </summary>
        /// <value>The name without extension.</value>
        /// <remarks>By definition directories do not have an extension</remarks>
        public string NameWithoutExtension
        {
            get { return IsDirectory ? Name : Path.GetFileNameWithoutExtension(Name); }
        }

        /// <summary>
        /// Gets the <see cref="GitItem"/> of this instances parent (the directory it is in)
        /// </summary>
        /// <value>The parent directory or <c>null</c> if this instance is the root directory 
        /// or the cache can not be contacted</value>
        public GitItem Parent
        {
            get
            {
                string parentDir = Directory;

                if (string.IsNullOrEmpty(parentDir))
                    return null; // We are the root folder!

                IFileStatusCache cache = StatusCache;

                if (cache != null)
                    return cache[parentDir];
                else
                    return null;
            }
        }

        public GitDirectory ParentDirectory
        {
            get
            {
                string parentDir = Directory;

                if (string.IsNullOrEmpty(parentDir))
                    return null;

                IFileStatusCache cache = StatusCache;

                if (cache == null)
                    return null;

                return cache.GetDirectory(parentDir);
            }
        }

        /// <summary>
        /// Gets the working copy containing this <see cref="GitItem"/>
        /// </summary>
        /// <value>The working copy.</value>
        public GitWorkingCopy WorkingCopy
        {
            get
            {
                if (_workingCopy == null)
                {
                    if (IsAdministrativeArea || (!Exists && !IsVersioned))
                        return null;
                    else if (!IsDirectory)
                        _workingCopy = Parent;
                    else
                        _workingCopy = GitWorkingCopy.CalculateWorkingCopy(_context, this);
                }

                if (_workingCopy != null)
                    return _workingCopy.WorkingCopy;
                else
                    return null;
            }
        }

        /// <summary>
        /// Long path safe version of File.Exists(path) || Directory.Exists(path)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool PathExists(string path)
        {
            return NativeMethods.GetFileAttributes(path) != NativeMethods.INVALID_FILE_ATTRIBUTES;
        }

        static class NativeMethods
        {
            /// <summary>
            /// Gets the fileattributes of the specified file without going through the .Net normalization rules
            /// </summary>
            /// <param name="filename"></param>
            /// <returns></returns>
            public static uint GetFileAttributes(string filename)
            {
                // This method assumes filename is an absolute and/or rooted path
                if (string.IsNullOrEmpty(filename))
                    throw new ArgumentNullException("filename");

                if (filename.Length < 160)
                    return GetFileAttributesW(filename);
                else
                    return GetFileAttributesW("\\\\?\\" + filename); // Documented method of allowing paths over 160 characters (APR+SharpSvn use this too!)
            }

            [DllImport("kernel32.dll", ExactSpelling = true)]
            extern static uint GetFileAttributesW([MarshalAs(UnmanagedType.LPWStr)]string filename);

            public const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;
            public const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
            public const uint FILE_ATTRIBUTE_READONLY = 0x1;
        }

        /// <summary>
        /// Checks if the node is somehow added. In this case its parents are
        /// needed for commits
        /// </summary>
        public bool IsNewAddition
        {
            get { return IsAdded || IsReplaced || Status.IsCopied; }
        }

        static int _globalCookieBox = 0;

        /// <summary>
        /// Gets a new unique cookie
        /// </summary>
        /// <returns></returns>
        /// <remarks>Threadsafe provider of cookie values</remarks>
        static int NextCookie()
        {
            int n = System.Threading.Interlocked.Increment(ref _globalCookieBox); // Wraps on Int32.MaxValue

            if (n != 0)
                return n;
            else
                return NextCookie(); // 1 in 4 billion times
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            int lc = path.LastIndexOf(':');
            if (lc > 1)
                return false;
            else if (lc == 1 && path.IndexOf('\\') == 2)
                return true;
            else if (lc < 0 && path.StartsWith(@"\\", StringComparison.Ordinal))
                return true;

            // TODO: Add more checks. This code is called from the OpenDocumentTracker, Filestatus cache and selection provider

            return false;
        }

        public static bool IsValidPath(string path, bool extraChecks)
        {
            if (!IsValidPath(path))
                return false;

            if (extraChecks)
                foreach (char c in path)
                    switch (c)
                    {
                        case '<':
                        case '>':
                        case '|':
                        case '\"':
                        case '*':
                        case '?':
                            return false;
                        default:
                            if (c < 32)
                                return false;
                            break;
                    }

            return true;
        }
        /// <summary>
        /// Determines whether the current instance is below the specified path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// 	<c>true</c> if the <see cref="GitItem"/> is below or equal to the specified path; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBelowPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (!FullPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                return false;

            int n = FullPath.Length - path.Length;

            if (n > 0)
                return (FullPath[path.Length] == '\\');

            return (n == 0);
        }

        /// <summary>
        /// Determines whether the current instance is below the specified path
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [is below path] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBelowPath(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            return IsBelowPath(item.FullPath);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="other">The other.</param>
        /// <returns>The result of the operator.</returns>
        [DebuggerStepThrough]
        public static bool operator ==(GitItem one, GitItem other)
        {
            bool n1 = (object)one == null;
            bool n2 = (object)other == null;

            if (n1 || n2)
                return n1 && n2;

            return StringComparer.OrdinalIgnoreCase.Equals(one.FullPath, other.FullPath);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="other">The other.</param>
        /// <returns>The result of the operator.</returns>
        [DebuggerStepThrough]
        public static bool operator !=(GitItem one, GitItem other)
        {
            return !(one == other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            return Equals(obj as GitItem);
        }

        /// <summary>
        /// Equalses the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public bool Equals(GitItem obj)
        {
            if ((object)obj == null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(obj.FullPath, FullPath);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FullPath);
        }

        /// <summary>
        /// Gets the Uri of the node
        /// </summary>
        public Uri Uri
        {
            get
            {
                VisualGitStatus status = Status;

                return status.Uri;
            }
        }
    }
}
