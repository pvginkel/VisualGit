using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Api.Errors;

namespace SharpGit
{
    internal sealed class GitSwitchCommand : GitCommand<GitSwitchArgs>
    {
        public GitSwitchCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        internal GitSwitchResult Execute(GitRef target, string repositoryPath)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var result = new GitSwitchResult();

                var checkoutCommand = new Git(repositoryEntry.Repository).Checkout();

                checkoutCommand.SetName(target.Name);
                checkoutCommand.SetForce(Args.Force);

                try
                {
                    checkoutCommand.Call();
                }
                catch (JGitInternalException ex)
                {
                    var exception = new GitException(GitErrorCode.CheckoutFailed, ex);

                    Args.SetError(exception);

                    result.PostSwitchError = ex.Message;

                    if (Args.ShouldThrow(exception.ErrorCode))
                        throw exception;
                }

                RaiseNotifyFromDiff(repositoryEntry.Repository);

                return result;
            }
        }
    }
}
