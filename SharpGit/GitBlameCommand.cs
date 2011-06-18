using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Diff;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitBlameCommand : GitCommand<GitBlameArgs>
    {
        public GitBlameCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitBlameResult Execute(GitTarget target)
        {
            string fullPath;
            if (target is GitPathTarget)
                fullPath = ((GitPathTarget)target).FullPath;
            else if (target is GitUriTarget)
                fullPath = GitTools.GetAbsolutePath(((GitUriTarget)target).Uri);
            else
                throw new NotSupportedException();

            if (FileSystemUtil.FileIsBinary(fullPath))
                throw new GitClientBinaryFileException();

            var repositoryEntry = Client.GetRepository(fullPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Blame();

                command.SetFilePath(repository.GetRepositoryPath(fullPath));
                command.SetFollowFileRenames(true);

                if (Args.End != null)
                    command.SetStartCommit(Args.End.GetObjectId(repository));

                switch (Args.IgnoreSpacing)
                {
                    case GitIgnoreSpacing.IgnoreAll:
                        command.SetTextComparator(RawTextComparator.WS_IGNORE_ALL);
                        break;

                    case GitIgnoreSpacing.IgnoreSpace:
                        command.SetTextComparator(IgnoreSpaceTextComparator.Instance);
                        break;
                }

                var result = command.Call();

                var commandResult = new GitBlameResult();
                var resultContents = result.GetResultContents();

                for (int i = 0, count = resultContents.Size(); i < count; i++)
                {
                    var commit = result.GetSourceCommit(i);

                    commandResult.Items.Add(new GitBlameEventArgs
                    {
                        Author = result.GetSourceAuthor(i).GetName(),
                        LineNumber = i,
                        Line = resultContents.GetString(i),
                        Revision = commit.Name,
                        Time = GitTools.CreateDateFromGitTime(commit.CommitTime)
                    });
                }

                return commandResult;
            }
        }
    }
}
