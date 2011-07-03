using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Api.Errors;
using NGit;

namespace SharpGit
{
    internal class GitPushCommand : GitTransportCommand<GitPushArgs>
    {
        public GitPushCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal void Execute(string repositoryPath)
        {
            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                Repository repository = null;

                try
                {
                    repository = repositoryEntry.Repository;

                    var pushCommand = new Git(repository).Push();

                    if (Args.AllBranches || Args.AllTags)
                    {
                        if (Args.AllBranches)
                            pushCommand.SetPushAll();
                        if (Args.AllTags)
                            pushCommand.SetPushTags();
                    }
                    else if (Args.LocalBranch == null && Args.Tag == null)
                    {
                        pushCommand.Add(repository.GetFullBranch());
                    }
                    else
                    {
                        if (Args.LocalBranch != null)
                            pushCommand.Add(Args.LocalBranch.ShortName);
                        if (Args.Tag != null)
                            pushCommand.Add(Args.Tag.ShortName);
                    }

                    pushCommand.SetForce(Args.Force);

                    if (Args.Remote != null)
                        pushCommand.SetRemote(Args.Remote);
                    else if (Args.RemoteUri != null)
                        pushCommand.SetRemote(Args.RemoteUri);

                    pushCommand.SetProgressMonitor(new ProgressMonitor(this));

                    using (new CredentialsProviderScope(new CredentialsProvider(this)))
                    {
                        pushCommand.Call();
                    }
                }
                finally
                {
                    if (repository != null)
                        repository.Close();
                }
            }
        }
    }
}
