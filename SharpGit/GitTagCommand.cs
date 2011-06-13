using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Revwalk;
using NGit;

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

                var revWalk = new RevWalk(repository);
                try
                {
                    var revision = Args.Revision ?? GitRevision.Head;
                    var commit = revWalk.ParseCommit(revision.GetObjectId(repository));


                    if (Args.AnnotatedTag)
                    {
                        var command = new Git(repository).Tag();

                        command.SetMessage(Args.Message);
                        command.SetForceUpdate(Args.Force);
                        command.SetName(tagName);

                        command.SetObjectId(commit);

                        command.Call();
                    }
                    else
                    {
                        // TagCommand is not smart enough to support not annotated
                        // tags. Create a tag ourself.

                        var tagRef = repository.UpdateRef(Constants.R_TAGS + tagName);

                        tagRef.SetNewObjectId(commit.ToObjectId());
                        tagRef.SetForceUpdate(Args.Force);
                        tagRef.SetRefLogMessage("tagged " + tagName, false);

                        tagRef.Update(revWalk);
                    }
                }
                finally
                {
                    revWalk.Release();
                }
            }
        }
    }
}
