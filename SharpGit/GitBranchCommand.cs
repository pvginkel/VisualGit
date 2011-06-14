using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitBranchCommand : GitCommand<GitBranchArgs>
    {
        public GitBranchCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, string branchName)
        {
            if (branchName == null)
                throw new ArgumentNullException("branchName");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).BranchCreate();

                command.SetForce(Args.Force);
                command.SetName(branchName);
                
                if (Args.Revision != null)
                {
                    var revWalk = new RevWalk(repository);

                    try
                    {
                        command.SetStartPoint(revWalk.ParseCommit(Args.Revision.GetObjectId(repository)));
                    }
                    finally
                    {
                        revWalk.Release();
                    }
                }

                command.Call();
            }
        }
    }
}
