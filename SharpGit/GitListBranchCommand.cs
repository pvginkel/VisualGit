using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;

namespace SharpGit
{
    internal sealed class GitListBranchCommand : GitCommand<GitListBranchArgs>
    {
        public GitListBranchCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitListBranchResult Execute(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            var repositoryEntry = RepositoryManager.GetRepository(repositoryPath);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var listCommand = new Git(repository).BranchList();

                listCommand.SetListMode(
                    Args.RetrieveRemoteOnly
                    ? ListBranchCommand.ListMode.REMOTE
                    : ListBranchCommand.ListMode.ALL
                );

                var result = new GitListBranchResult();

                foreach (var @ref in listCommand.Call())
                {
                    result.Branches.Add(new GitBranchRef(@ref.GetName()));
                }

                return result;
            }
        }
    }
}
