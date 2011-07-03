// SharpGit\GitCommitCommand.cs
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
using NGit.Api;
using NGit.Treewalk;
using NGit;
using NGit.Api.Errors;

namespace SharpGit
{
    internal sealed class GitCommitCommand : GitCommand<GitCommitArgs>
    {
        public GitCommitCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitCommitResult Execute(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            var collectedPaths = GitTools.CollectPaths(paths);

            if (collectedPaths.Count == 0)
                throw new GitNoRepositoryException();
            else if (collectedPaths.Count > 1)
                throw new GitUnexpectedMultipleRepositoriesException(Properties.Resources.UnexpectedMultipleRepositories);

            var result = new GitCommitResult();

            foreach (var item in collectedPaths)
            {
                using (item.Key.Lock())
                {
                    var repository = item.Key.Repository;

                    // Calculate the notification actions.

                    var notifyActions = CalculateDiff(item.Value, repository);

                    // Verify all files were staged.

                    var sb = new StringBuilder();

                    foreach (var notifyAction in notifyActions)
                    {
                        if (notifyAction.Value == GitNotifyAction.Unknown)
                        {
                            sb.AppendLine(String.Format(
                                Properties.Resources.CommittingUnstagedFileX, notifyAction.Key
                            ));
                        }
                    }

                    if (sb.Length > 0)
                    {
                        throw new GitUnstagedFileCommitException(sb.ToString());
                    }

                    // Prepare the commit command.

                    var commitCommand = new Git(repository).Commit();

                    var e = new GitCommittingEventArgs
                    {
                        CurrentCommandType = Args.CommandType,
                        LogMessage = Args.LogMessage
                    };

                    foreach (string path in item.Value)
                    {
                        commitCommand.SetOnly(path);

                        e.Items.Add(new GitCommitItem
                        {
                            FullPath = repository.GetAbsoluteRepositoryPath(path)
                        });
                    }

                    // Tell te client we're about to commit.

                    RaiseCommitting(e);

                    if (CancelRequested(e))
                        return null;

                    commitCommand.SetMessage(Args.LogMessage);

                    if (Args.AmendLastCommit)
                        commitCommand.SetAmend(true);

                    var commit = commitCommand.Call();

                    result.Revision = new GitRevision(commit.Id.Name);

                    foreach (var commitAction in notifyActions)
                    {
                        RaiseNotify(new GitNotifyEventArgs
                        {
                            Action = commitAction.Value,
                            CommandType = Args.CommandType,
                            ContentState = GetContentState(commitAction.Value),
                            FullPath = repository.GetAbsoluteRepositoryPath(commitAction.Key),
                            NodeKind = GitNodeKind.File
                        });
                    }
                }
            }

            return result;
        }

        private GitNotifyState GetContentState(GitNotifyAction action)
        {
            switch (action)
            {
                case GitNotifyAction.CommitModified:
                    return GitNotifyState.Changed;

                default:
                    return GitNotifyState.Unknown;
            }
        }

        private Dictionary<string, GitNotifyAction> CalculateDiff(IEnumerable<string> paths, Repository repository)
        {
            var workingTreeIt = new FileTreeIterator(repository);
            var diff = new IndexDiff(repository, Constants.HEAD, workingTreeIt);

            diff.Diff();

            var notifyActions = new Dictionary<string, GitNotifyAction>(FileSystemUtil.StringComparer);

            foreach (string path in paths)
            {
                notifyActions.Add(path, GitNotifyAction.Unknown);
            }

            foreach (string path in diff.GetAdded())
            {
                if (notifyActions.ContainsKey(path))
                    notifyActions[path] = GitNotifyAction.CommitAdded;
            }

            foreach (string path in diff.GetChanged())
            {
                if (notifyActions.ContainsKey(path))
                    notifyActions[path] = GitNotifyAction.CommitModified;
            }

            foreach (string path in diff.GetModified())
            {
                if (notifyActions.ContainsKey(path))
                    notifyActions[path] = GitNotifyAction.CommitModified;
            }

            foreach (string path in diff.GetRemoved())
            {
                if (notifyActions.ContainsKey(path))
                    notifyActions[path] = GitNotifyAction.CommitDeleted;
            }

            return notifyActions;
        }
    }
}
