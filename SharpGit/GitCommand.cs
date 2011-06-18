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
