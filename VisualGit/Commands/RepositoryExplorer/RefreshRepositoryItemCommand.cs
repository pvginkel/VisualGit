using System;
using VisualGit.Scc;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to refresh the current item in the Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.RefreshRepositoryItem, AlwaysAvailable=true)]
    class RefreshRepositoryItemCommand : CommandBase
    {
        
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (IGitRepositoryItem it in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                if (it.Origin != null)
                    return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            foreach (IGitRepositoryItem it in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                if (it.Origin != null)
                    it.RefreshItem(false);
            }
        }
    }
}
