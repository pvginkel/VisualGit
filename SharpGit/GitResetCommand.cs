using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using System.Diagnostics;

namespace SharpGit
{
    internal class GitResetCommand : GitCommand<GitResetArgs>
    {
        public GitResetCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, GitRevision revision, GitResetType type)
        {
            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Reset();

                command.SetMode(UnpackMode(type));
                command.SetRef(revision.GetObjectId(repository).Name);

                command.Call();
            }
        }

        private ResetCommand.ResetType UnpackMode(GitResetType type)
        {
            switch (type)
            {
                case GitResetType.Hard: return ResetCommand.ResetType.HARD;
                case GitResetType.Mixed: return ResetCommand.ResetType.MIXED;
                case GitResetType.Soft: return ResetCommand.ResetType.SOFT;
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
