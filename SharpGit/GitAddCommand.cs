using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;

namespace SharpGit
{
    internal sealed class GitAddCommand : GitCommand<GitAddArgs>
    {
        public GitAddCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var repositoryEntry = RepositoryManager.GetRepository(path);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            using (repositoryEntry.Lock())
            {
                var addCommand = new Git(repositoryEntry.Repository).Add();

                addCommand.AddFilepattern(repositoryEntry.Repository.GetRepositoryPath(path));

                addCommand.Call();

                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.Add,
                    CommandType = Args.CommandType,
                    ContentState = GitNotifyState.Unknown,
                    FullPath = path,
                    NodeKind = GitNodeKind.File
                });
            }
        }
    }
}
