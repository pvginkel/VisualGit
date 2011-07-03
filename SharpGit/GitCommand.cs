// SharpGit\GitCommand.cs
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
using System.ComponentModel;
using NGit;
using NGit.Treewalk;
using NGit.Api;
using System.Diagnostics;
using System.IO;

namespace SharpGit
{
    internal abstract class GitCommand
    {
        protected GitCommand(GitClient client, GitClientArgs args)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (args == null)
                throw new ArgumentNullException("args");

            Client = client;
            Args = args;
        }

        public GitClient Client { get; private set; }
        public GitClientArgs Args { get; private set; }

        public void RaiseNotify(GitNotifyEventArgs e)
        {
            Args.OnNotify(e);
            Client.OnNotify(e);
        }

        public void RaiseCommitting(GitCommittingEventArgs e)
        {
            var withCommitArgs = Args as GitClientArgsWithCommit;

            if (withCommitArgs != null)
                withCommitArgs.OnCommitting(e);

            Client.OnCommitting(e);
        }

        public bool CancelRequested()
        {
            return CancelRequested(null);
        }

        public bool CancelRequested(CancelEventArgs cancelEventArgs)
        {
            if (cancelEventArgs == null || !cancelEventArgs.Cancel)
            {
                cancelEventArgs = new CancelEventArgs();

                Args.OnCancel(cancelEventArgs);
            }

            if (cancelEventArgs.Cancel && Args.ThrowOnCancel)
                throw new GitOperationCancelledException();

            return cancelEventArgs.Cancel;
        }

        protected void RaiseNotifyFromDiff(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");
            var workingTreeIt = new FileTreeIterator(repository);
            var diff = new IndexDiff(repository, Constants.HEAD, workingTreeIt);

            diff.Diff();

            RaiseNotifyFromDiff(repository, diff.GetAdded(), GitNotifyAction.UpdateAdd);
            RaiseNotifyFromDiff(repository, diff.GetAssumeUnchanged(), GitNotifyAction.UpdateUpdate);
            RaiseNotifyFromDiff(repository, diff.GetChanged(), GitNotifyAction.UpdateUpdate);
            RaiseNotifyFromDiff(repository, diff.GetConflicting(), GitNotifyAction.UpdateUpdate);
            RaiseNotifyFromDiff(repository, diff.GetMissing(), GitNotifyAction.UpdateDeleted);
            RaiseNotifyFromDiff(repository, diff.GetModified(), GitNotifyAction.UpdateUpdate);
            RaiseNotifyFromDiff(repository, diff.GetRemoved(), GitNotifyAction.UpdateDeleted);
            RaiseNotifyFromDiff(repository, diff.GetUntracked(), GitNotifyAction.UpdateUpdate);
        }

        private void RaiseNotifyFromDiff(Repository repository, ICollection<string> paths, GitNotifyAction action)
        {
            foreach (string path in paths)
            {
                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = action,
                    CommandType = Args.CommandType,
                    ContentState = GitNotifyState.Unknown,
                    FullPath = repository.GetAbsoluteRepositoryPath(path),
                    NodeKind = GitNodeKind.Unknown
                });
            }
        }

        protected void RaiseMergeResults(RepositoryEntry repositoryEntry, MergeCommandResult mergeResults)
        {
            Debug.Assert(Args is IGitConflictsClientArgs, "Merge results may only be reported on Args that implement IGitConflictsClientArgs");

            // We ignore the merge results for getting a list of conflicts.
            // Instead, we go to the index directly.

            var conflicts = new HashSet<string>(FileSystemUtil.StringComparer);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var dirCache = repository.ReadDirCache();

                for (int i = 0, count = dirCache.GetEntryCount(); i < count; i++)
                {
                    var entry = dirCache.GetEntry(i);

                    if (entry.Stage > 0)
                    {
                        string path = entry.PathString;

                        if (!conflicts.Contains(path))
                            conflicts.Add(path);
                    }
                }
            }

            if (conflicts.Count == 0)
                return;

            foreach (var item in conflicts)
            {
                string fullPath = repositoryEntry.Repository.GetAbsoluteRepositoryPath(item);

                var args = new GitConflictEventArgs
                {
                    MergedFile = fullPath,
                    Path = item,
                    ConflictReason = GitConflictReason.Edited
                };

                try
                {
                    Args.OnConflict(args);

                    if (args.Cancel)
                        return;
                    if (args.Choice == GitAccept.Postpone)
                        continue;

                    Client.Resolve(args.MergedFile, args.Choice, new GitResolveArgs
                    {
                        ConflictArgs = args
                    });
                }
                finally
                {
                    args.Cleanup();
                }
            }
        }

        protected Repository CreateDummyRepository()
        {
            var builder = new RepositoryBuilder();

            builder.SetBare();
            builder.SetWorkTree(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            builder.Setup();

            return builder.Build();
        }

        protected IDisposable MoveAway(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (File.Exists(path))
            {
                string tmpPath;

                for (int i = 0; ; i++)
                {
                    tmpPath = path + ".SharpGit." + i + ".tmp";

                    if (!File.Exists(tmpPath))
                        break;
                }

                File.Move(path, tmpPath);

                return new RevertMove(path, tmpPath);
            }
            else
            {
                return null;
            }
        }

        private class RevertMove : IDisposable
        {
            private bool _disposed;
            private string _path;
            private string _tmpPath;

            public RevertMove(string path, string tmpPath)
            {
                _path = path;
                _tmpPath = tmpPath;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (File.Exists(_path))
                        File.Delete(_path);

                    File.Move(_tmpPath, _path);

                    _disposed = true;
                }
            }
        }
    }

    internal abstract class GitCommand<T> : GitCommand
        where T : GitClientArgs
    {
        protected GitCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public new T Args
        {
            get { return (T)base.Args; }
        }
    }
}
