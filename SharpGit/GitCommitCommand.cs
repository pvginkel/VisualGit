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

            var collectedPaths = RepositoryUtil.CollectPaths(paths);

            if (collectedPaths.Count == 0)
                throw new GitNoRepositoryException();
            else if (collectedPaths.Count > 1)
                throw new GitException(GitErrorCode.UnexpectedMultipleRepositories);

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
                        result.PostCommitError = sb.ToString();

                        return result;
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

                    try
                    {
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
                    catch (JGitInternalException ex)
                    {
                        result.PostCommitError = ex.Message;
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
