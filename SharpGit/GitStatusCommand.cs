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
using System.IO;

namespace SharpGit
{
    internal sealed class GitStatusCommand : GitCommand<GitStatusArgs>
    {
        private HashSet<string> _seen;
        private EventHandler<GitStatusEventArgs> _callback;
        private Repository _repository;
        private string _relativePath;

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
                throw new InvalidOperationException("Expected GitDepth to be greater than or equal to Files");

            using (repositoryEntry.Lock())
            {
                _seen = new HashSet<string>(FileSystemUtil.StringComparer);
                _callback = callback;
                _repository = repositoryEntry.Repository;

                _relativePath = _repository.GetRepositoryPath(path);

                var diff = new IndexDiff(_repository, Constants.HEAD, new FileTreeIterator(_repository));

                diff.SetFilter(new CustomPathFilter(_relativePath, Args.Depth));

                diff.Diff();

                if (GetNodeKind(path, diff) == GitNodeKind.File)
                    throw new NotImplementedException();

                ExecuteDirectory(path, diff);
            }
        }

        private void ExecuteDirectory(string path, IndexDiff diff)
        {
            bool cancelled;

            ReportDirectory(path, out cancelled);

            if (cancelled)
                return;

            if (Args.Depth > GitDepth.Files)
            {
                foreach (string fullPath in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    ReportDirectory(fullPath, out cancelled);

                    if (cancelled)
                        return;
                }
            }

            foreach (string fullPath in Directory.GetFiles(path, "*", Args.Depth > GitDepth.Files ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                string relativePath = _repository.GetRepositoryPath(fullPath);

                var state = GetInternalStatus(relativePath, diff);

                ReportFile(
                    new GitStatusEventArgs
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
                    },
                    out cancelled
                );

                if (cancelled)
                    return;
            }

            AddUnseenFiles(diff.GetRemoved(), GitInternalStatus.Removed, out cancelled);

            if (cancelled)
                return;

            AddUnseenFiles(diff.GetUntracked(), GitInternalStatus.Untracked, out cancelled);

            if (cancelled)
                return;

            if (Args.RetrieveIgnoredEntries)
                AddUnseenFiles(diff.GetIgnoredNotInIndex(), GitInternalStatus.Ignored, out cancelled);
        }

        private GitNodeKind GetNodeKind(string path, IndexDiff diff)
        {
            foreach (string pathString in diff.GetRemoved())
            {
                if (String.Equals(pathString, _relativePath, FileSystemUtil.StringComparison))
                    return GitNodeKind.File;
                if (GitTools.PathMatches(_relativePath, pathString, false, Args.Depth))
                    return GitNodeKind.Directory;
            }

            if (Directory.Exists(path))
                return GitNodeKind.Directory;
            else if (File.Exists(path))
                return GitNodeKind.File;

            return GitNodeKind.Unknown;

            throw new InvalidOperationException("Could not determine node kind");
        }

        private void ReportDirectory(string path, out bool cancelled)
        {
            var e = new GitStatusEventArgs
            {
                FullPath = path,
                LocalContentStatus = GetStatus(GitInternalStatus.Unset),
                NodeKind = GitNodeKind.Directory,
                WorkingCopyInfo = new GitWorkingCopyInfo
                {
                    NodeKind = GitNodeKind.Directory,
                    Schedule = GetScheduleState(GitInternalStatus.Unset)
                }
            };

            _callback(Client, e);

            cancelled = CancelRequested(e);
        }

        private void ReportFile(GitStatusEventArgs e, out bool cancelled)
        {
            if (_seen.Contains(e.FullPath))
            {
                cancelled = false;
                return;
            }

            _seen.Add(e.FullPath);

            _callback(Client, e);

            cancelled = CancelRequested(e);
        }

        private void AddUnseenFiles(IEnumerable<string> paths, GitInternalStatus state, out bool cancelled)
        {
            cancelled = false;

            foreach (string deletedPath in paths)
            {
                if (!GitTools.PathMatches(_relativePath, deletedPath, false, Args.Depth))
                    continue;

                string fullPath = _repository.GetAbsoluteRepositoryPath(deletedPath);

                ReportFile(
                    new GitStatusEventArgs
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
                    },
                    out cancelled
                );

                if (cancelled)
                    return;
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

        private GitInternalStatus GetInternalStatus(string relativePath, IndexDiff diff)
        {
            GitInternalStatus result = GitInternalStatus.Unset;

            if (diff.GetConflicting().Contains(relativePath))
                result |= GitInternalStatus.Conflicted;
            if (diff.GetAdded().Contains(relativePath))
                result |= GitInternalStatus.Added;
            if (diff.GetAssumeUnchanged().Contains(relativePath))
                result |= GitInternalStatus.AssumeUnchanged;
            if (diff.GetChanged().Contains(relativePath))
                result |= GitInternalStatus.Changed;
            if (diff.GetModified().Contains(relativePath))
                result |= GitInternalStatus.Modified;
            if (diff.GetMissing().Contains(relativePath))
                result |= GitInternalStatus.Missing;
            if (diff.GetRemoved().Contains(relativePath))
                result |= GitInternalStatus.Removed;
            if (diff.GetUntracked().Contains(relativePath))
                result |= GitInternalStatus.Untracked;

            return result;
        }
    }
}
