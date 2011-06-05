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

        internal GitPushResult Execute(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            var repositoryEntry = RepositoryManager.GetRepository(repositoryPath);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

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

                    pushCommand.SetProgressMonitor(new ProgressMonitory(this));
                    pushCommand.SetCredentialsProvider(new CredentialsProvider(this));

                    var result = new GitPushResult();

                    try
                    {
                        pushCommand.Call();
                    }
                    catch (JGitInternalException ex)
                    {
                        var exception = new GitException(GitErrorCode.PushFailed, ex);

                        Args.SetError(exception);

                        result.PostPushError = ex.Message;

                        if (Args.ShouldThrow(exception.ErrorCode))
                            throw exception;
                    }

                    return result;
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
