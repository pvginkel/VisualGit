using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NGit;
using NGit.Api;
using NGit.Treewalk;
using NGit.Diff;
using NGit.Treewalk.Filter;
using NGit.Revwalk;

namespace SharpGit
{
    internal sealed class GitLogCommand : GitCommand<GitLogArgs>
    {
        public GitLogCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal void Execute(IEnumerable<Uri> uris)
        {
            var paths = new List<string>();

            foreach (var uri in uris)
            {
                paths.Add(GitTools.GetAbsolutePath(uri));
            }

            var collectedPaths = RepositoryUtil.CollectPaths(paths);

            if (collectedPaths.Count == 0)
                return;
            else if (collectedPaths.Count > 1)
                throw new GitException(GitErrorCode.UnexpectedMultipleRepositories);

            var collectedPathsEntry = collectedPaths.Single();

            using (collectedPathsEntry.Key.Lock())
            {
                ExecuteLog(collectedPathsEntry.Key.Repository, collectedPathsEntry.Value);
            }
        }

        internal void Execute(string repositoryPath)
        {
            var repositoryEntry = Client.GetRepository(repositoryPath);

            // We do not lock the repository for log. Git itself also doesn't
            // log it, and because we do logs in the background, this gives major
            // problems on large logs.

            // using (repositoryEntry.Lock())
            {
                ExecuteLog(repositoryEntry.Repository, null);
            }
        }

        private void ExecuteLog(Repository repository, ICollection<string> paths)
        {
            if (Args.RetrieveMergedRevisions)
                throw new NotImplementedException();

            var revWalk = new RevWalk(repository);

            try
            {
                if (Args.Start == null)
                {
                    revWalk.MarkStart(revWalk.LookupCommit(repository.Resolve(Constants.HEAD)));
                }
                else
                {
                    revWalk.MarkStart(revWalk.LookupCommit(Args.Start.GetObjectId(repository)));

                    if (Args.End != null)
                    {
                        var end = Args.End.GetObjectId(repository);

                        if (end != null)
                            revWalk.MarkUninteresting(revWalk.LookupCommit(end));
                    }
                }

                if (paths != null && paths.Count > 0)
                {
                    var pathsToAdd = new List<string>();

                    foreach (string path in paths)
                    {
                        if (!String.IsNullOrEmpty(path))
                            pathsToAdd.Add(path);
                    }

                    if (pathsToAdd.Count > 0)
                        revWalk.SetTreeFilter(AndTreeFilter.Create(PathFilterGroup.CreateFromStrings(pathsToAdd), TreeFilter.ANY_DIFF));
                }

                int count = Args.Limit;

                var refs = repository.GetAllRefs().Select(p => new GitRef(p.Value));

                foreach (var commit in revWalk)
                {
                    var authorIdent = commit.GetAuthorIdent();

                    var parents = new string[commit.Parents.Length];

                    for (int i = 0; i < commit.Parents.Length; i++)
                    {
                        parents[i] = commit.Parents[i].Id.Name;
                    }

                    string commitId = commit.Id.Name;

                    var e = new GitLogEventArgs
                    {
                        Author = authorIdent.GetName() + " <" + authorIdent.GetEmailAddress() + ">",
                        AuthorTime = authorIdent.GetWhen(),
                        AuthorName = authorIdent.GetName(),
                        AuthorEmail = authorIdent.GetEmailAddress(),
                        LogMessage = commit.GetFullMessage(),
                        Revision = commitId,
                        Time = GitTools.CreateDateFromGitTime(commit.CommitTime),
                        ParentRevisions = parents,
                        Refs = refs.Where(p => p.Revision == commitId).ToArray()
                    };

                    if (Args.RetrieveChangedPaths)
                    {
                        if (commit.Parents.Length == 1)
                        {
                            var diffFormatter = new DiffFormatter(NullOutputStream.Instance);

                            diffFormatter.SetRepository(repository);

                            e.ChangedPaths = new GitChangeItemCollection();

                            foreach (var diffEntry in diffFormatter.Scan(commit.Tree, commit.Parents[0].Tree))
                            {
                                e.ChangedPaths.Add(new GitChangeItem
                                {
                                    Path = GetPath(repository, diffEntry),
                                    OldRevision = GetOldRevision(diffEntry, commit),
                                    OldPath = GetOldPath(repository, diffEntry),
                                    NodeKind = GitNodeKind.File,
                                    Action = GetChangeType(diffEntry.GetChangeType())
                                });
                            }
                        }
                    }

                    Args.OnLog(e);

                    if (--count == 0 || CancelRequested(e))
                        return;
                }
            }
            finally
            {
                revWalk.Release();
            }
        }

        private string GetOldRevision(DiffEntry diffEntry, RevCommit commit)
        {
            switch (diffEntry.GetChangeType())
            {
                case DiffEntry.ChangeType.COPY:
                case DiffEntry.ChangeType.RENAME:
                case DiffEntry.ChangeType.MODIFY:
                    return commit.Parents[0].Id.Name;

                default:
                    return null;
            }
        }

        private string GetPath(Repository repository, DiffEntry diffEntry)
        {
            string path;

            switch (diffEntry.GetChangeType())
            {
                case DiffEntry.ChangeType.ADD:
                case DiffEntry.ChangeType.COPY:
                case DiffEntry.ChangeType.MODIFY:
                case DiffEntry.ChangeType.RENAME:
                    path = diffEntry.GetPath(DiffEntry.Side.NEW);
                    break;

                default:
                    path = diffEntry.GetPath(DiffEntry.Side.OLD);
                    break;
            }

            return repository.GetAbsoluteRepositoryPath(path);
        }

        private string GetOldPath(Repository repository, DiffEntry diffEntry)
        {
            switch (diffEntry.GetChangeType())
            {
                case DiffEntry.ChangeType.COPY:
                case DiffEntry.ChangeType.RENAME:
                    return repository.GetAbsoluteRepositoryPath(diffEntry.GetPath(DiffEntry.Side.OLD));

                default:
                    return null;
            }
        }

        private GitChangeAction GetChangeType(DiffEntry.ChangeType changeType)
        {
            switch (changeType)
            {
                case DiffEntry.ChangeType.ADD: return GitChangeAction.Add;
                case DiffEntry.ChangeType.COPY: return GitChangeAction.Copy;
                case DiffEntry.ChangeType.DELETE: return GitChangeAction.Delete;
                case DiffEntry.ChangeType.MODIFY: return GitChangeAction.Modify;
                case DiffEntry.ChangeType.RENAME: return GitChangeAction.Rename;
                default: throw new ArgumentOutOfRangeException("changeType");
            }
        }
    }
}
