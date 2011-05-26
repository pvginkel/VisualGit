using System;
using VisualGit.UI.RepositoryExplorer;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to remove a URL from the Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.RemoveRepositoryRoot, AlwaysAvailable=true)]
    class RemoveRepositoryRootCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool enabled = false;
            RepositoryExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as RepositoryExplorerControl;
            if (ctrl != null)
            {
                Uri uri = ctrl.SelectedUri;
                enabled = uri != null;
            }
            e.Enabled = enabled;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            RepositoryExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as RepositoryExplorerControl;
            ctrl.RemoveRootOf(ctrl.SelectedUri);
        }
    }
}
