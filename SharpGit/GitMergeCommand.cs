using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Merge;

namespace SharpGit
{
    internal class GitMergeCommand : GitCommand<GitMergeArgs>
    {
        public GitMergeCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, GitRef branch)
        {
            if (branch == null)
                throw new ArgumentNullException("branch");

            MergeCommandResult mergeResult;

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).Merge();

                switch (Args.Strategy)
                {
                    case GitMergeStrategy.Ours:
                        command.SetStrategy(MergeStrategy.OURS);
                        break;

                    case GitMergeStrategy.Theirs:
                        command.SetStrategy(MergeStrategy.THEIRS);
                        break;

                    case GitMergeStrategy.SimpleTwoWayInCore:
                        command.SetStrategy(MergeStrategy.SIMPLE_TWO_WAY_IN_CORE);
                        break;

                    case GitMergeStrategy.Resolve:
                        command.SetStrategy(MergeStrategy.RESOLVE);
                        break;
                }

                command.Include(repository.Resolve(branch.Name));

                mergeResult = command.Call();

                RaiseNotifyFromDiff(repositoryEntry.Repository);
            }

            // Conflict resolving is run outside of the repository lock
            // because it calls back into VS code.

            RaiseMergeResults(repositoryEntry, mergeResult);
        }
    }
}
