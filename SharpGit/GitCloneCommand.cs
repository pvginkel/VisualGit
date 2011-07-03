using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Api.Errors;

namespace SharpGit
{
    internal class GitCloneCommand : GitTransportCommand<GitCloneArgs>
    {
        public GitCloneCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal void Execute(string remote, GitRef @ref, string destination)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (@ref == null)
                throw new ArgumentNullException("ref");
            if (destination == null)
                throw new ArgumentNullException("destination");

            var command = new CloneCommand();

            command.SetBranch(@ref.Name);
            command.SetProgressMonitor(new ProgressMonitor(this));
            command.SetDirectory(destination);
            command.SetURI(remote);

            using (new CredentialsProviderScope(new CredentialsProvider(this)))
            {
                command.Call();
            }
        }
    }
}
