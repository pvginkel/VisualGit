using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitTagCommand : GitCommand<GitTagArgs>
    {
        public GitTagCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, string tagName)
        {
            if (tagName == null)
                throw new ArgumentNullException("tagName");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Tag();

                if (Args.Message != null)
                    command.SetMessage(Args.Message);

                command.SetForceUpdate(Args.Force);
                command.SetName(tagName);

                var revision = Args.Revision ?? GitRevision.Head;
                var revWalk = new RevWalk(repository);

                try
                {
                    // TODO: NGit does not register the tag on the commit id.
                    // May need to report a bug.

                    command.SetObjectId(revWalk.ParseCommit(revision.GetObjectId(repository)));
                }
                finally
                {
                    revWalk.Release();
                }

                command.Call();
            }
        }
    }
}
