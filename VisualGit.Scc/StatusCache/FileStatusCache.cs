using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VisualGit.Commands;
using VisualGit.Scc;
using SharpSvn;
using SharpGit;

namespace VisualGit.Scc.StatusCache
{
    /// <summary>
    /// Maintains path->GitItem mappings.
    /// </summary>
    [GlobalService(typeof(IFileStatusCache), AllowPreRegistered=true)]
    [GlobalService(typeof(IGitItemChange), AllowPreRegistered=true)]
    sealed partial class FileStatusCache : VisualGitService, VisualGit.Scc.IFileStatusCache, IGitItemChange
    {
        readonly object _lock = new object();
        readonly GitClient _client;
        readonly Dictionary<string, GitItem> _map; // Maps from full-normalized paths to SvnItems
        readonly Dictionary<string, GitDirectory> _dirMap;
        IVisualGitCommandService _commandService;

        public FileStatusCache(IVisualGitServiceProvider context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _client = new GitClient();
            _map = new Dictionary<string, GitItem>(StringComparer.OrdinalIgnoreCase);
            _dirMap = new Dictionary<string, GitDirectory>(StringComparer.OrdinalIgnoreCase);
            InitializeShellMonitor();
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseShellMonitor(disposing);
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the command service.
        /// </summary>
        /// <value>The command service.</value>
        IVisualGitCommandService CommandService
        {
            get { return _commandService ?? (_commandService = Context.GetService<IVisualGitCommandService>()); }
        }

        void VisualGit.Scc.IFileStatusCache.RefreshItem(GitItem item, GitNodeKind nodeKind)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            RefreshPath(item.FullPath, nodeKind, GitDepth.Files);

            IGitItemUpdate updateItem = (IGitItemUpdate)item;

            if (!updateItem.IsStatusClean())
            {
                // Ok, the status update did not refresh the item requesting to be refreshed
                // That means the item is not here or RefreshPath would have added it

                GitItem other;
                if (_map.TryGetValue(item.FullPath, out other) && other != item)
                {
                    updateItem.RefreshTo(other); // This item is no longer current; but we have the status anyway
                }
                else
                {
                    Debug.Assert(false, "RefreshPath did not deliver up to date information",
                        "The RefreshPath public api promises delivering up to date data, but none was received");

                    updateItem.RefreshTo(item.Exists ? NoSccStatus.NotVersioned : NoSccStatus.NotExisting, GitNodeKind.Unknown);
                }
            }

            Debug.Assert(updateItem.IsStatusClean(), "The item requesting to be updated is updated");
        }

        GitItem CreateItem(string fullPath, VisualGitStatus status)
        {
            return new GitItem(this, fullPath, status);
        }

        GitItem CreateItem(string fullPath, NoSccStatus status, GitNodeKind nodeKind)
        {
            return new GitItem(this, fullPath, status, nodeKind);
        }

        GitItem CreateItem(string fullPath, NoSccStatus status)
        {
            return CreateItem(fullPath, status, GitNodeKind.Unknown);
        }

        /// <summary>
        /// Stores the item in the caching dictionary/ies
        /// </summary>
        /// <param name="item"></param>
        void StoreItem(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _map[item.FullPath] = item;

            GitDirectory dir;
            if (_dirMap.TryGetValue(item.FullPath, out dir))
            {
                if (item.IsDirectory)
                {
                    ((IGitDirectoryUpdate)dir).Store(item);
                }
                else
                    ScheduleForCleanup(dir);
            }

            string parentDir = SvnTools.GetNormalizedDirectoryName(item.FullPath);

            if (string.IsNullOrEmpty(parentDir) || parentDir == item.FullPath)
                return; // Skip root directory

            if (_dirMap.TryGetValue(item.FullPath, out dir))
            {
                ((IGitDirectoryUpdate)dir).Store(item);
            }
        }

        void RemoveItem(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            bool deleted = false;
            GitDirectory dir;
            if (_dirMap.TryGetValue(item.FullPath, out dir))
            {
                // The item is a directory itself.. remove it's map
                if (dir.Directory == item)
                {
                    _dirMap.Remove(item.FullPath);
                    deleted = true;
                }
            }

            GitItem other;
            if (_map.TryGetValue(item.FullPath, out other))
            {
                if (item == other)
                    _map.Remove(item.FullPath);
            }

            if (!deleted)
                return;

            string parentDir = SvnTools.GetNormalizedDirectoryName(item.FullPath);

            if (string.IsNullOrEmpty(parentDir) || parentDir == item.FullPath)
                return; // Skip root directory

            if (_dirMap.TryGetValue(item.FullPath, out dir))
            {
                dir.Remove(item.FullPath);
            }
        }

        /// <summary>
        /// Refreshes the specified path using the specified depth
        /// </summary>
        /// <param name="path">A normalized path</param>
        /// <param name="pathKind"></param>
        /// <param name="depth"></param>
        /// <remarks>
        /// If the path is a file and depth is greater that <see cref="GitDepth.Empty"/> the parent folder is walked instead.
        /// 
        /// <para>This method guarantees that after calling it at least one up-to-date item exists 
        /// in the statusmap for <paramref name="path"/>. If we can not find information we create
        /// an unspecified item
        /// </para>
        /// </remarks>
        void RefreshPath(string path, GitNodeKind pathKind, GitDepth depth)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (depth < GitDepth.Empty || depth > GitDepth.Infinity)
                throw new ArgumentNullException("depth"); // Make sure we fail on possible new depths

            string walkPath = path;
            bool walkingDirectory = false;

            switch (pathKind)
            {
                case GitNodeKind.Directory:
                    walkingDirectory = true;
                    break;
                case GitNodeKind.File:
                    if (depth != GitDepth.Empty)
                    {
                        walkPath = SvnTools.GetNormalizedDirectoryName(path);
                        walkingDirectory = true;
                    }
                    break;
                default:
                    try
                    {
                        if (File.Exists(path)) // ### Not long path safe
                        {
                            pathKind = GitNodeKind.File;
                            goto case GitNodeKind.File;
                        }
                    }
                    catch (PathTooLongException)
                    { /* Fall through */ }
                    break;
            }

            GitStatusArgs args = new GitStatusArgs();
            args.Depth = depth;
            args.RetrieveAllEntries = true;
            args.RetrieveIgnoredEntries = true;
            args.ThrowOnError = false;

            lock (_lock)
            {
                GitDirectory directory = null;
                IGitDirectoryUpdate updateDir = null;
                GitItem walkItem;

                if (depth > GitDepth.Empty)
                {
                    // We get more information for free, lets use that to update other items
                    if (_dirMap.TryGetValue(walkPath, out directory))
                    {
                        updateDir = directory;

                        if (depth > GitDepth.Children)
                            updateDir.TickAll();
                        else
                            updateDir.TickFiles();
                    }
                    else
                    {
                        // No existing directory instance, let's create one
                        updateDir = directory = new GitDirectory(Context, walkPath);
                        _dirMap[walkPath] = directory;
                    }

                    walkItem = directory.Directory;
                }
                else
                {
                    if (_map.TryGetValue(walkPath, out walkItem))
                        ((IGitItemUpdate)walkItem).TickItem();
                }

                bool statSelf = false;

                bool ok = _client.Status(walkPath, args, RefreshCallback);

                if (!ok)
                    statSelf = true;
                else if (directory != null)
                    walkItem = directory.Directory; // Might have changed via casing

                if (!statSelf)
                {
                    if (((IGitItemUpdate)walkItem).ShouldRefresh())
                        statSelf = true;
                    else if (walkingDirectory && !walkItem.IsVersioned)
                        statSelf = true;
                }

                if (statSelf)
                {
                    // Svn did not stat the items for us.. Let's make something up

                    if (walkingDirectory)
                        StatDirectory(walkPath, depth, directory);
                    else
                    {
                        // Just stat the item passed and nothing else in the Depth.Empty case

                        if (walkItem == null)
                        {
                            string truepath = SvnTools.GetTruePath(walkPath); // Gets the on-disk casing if it exists

                            StoreItem(walkItem = CreateItem(truepath ?? walkPath,
                                (truepath != null) ? NoSccStatus.NotVersioned : NoSccStatus.NotExisting, GitNodeKind.Unknown));
                        }
                        else
                        {
                            ((IGitItemUpdate)walkItem).RefreshTo(walkItem.Exists ? NoSccStatus.NotVersioned : NoSccStatus.NotExisting, GitNodeKind.Unknown);
                        }
                    }
                }

                if (directory != null)
                {
                    foreach (IGitItemUpdate item in directory)
                    {
                        if (item.IsItemTicked()) // These items were not found in the stat calls
                            item.RefreshTo(NoSccStatus.NotExisting, GitNodeKind.Unknown);
                    }

                    if (updateDir.ScheduleForCleanup)
                        ScheduleForCleanup(directory); // Handles removing already deleted items
                    // We keep them cached for the current command only
                }


                GitItem pathItem; // We promissed to return an updated item for the specified path; check if we updated it

                if (!_map.TryGetValue(path, out pathItem))
                {
                    // We did not; it does not even exist in the cache
                    StoreItem(pathItem = CreateItem(path, NoSccStatus.NotExisting));

                    if (directory != null)
                    {
                        ((IGitDirectoryUpdate)directory).Store(pathItem);
                        ScheduleForCleanup(directory);
                    }
                }
                else
                {
                    IGitItemUpdate update = pathItem;

                    if (!update.IsStatusClean())
                    {
                        update.RefreshTo(NoSccStatus.NotExisting, GitNodeKind.Unknown); // We did not see it in the walker

                        if (directory != null)
                        {
                            ((IGitDirectoryUpdate)directory).Store(pathItem);
                            ScheduleForCleanup(directory);
                        }
                    }
                }
            }
        }

        private void StatDirectory(string walkPath, GitDepth depth, GitDirectory directory)
        {
            // Note: There is a lock(_lock) around this in our caller

            bool canRead;
            string adminName = GitConstants.AdministrativeDirectoryName;
            foreach (SccFileSystemNode node in SccFileSystemNode.GetDirectoryNodes(walkPath, out canRead))
            {
                if (depth < GitDepth.Files)
                    break;

                if (string.Equals(node.Name, adminName, StringComparison.OrdinalIgnoreCase) || node.IsHiddenOrSystem)
                    continue;

                GitItem item;
                if (node.IsFile)
                {
                    if (!_map.TryGetValue(node.FullPath, out item))
                        StoreItem(CreateItem(node.FullPath, NoSccStatus.NotVersioned, GitNodeKind.File));
                    else
                    {
                        IGitItemUpdate updateItem = item;
                        if (updateItem.ShouldRefresh())
                            updateItem.RefreshTo(NoSccStatus.NotVersioned, GitNodeKind.File);
                    }
                }
                else
                {
                    if (!_map.TryGetValue(node.FullPath, out item))
                        StoreItem(CreateItem(node.FullPath, NoSccStatus.NotVersioned, GitNodeKind.Directory));
                    // Don't clear state of a possible working copy
                }
            }

            if (canRead) // The directory exists
            {
                GitItem item;

                if (!_map.TryGetValue(walkPath, out item))
                {
                    StoreItem(CreateItem(walkPath, NoSccStatus.NotVersioned, GitNodeKind.Directory));
                    // Mark it as existing if we are sure 
                }
                else
                {
                    IGitItemUpdate updateItem = item;
                    if (updateItem.ShouldRefresh())
                        updateItem.RefreshTo(NoSccStatus.NotVersioned, GitNodeKind.Directory);
                }
            }
       
            // Note: There is a lock(_lock) around this in our caller
        }

        bool _postedCleanup;
        List<GitDirectory> _cleanup = new List<GitDirectory>();
        private void ScheduleForCleanup(GitDirectory directory)
        {
            lock (_lock)
            {
                if (!_cleanup.Contains(directory))
                    _cleanup.Add(directory);

                if (!_postedCleanup)
                    CommandService.SafePostTickCommand(ref _postedCleanup, VisualGitCommand.FileCacheFinishTasks);
            }
        }

        internal void OnCleanup()
        {
            lock (_lock)
            {
                _postedCleanup = false;

                while (_cleanup.Count > 0)
                {
                    GitDirectory dir = _cleanup[0];
                    string path = dir.FullPath;

                    _cleanup.RemoveAt(0);

                    for (int i = 0; i < dir.Count; i++)
                    {
                        GitItem item = dir[i];
                        if (((IGitItemUpdate)item).ShouldClean())
                        {
                            RemoveItem(item);
                            dir.RemoveAt(i--);
                        }
                    }

                    if (dir.Count == 0)
                    {
                        // We cache the path before.. as we don't want the svnitem to be generated again
                        _dirMap.Remove(path);
                    }
                }
            }
        }

        static bool NewFullPathOk(GitItem item, string fullPath, VisualGitStatus status)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            else if (status == null)
                throw new ArgumentNullException("status");

            if (fullPath == item.FullPath)
                return true;

            switch (status.State)
            {
                case GitStatus.Added:
                case GitStatus.Conflicted:
                case GitStatus.Merged:
                case GitStatus.Modified:
                case GitStatus.Normal:
                case GitStatus.Replaced:
                case GitStatus.Deleted:
                case GitStatus.Incomplete:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Called from RefreshPath's call to <see cref="SvnClient::Status"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// All information we receive here is live from SVN and Disk and is therefore propagated
        /// in all SvnItems wishing information
        /// </remarks>
        void RefreshCallback(object sender, GitStatusEventArgs e)
        {
            // Note: There is a lock(_lock) around this in our caller

            VisualGitStatus status = new VisualGitStatus(e);
            string path = e.FullPath; // Fully normalized

            GitItem item;
            if (!_map.TryGetValue(path, out item) || !NewFullPathOk(item, path, status))
            {
                // We only create an item if we don't have an existing
                // with a valid path. (No casing changes allowed!)
                
                GitItem newItem = CreateItem(path, status); 
                StoreItem(newItem);

                if (item != null)
                {
                    ((IGitItemUpdate)item).RefreshTo(newItem); 
                    item.Dispose();
                }

                item = newItem;
            }
            else
                ((IGitItemUpdate)item).RefreshTo(status);

            // Note: There is a lock(_lock) around this in our caller
        }

        /// <summary>
        /// Marks the specified file dirty
        /// </summary>
        /// <param name="file"></param>
        void VisualGit.Scc.IFileStatusCache.MarkDirty(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            string normPath = SvnTools.GetNormalizedFullPath(path);

            lock (_lock)
            {
                GitItem item;

                if (_map.TryGetValue(normPath, out item))
                {
                    item.MarkDirty();
                }
            }
        }

        void VisualGit.Scc.IFileStatusCache.MarkDirtyRecursive(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            
            lock (_lock)
            {
                List<string> names = new List<string>();

                foreach (GitItem v in _map.Values)
                {
                    string name = v.FullPath;
                    if (v.IsBelowPath(path))
                    {
                        v.MarkDirty();
                    }
                }
            }
        }

        public IList<GitItem> GetCachedBelow(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            
            lock (_lock)
            {
                List<GitItem> items = new List<GitItem>();

                foreach (GitItem v in _map.Values)
                {
                    if(v.IsBelowPath(path))
                        items.Add(v);
                }

                return items;
            }
        }

        public IList<GitItem> GetCachedBelow(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("path");

            lock (_lock)
            {
                SortedList<string, GitItem> items = new SortedList<string, GitItem>(StringComparer.OrdinalIgnoreCase);

                foreach (string path in paths)
                {
                    foreach (GitItem v in _map.Values)
                    {
                        if (v.IsBelowPath(path))
                            items[v.FullPath] = v;
                    }
                }

                return new List<GitItem>(items.Values);
            }
        }

        /// <summary>
        /// Marks the specified file dirty
        /// </summary>
        /// <param name="file"></param>
        void VisualGit.Scc.IFileStatusCache.MarkDirty(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            lock (_lock)
            {
                foreach (string path in paths)
                {
                    string normPath = SvnTools.GetNormalizedFullPath(path);
                    GitItem item;

                    if (_map.TryGetValue(normPath, out item))
                    {
                        item.MarkDirty();
                    }
                }
            }
        }


        public GitItem this[string path]
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException("path");

                path = SvnTools.GetNormalizedFullPath(path);

                lock (_lock)
                {
                    GitItem item;

                    if (!_map.TryGetValue(path, out item))
                    {
                        string truePath = SvnTools.GetTruePath(path, true);

                        // Just create an item based on his name. Delay the svn calls as long as we can
                        StoreItem(item = new GitItem(this, truePath ?? path, NoSccStatus.Unknown, GitNodeKind.Unknown));

                        //item.MarkDirty(); // Load status on first access
                    }

                    return item;
                }
            }
        }

        /// <summary>
        /// Clears the whole cache; called from solution closing (Scc)
        /// </summary>
        public void ClearCache()
        {
            lock (_lock)
            {
                this._dirMap.Clear();
                this._map.Clear();
            }
        }

        void IFileStatusCache.SetSolutionContained(string path, bool contained)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            GitItem item;
            if (_map.TryGetValue(path, out item))
                ((IGitItemStateUpdate)item).SetSolutionContained(contained);
        }

        #region IFileStatusCache Members


        public GitDirectory GetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            lock (_lock)
            {
                GitDirectory dir;

                if (_dirMap.TryGetValue(path, out dir))
                    return dir;

                GitItem item = this[path];

                if (item.IsDirectory)
                {
                    dir = new GitDirectory(Context, path);
                    dir.Add(item);
                    return dir;
                }
                else
                    return null;
            }
        }

        #endregion

        internal void BroadcastChanges()
        {
            IGitItemStateUpdate update;
            if(_map.Count > 0)
                update = GetFirst(_map.Values);
            else
                update = this["c:\\windows"]; // Just give me a GitItem instance to access the interface

            IList<GitItem> updates = update.GetUpdateQueueAndClearScheduled();

            if(updates != null)
                OnGitItemsChanged(new GitItemsEventArgs(updates));
        }

        static T GetFirst<T>(IEnumerable<T> valueCollection)
            where T : class
        {
            foreach (T a in valueCollection)
                return a;

            return null;
        }

        public event EventHandler<GitItemsEventArgs> GitItemsChanged;

        public void OnGitItemsChanged(GitItemsEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (GitItemsChanged != null)
                GitItemsChanged(this, e);
        }
    }
}
