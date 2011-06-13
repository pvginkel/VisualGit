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
            if (FileSystemUtil.FileIsBinary(target.TargetName))
                throw new GitClientBinaryFileException();

            var repositoryEntry = Client.GetRepository(target.TargetName);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Blame();

                command.SetFilePath(repository.GetRepositoryPath(target.TargetName));
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
