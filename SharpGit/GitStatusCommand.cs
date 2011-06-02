using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Treewalk;
using NGit;
using NGit.Treewalk.Filter;
using NGit.Dircache;
using System.IO;

namespace SharpGit
{
    internal sealed class GitStatusCommand : GitCommand<GitStatusArgs>
    {
        public GitStatusCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string path, EventHandler<GitStatusEventArgs> callback)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (callback == null)
                throw new ArgumentNullException("callback");

            var repositoryEntry = RepositoryManager.GetRepository(path);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            if (Args.Depth != GitDepth.Files)
                throw new NotImplementedException();

            lock (repositoryEntry.SyncLock)
            {
                var repository = repositoryEntry.Repository;

                // First collect all DirCacheEntry's also to detect whether the
                // provided entry was a directory or file.

                var actualNodeKind = GitNodeKind.Unknown;
                var dirCacheEntries = new List<DirCacheEntry>();
                string relativePath = repository.GetRepositoryPath(path);

                var workingTreeIt = new FileTreeIterator(repository);
                var diff = new IndexDiff(repository, Constants.HEAD, workingTreeIt);
                var filter = new PathFilterNotRecurse(relativePath, Args.Depth);
                diff.SetFilter(filter);
                diff.Diff();

                var dirCache = repository.ReadDirCache();
                int entryCount = dirCache.GetEntryCount();

                for (int i = 0; i < entryCount; i++)
                {
                    var entry = dirCache.GetEntry(i);
                    string pathString = entry.PathString;

                    if (String.Equals(pathString, relativePath, FileSystemUtil.StringComparison))
                        actualNodeKind = GitNodeKind.File;

                    if (RepositoryUtil.PathMatches(relativePath, pathString, false, Args.Depth))
                    {
                        if (actualNodeKind == GitNodeKind.Unknown)
                            actualNodeKind = GitNodeKind.Directory;

                        dirCacheEntries.Add(entry);
                    }
                }

                if (actualNodeKind == GitNodeKind.Unknown)
                {
                    foreach (string pathString in diff.GetRemoved())
                    {
                        if (String.Equals(pathString, relativePath, FileSystemUtil.StringComparison))
                        {
                            actualNodeKind = GitNodeKind.File;
                            break;
                        }
                        if (RepositoryUtil.PathMatches(relativePath, pathString, false, Args.Depth))
                        {
                            actualNodeKind = GitNodeKind.Directory;
                            break;
                        }
                    }
                }

                if (actualNodeKind == GitNodeKind.Unknown)
                {
                    if (Directory.Exists(path))
                        actualNodeKind = GitNodeKind.Directory;
                    else if (File.Exists(path))
                        actualNodeKind = GitNodeKind.File;
                }

                // Return an entry for the directory.

                GitStatusEventArgs e;

                if (actualNodeKind == GitNodeKind.Directory)
                {
                    e = new GitStatusEventArgs
                    {
                        FullPath = path,
                        LocalContentStatus = GitStatus.Normal,
                        NodeKind = GitNodeKind.Directory,
                        Uri = new Uri("file:///" + path),
                        WorkingCopyInfo = new GitWorkingCopyInfo
                        {
                            NodeKind = GitNodeKind.Directory,
                            Schedule = GitSchedule.Normal
                        }
                    };

                    callback(Client, e);

                    if (e.Cancel)
                        return;
                }

                var seen = new HashSet<string>(FileSystemUtil.StringComparer);

                foreach (var entry in dirCacheEntries)
                {
                    string fullPath = repository.GetAbsoluteRepositoryPath(entry.PathString);

                    e = new GitStatusEventArgs
                    {
                        FullPath = fullPath,
                        LocalContentStatus = GetStatus(entry, diff),
                        NodeKind = GitNodeKind.File,
                        Uri = new Uri("file:///" + fullPath),
                        WorkingCopyInfo = new GitWorkingCopyInfo
                        {
                            NodeKind = GitNodeKind.File,
                            Schedule = GitSchedule.Normal
                        }
                    };

                    callback(Client, e);

                    if (e.Cancel)
                        return;

                    seen.Add(fullPath);
                }

                foreach (string deletedPath in diff.GetRemoved())
                {
                    if (!RepositoryUtil.PathMatches(relativePath, deletedPath, false, Args.Depth))
                        continue;

                    string fullPath = repository.GetAbsoluteRepositoryPath(deletedPath);

                    e = new GitStatusEventArgs
                    {
                        FullPath = fullPath,
                        LocalContentStatus = GitStatus.Deleted,
                        NodeKind = GitNodeKind.File,
                        Uri = new Uri("file:///" + fullPath),
                        WorkingCopyInfo = new GitWorkingCopyInfo
                        {
                            NodeKind = GitNodeKind.File,
                            Schedule = GitSchedule.Delete
                        }
                    };

                    callback(Client, e);

                    if (e.Cancel)
                        return;

                    seen.Add(fullPath);
                }

                if (Args.RetrieveIgnoredEntries || Args.RetrieveAllEntries)
                {
                    var iterator = new FileTreeIterator(
                        path, repository.FileSystem, repository.GetConfig().Get(WorkingTreeOptions.KEY)
                    );

                    for (; !iterator.Eof; iterator.Next(1))
                    {
                        // If the item has not yet been seen, or it is unclean
                        // after we've read the entry.Repository, it's either added
                        // or ignored.

                        string fullPath = iterator.GetEntryFile();

                        if (!RepositoryUtil.PathMatches(relativePath, repository.GetRepositoryPath(fullPath), false, Args.Depth))
                            continue;

                        if (!seen.Contains(fullPath))
                        {
                            GitStatus state;

                            if (iterator.IsEntryIgnored())
                                state = GitStatus.Ignored;
                            else
                                state = GitStatus.NotVersioned;

                            if (
                                (Args.RetrieveAllEntries && (state == GitStatus.NotVersioned || state == GitStatus.Ignored)) ||
                                (Args.RetrieveIgnoredEntries && state == GitStatus.Ignored)
                            ) {
                                e = new GitStatusEventArgs
                                {
                                    FullPath = fullPath,
                                    LocalContentStatus = state,
                                    NodeKind = GitNodeKind.File,
                                    Uri = new Uri("file:///" + fullPath),
                                    WorkingCopyInfo = new GitWorkingCopyInfo
                                    {
                                        NodeKind = GitNodeKind.File,
                                        Schedule = GitSchedule.Delete
                                    }
                                };

                                callback(Client, e);

                                if (e.Cancel)
                                    return;
                            }

                            seen.Add(fullPath);
                        }
                    }
                }
            }
        }

        private GitStatus GetStatus(DirCacheEntry item, IndexDiff diff)
        {
            string itemName = item.PathString;

            if (diff.GetAdded().Contains(itemName))
                return GitStatus.Added;
            else if (diff.GetAssumeUnchanged().Contains(itemName))
                return GitStatus.Normal;
            else if (diff.GetChanged().Contains(itemName) || diff.GetModified().Contains(itemName))
                return GitStatus.Modified;
            else if (diff.GetMissing().Contains(itemName))
                return GitStatus.Deleted;
            else if (diff.GetRemoved().Contains(itemName))
                return GitStatus.Deleted;
            else if (diff.GetUntracked().Contains(itemName))
                return GitStatus.NotVersioned;
            else
                return GitStatus.Normal;
        }
    }
}
