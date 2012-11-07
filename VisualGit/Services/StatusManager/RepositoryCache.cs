using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.Services.StatusManager
{
    internal class RepositoryCache
    {
        private readonly object _syncRoot = new object();
        private CacheDirectoryEntry _cache;
        private readonly string _path;
        private readonly GitClient _client = new GitClient();

        public RepositoryCache(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _path = path;
        }

        public void Invalidate()
        {
            lock (_syncRoot)
            {
                _cache = null;
            }
        }

        public void GetFileStatus(string path, GitDepth depth, Func<GitFileStatus, bool> callback)
        {
            if (depth < GitDepth.Files)
                throw new InvalidOperationException("Expected GitDepth of Files or higher");

            Debug.Assert(
                String.Equals(path, _path, FileSystemUtil.StringComparison) ||
                path.StartsWith(_path + Path.DirectorySeparatorChar, FileSystemUtil.StringComparison)
            );

            CacheDirectoryEntry directory;

            lock (_syncRoot)
            {
                if (_cache == null)
                    RebuildCache();

                directory = _cache;
            }

            // Find the starting point. If we can match a file, process it
            // and finish.

            if (!String.Equals(path, _path, FileSystemUtil.StringComparison))
            {
                var parts = path.Split(Path.DirectorySeparatorChar);

                for (int i = 0; i < parts.Length; i++)
                {
                    CacheEntry entry;

                    // Path doesn't exist.

                    if (!directory.Items.TryGetValue(parts[i], out entry))
                        return;

                    if (entry.Status.NodeKind == GitNodeKind.File)
                    {
                        // If we're at the end, report the item and we're done.
                        // Otherwise, the path was invalid because it contained
                        // parts after a path to a file.

                        if (i == parts.Length - 1)
                            callback(entry.Status);

                        return;
                    }

                    directory = (CacheDirectoryEntry)entry;
                }
            }

            // We're reporting a directory.

            ReportDirectory(callback, directory, depth > GitDepth.Files);
        }

        private bool ReportDirectory(Func<GitFileStatus, bool> callback, CacheDirectoryEntry directory, bool recurse)
        {
            if (!callback(directory.Status))
                return false;

            foreach (var item in directory.Items.Values)
            {
                if (item.Status.NodeKind == GitNodeKind.Directory)
                {
                    if (recurse)
                    {
                        if (!ReportDirectory(callback, (CacheDirectoryEntry)item, true))
                            return false;
                    }
                }
                else
                {
                    if (!callback(item.Status))
                        return false;
                }
            }

            return true;
        }

        private void RebuildCache()
        {
            _cache = new CacheDirectoryEntry(GetDirectoryStatus(_path));

            _client.Status(_path, new GitStatusArgs { ThrowOnError = true }, Callback);
        }

        private void Callback(object sender, GitStatusEventArgs e)
        {
            var status = e.Status;

            Debug.Assert(
                status.FullPath.StartsWith(_path + Path.DirectorySeparatorChar, FileSystemUtil.StringComparison)
            );

            string relativePath = status.FullPath.Substring(_path.Length + 1);
            string[] parts = relativePath.Split(Path.DirectorySeparatorChar);

            var directory = _cache;
            string partialPath = _path;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                CacheEntry subDirectory;

                partialPath = Path.Combine(partialPath, parts[i]);

                if (!directory.Items.TryGetValue(parts[i], out subDirectory))
                {
                    subDirectory = new CacheDirectoryEntry(GetDirectoryStatus(partialPath));

                    directory.Items.Add(parts[i], subDirectory);
                }

                Debug.Assert(subDirectory.Status.NodeKind == GitNodeKind.Directory);

                directory = (CacheDirectoryEntry)subDirectory;
            }

            Debug.Assert(!directory.Items.ContainsKey(parts[parts.Length - 1]));

            directory.Items.Add(parts[parts.Length - 1], new CacheEntry(status));
        }

        private static GitFileStatus GetDirectoryStatus(string path)
        {
            return new GitFileStatus(
                path,
                GitStatus.Normal,
                GitSchedule.Normal,
                GitInternalStatus.Unset,
                GitNodeKind.Directory
            );
        }
    }
}
