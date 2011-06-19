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
            if (callback == null)
                throw new ArgumentNullException("callback");

            var repositoryEntry = Client.GetRepository(path);

            if (Args.Depth < GitDepth.Files)
                throw new NotImplementedException();

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                // First collect all DirCacheEntry's also to detect whether the
                // provided entry was a directory or file.

                var actualNodeKind = GitNodeKind.Unknown;
                var dirCacheEntries = new List<DirCacheEntry>();
                string relativePath = repository.GetRepositoryPath(path);

                var workingTreeIt = new FileTreeIterator(repository);
                var diff = new IndexDiff(repository, Constants.HEAD, workingTreeIt);

                var filter = new CustomPathFilter(relativePath, Args.Depth);
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

                    if (GitTools.PathMatches(relativePath, pathString, false, Args.Depth))
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
                        if (GitTools.PathMatches(relativePath, pathString, false, Args.Depth))
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

                // Inform the ignore manager we are working of a file or path.

                repositoryEntry.IgnoreManager.RefreshPath(path);

                // Return an entry for the directory.

                GitStatusEventArgs e;

                if (actualNodeKind == GitNodeKind.Directory)
                {
                    var state =
                        repositoryEntry.IgnoreManager.IsIgnored(path, GitNodeKind.Directory)
                        ? GitInternalStatus.Ignored
                        : GitInternalStatus.Unset;

                    e = new GitStatusEventArgs
                    {
                        FullPath = path,
                        LocalContentStatus = GetStatus(state),
                        NodeKind = GitNodeKind.Directory,
                        WorkingCopyInfo = new GitWorkingCopyInfo
                        {
                            NodeKind = GitNodeKind.Directory,
                            Schedule = GetScheduleState(state)
                        }
                    };

                    callback(Client, e);

                    if (CancelRequested(e))
                        return;
                }

                var seen = new HashSet<string>(FileSystemUtil.StringComparer);

                foreach (var entry in dirCacheEntries)
                {
                    string fullPath = repository.GetAbsoluteRepositoryPath(entry.PathString);

                    var state = GetInternalStatus(entry, diff);

                    e = new GitStatusEventArgs
                    {
                        FullPath = fullPath,
                        LocalContentStatus = GetStatus(state),
                        InternalContentStatus = state,
                        NodeKind = GitNodeKind.File,
                        WorkingCopyInfo = new GitWorkingCopyInfo
                        {
                            NodeKind = GitNodeKind.File,
                            Schedule = GetScheduleState(state)
                        }
                    };

                    callback(Client, e);

                    if (CancelRequested(e))
                        return;

                    seen.Add(fullPath);
                }

                bool cancelled;

                AddUnseenFiles(
                    callback, repository, relativePath, seen, diff.GetRemoved(),
                    GitInternalStatus.Removed, out cancelled
                );

                if (cancelled)
                    return;

                AddUnseenFiles(
                    callback, repository, relativePath, seen, diff.GetUntracked(),
                    GitInternalStatus.Untracked, out cancelled
                );

                if (cancelled)
                    return;

                if (
                    (Args.RetrieveIgnoredEntries || Args.RetrieveAllEntries) &&
                    actualNodeKind == GitNodeKind.Directory &&
                    Directory.Exists(path)
                ) {
                    foreach (string fullPath in Directory.GetFiles(path))
                    {
                        // If the item has not yet been seen, or it is unclean
                        // after we've read the entry.Repository, it's either added
                        // or ignored.

                        if (!GitTools.PathMatches(relativePath, repository.GetRepositoryPath(fullPath), false, Args.Depth))
                            continue;

                        if (!seen.Contains(fullPath))
                        {
                            var state =
                                repositoryEntry.IgnoreManager.IsIgnored(fullPath, GitNodeKind.File)
                                ? GitInternalStatus.Ignored
                                : GitInternalStatus.Untracked;

                            if (
                                (Args.RetrieveAllEntries && (state == GitInternalStatus.Untracked || state == GitInternalStatus.Ignored)) ||
                                (Args.RetrieveIgnoredEntries && state == GitInternalStatus.Ignored)
                            ) {
                                e = new GitStatusEventArgs
                                {
                                    FullPath = fullPath,
                                    LocalContentStatus = GetStatus(state),
                                    InternalContentStatus = state,
                                    NodeKind = GitNodeKind.File,
                                    WorkingCopyInfo = new GitWorkingCopyInfo
                                    {
                                        NodeKind = GitNodeKind.File,
                                        Schedule = GetScheduleState(state)
                                    }
                                };

                                callback(Client, e);

                                if (CancelRequested(e))
                                    return;
                            }

                            seen.Add(fullPath);
                        }
                    }
                }
            }
        }

        private void AddUnseenFiles(EventHandler<GitStatusEventArgs> callback, Repository repository, string relativePath, HashSet<string> seen, ICollection<string> paths, GitInternalStatus state, out bool cancelled)
        {
            cancelled = false;

            foreach (string deletedPath in paths)
            {
                if (!GitTools.PathMatches(relativePath, deletedPath, false, Args.Depth))
                    continue;

                string fullPath = repository.GetAbsoluteRepositoryPath(deletedPath);

                // Prevent double reporting of naming conflicts. This ensures
                // that only the Missing is reported, and not the Untracked.

                if (seen.Contains(fullPath))
                    continue;

                var e = new GitStatusEventArgs
                {
                    FullPath = fullPath,
                    LocalContentStatus = GetStatus(state),
                    InternalContentStatus = state,
                    NodeKind = GitNodeKind.File,
                    WorkingCopyInfo = new GitWorkingCopyInfo
                    {
                        NodeKind = GitNodeKind.File,
                        Schedule = GetScheduleState(state)
                    }
                };

                callback(Client, e);

                if (CancelRequested(e))
                {
                    cancelled = true;
                    break;
                }

                seen.Add(fullPath);
            }
        }

        private GitStatus GetStatus(GitInternalStatus state)
        {
            if (state.HasFlag(GitInternalStatus.Conflicted))
                return GitStatus.Conflicted;
            if (state.HasFlag(GitInternalStatus.Added))
                return GitStatus.Added;
            if (state.HasFlag(GitInternalStatus.AssumeUnchanged))
                return GitStatus.Normal;
            if (state.HasFlag(GitInternalStatus.Changed))
                return GitStatus.Modified;
            if (state.HasFlag(GitInternalStatus.Ignored))
                return GitStatus.Ignored;
            if (state.HasFlag(GitInternalStatus.Missing))
                return GitStatus.Missing;
            if (state.HasFlag(GitInternalStatus.Modified))
                return GitStatus.Modified;
            if (state.HasFlag(GitInternalStatus.Removed))
                return GitStatus.Deleted;
            if (state.HasFlag(GitInternalStatus.Untracked))
                return GitStatus.NotVersioned;

            return GitStatus.Normal;
        }

        private GitSchedule GetScheduleState(GitInternalStatus state)
        {
            switch (state)
            {
                case GitInternalStatus.Added: return GitSchedule.Add;
                case GitInternalStatus.Removed: return GitSchedule.Delete;
                default: return GitSchedule.Normal;
            }
        }

        private GitInternalStatus GetInternalStatus(DirCacheEntry item, IndexDiff diff)
        {
            string itemName = item.PathString;
            GitInternalStatus result = GitInternalStatus.Unset;

            if (diff.GetConflicting().Contains(itemName))
                result |= GitInternalStatus.Conflicted;
            if (diff.GetAdded().Contains(itemName))
                result |= GitInternalStatus.Added;
            if (diff.GetAssumeUnchanged().Contains(itemName))
                result |= GitInternalStatus.AssumeUnchanged;
            if (diff.GetChanged().Contains(itemName))
                result |= GitInternalStatus.Changed;
            if (diff.GetModified().Contains(itemName))
                result |= GitInternalStatus.Modified;
            if (diff.GetMissing().Contains(itemName))
                result |= GitInternalStatus.Missing;
            if (diff.GetRemoved().Contains(itemName))
                result |= GitInternalStatus.Removed;
            if (diff.GetUntracked().Contains(itemName))
                result |= GitInternalStatus.Untracked;

            return result;
        }
    }
}
