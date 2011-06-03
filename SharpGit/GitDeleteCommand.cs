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
            var repositoryEntry = RepositoryManager.GetRepository(path);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                if (!Args.KeepLocal && File.Exists(path))
                    File.Delete(path);

                var command = new Git(repository).Rm();

                command.AddFilepattern(repository.GetRepositoryPath(path));

                command.Call();

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
