// SharpGit\GitStatusCommand.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Treewalk;
using NGit;
using NGit.Dircache;

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

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var seen = new HashSet<string>(FileSystemUtil.StringComparer);

                var diff = new IndexDiff(repository, Constants.HEAD, new FileTreeIterator(repository));

                diff.Diff();

                var dirCache = repository.ReadDirCache();

                for (int i = 0, count = dirCache.GetEntryCount(); i < count; i++)
                {
                    var entry = dirCache.GetEntry(i);
                    string fullPath = repository.GetAbsoluteRepositoryPath(entry.PathString);

                    var state = GetInternalStatus(entry, diff);

                    var e = new GitStatusEventArgs
                    {
                        Status = new GitFileStatus(
                            fullPath,
                            GetStatus(state),
                            GetScheduleState(state),
                            state,
                            GitNodeKind.File
                        )
                    };

                    callback(Client, e);

                    if (CancelRequested(e))
                        return;

                    seen.Add(fullPath);
                }

                bool cancelled;

                AddUnseenFiles(
                    callback, repository, seen, diff.GetRemoved(),
                    GitInternalStatus.Removed, out cancelled
                );

                if (cancelled)
                    return;

                AddUnseenFiles(
                    callback, repository, seen, diff.GetUntracked(),
                    GitInternalStatus.Untracked, out cancelled
                );

                if (cancelled)
                    return;

                AddUnseenFiles(
                    callback, repository, seen, diff.GetIgnoredNotInIndex(),
                    GitInternalStatus.Ignored, out cancelled
                );
            }
        }

        private void AddUnseenFiles(EventHandler<GitStatusEventArgs> callback, Repository repository, HashSet<string> seen, IEnumerable<string> paths, GitInternalStatus state, out bool cancelled)
        {
            cancelled = false;

            foreach (string path in paths)
            {
                string fullPath = repository.GetAbsoluteRepositoryPath(path);

                // Prevent double reporting of naming conflicts. This ensures
                // that only the Missing is reported, and not the Untracked.

                if (seen.Contains(fullPath))
                    continue;

                var e = new GitStatusEventArgs
                {
                    Status = new GitFileStatus(
                        fullPath,
                        GetStatus(state),
                        GetScheduleState(state),
                        state,
                        GitNodeKind.File
                    )
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
