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

        protected void RaiseMergeResults(RepositoryEntry repositoryEntry, MergeCommandResult mergeResult)
        {
            Debug.Assert(Args is IGitConflictsClientArgs, "Merge results may only be reported on Args that implement IGitConflictsClientArgs");

            var conflicts = mergeResult.GetConflicts();

            if (conflicts == null)
                return;

            foreach (var item in conflicts)
            {
                string fullPath = repositoryEntry.Repository.GetAbsoluteRepositoryPath(item.Key);

                var args = new GitConflictEventArgs
                {
                    MergedFile = fullPath,
                    Path = item.Key,
                    ConflictReason = GitConflictReason.Edited
                };

                try
                {
                    Args.OnConflict(args);

                    if (args.Cancel)
                        return;
                    if (args.Choice == GitAccept.Postpone)
                        continue;

                    using (repositoryEntry.Lock())
                    {
                        var repository = repositoryEntry.Repository;

                        switch (args.Choice)
                        {
                            case GitAccept.Merged:
                                MarkMerged(repository, args);
                                break;

                            case GitAccept.Base:
                                SelectAndMarkMerged(repository, args, args.BaseFile);
                                break;

                            case GitAccept.MineFull:
                                SelectAndMarkMerged(repository, args, args.MyFile);
                                break;

                            case GitAccept.TheirsFull:
                                SelectAndMarkMerged(repository, args, args.TheirFile);
                                break;
                        }
                    }
                }
                finally
                {
                    args.Cleanup();
                }
            }
        }

        private void MarkMerged(Repository repository, GitConflictEventArgs args)
        {
            new GitClient().Resolve(args.MergedFile, GitAccept.Merged, new GitResolveArgs());
        }

        private void SelectAndMarkMerged(Repository repository, GitConflictEventArgs args, string path)
        {
            File.Copy(path, args.MergedFile, true);

            MarkMerged(repository, args);
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
