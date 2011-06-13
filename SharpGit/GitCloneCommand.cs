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

        internal GitCloneResult Execute(string remote, GitRef @ref, string destination)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (@ref == null)
                throw new ArgumentNullException("ref");
            if (destination == null)
                throw new ArgumentNullException("destination");

            var command = new CloneCommand();

            command.SetBranch(@ref.Name);
            command.SetCredentialsProvider(new CredentialsProvider(this));
            command.SetProgressMonitor(new ProgressMonitor(this));
            command.SetDirectory(destination);
            command.SetURI(remote);

            var result = new GitCloneResult();

            try
            {
                command.Call();
            }
            catch (JGitInternalException ex)
            {
                var exception = new GitException(GitErrorCode.CloneFailed, ex);

                Args.SetError(exception);

                result.PostCloneError = ex.Message;

                if (Args.ShouldThrow(exception.ErrorCode))
                    throw exception;
            }

            return result;
        }
    }
}
