using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;

namespace SharpGit
{
    internal class GitCreateRepositoryCommand : GitCommand<GitCreateRepositoryArgs>
    {
        public GitCreateRepositoryCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            var builder = new RepositoryBuilder();

            builder.SetWorkTree(repositoryPath);
            builder.Setup();

            var repository = builder.Build();

            try
            {
                repository.Create();

                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.UpdateUpdate,
                    ContentState = GitNotifyState.Unknown,
                    FullPath = repositoryPath,
                    NodeKind = GitNodeKind.Directory
                });
            }
            finally
            {
                repository.Close();
            }
        }
    }
}
