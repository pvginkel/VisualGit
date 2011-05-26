using VisualGit.UI;
using VisualGit.UI.RepositoryExplorer;
using VisualGit.VS;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command implementation of the show toolwindow commands
    /// </summary>
    [Command(VisualGitCommand.ShowPendingChanges)]
    [Command(VisualGitCommand.ShowWorkingCopyExplorer)]
    [Command(VisualGitCommand.ShowGitInfo)]
    [Command(VisualGitCommand.ShowRepositoryExplorer, AlwaysAvailable=true)]
    class ShowToolWindows : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            switch (e.Command)
            {
                case VisualGitCommand.ShowPendingChanges:
                    if (!e.State.SccProviderActive)
                        e.Visible = e.Enabled = false;
                    break;
            }
        }
        public override void OnExecute(CommandEventArgs e)
        {
            IVisualGitPackage package = e.Context.GetService<IVisualGitPackage>();

            VisualGitToolWindow toolWindow;
            switch (e.Command)
            {
                case VisualGitCommand.ShowPendingChanges:
                    toolWindow = VisualGitToolWindow.PendingChanges;
                    break;
                case VisualGitCommand.ShowWorkingCopyExplorer:
                    toolWindow = VisualGitToolWindow.WorkingCopyExplorer;
                    break;
                case VisualGitCommand.ShowRepositoryExplorer:
                    toolWindow = VisualGitToolWindow.RepositoryExplorer;
                    break;
                case VisualGitCommand.ShowGitInfo:
                    toolWindow = VisualGitToolWindow.GitInfo;
                    break;
                default:
                    return;
            }

            package.ShowToolWindow(toolWindow);

            if (e.Command == VisualGitCommand.ShowRepositoryExplorer)
            {
                IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();

                if (ss.ProjectRootUri != null)
                {
                    RepositoryExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as RepositoryExplorerControl;

                    if (ctrl != null)
                        ctrl.AddRoot(ss.ProjectRootUri);
                }
            }
        }
    }
}
