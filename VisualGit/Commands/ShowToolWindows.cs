using VisualGit.UI;
using VisualGit.VS;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command implementation of the show toolwindow commands
    /// </summary>
    [Command(VisualGitCommand.ShowPendingChanges)]
    [Command(VisualGitCommand.ShowWorkingCopyExplorer)]
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
                default:
                    return;
            }

            package.ShowToolWindow(toolWindow);
        }
    }
}
