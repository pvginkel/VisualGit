using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using System.IO;

namespace SharpGit
{
    internal sealed class GitDeleteCommand : GitCommand<GitDeleteArgs>
    {
        public GitDeleteCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string path)
        {
            var repositoryEntry = Client.GetRepository(path);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                if (!Args.KeepLocal && File.Exists(path))
                    File.Delete(path);

                var command = new Git(repository).Rm();

                command.AddFilepattern(repository.GetRepositoryPath(path));

                // DeleteCommand does not have an option to keep the local file
                // to be deleted. Temporarily move it away so the DeleteCommand
                // won't delete it.

                IDisposable moveAwayHandle = null;

                if (Args.KeepLocal && File.Exists(path))
                    moveAwayHandle = MoveAway(path);

                try
                {
                    command.Call();
                }
                finally
                {
                    if (moveAwayHandle != null)
                        moveAwayHandle.Dispose();
                }

                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.Delete,
                    CommandType = Args.CommandType,
                    FullPath = path,
                    NodeKind = GitNodeKind.File,
                    ContentState= GitNotifyState.None
                });
            }
        }
    }
}
