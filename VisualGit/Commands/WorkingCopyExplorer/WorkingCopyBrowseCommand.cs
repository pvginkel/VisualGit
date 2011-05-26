using System;
using VisualGit.UI;
using VisualGit.UI.WorkingCopyExplorer;
using System.Windows.Forms;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to add a new root to the Working Copy Explorer.
    /// </summary>
    [Command(VisualGitCommand.WorkingCopyBrowse, ArgumentDefinition="d")]
    [Command(VisualGitCommand.WorkingCopyAdd, ArgumentDefinition = "d")]
    class AddWorkingCopyExplorerRootCommand : CommandBase
    {
        public override void OnExecute(CommandEventArgs e)
        {
            string info;

            if (e.Argument is string)
            {
                // Allow opening from
                info = (string)e.Argument;
            }
            else if (e.Command == VisualGitCommand.WorkingCopyAdd)
            {
                using (AddWorkingCopyExplorerRootDialog dlg = new AddWorkingCopyExplorerRootDialog())
                {
                    DialogResult dr = dlg.ShowDialog(e.Context);

                    if (dr != DialogResult.OK || string.IsNullOrEmpty(dlg.NewRoot))
                        return;

                    info = dlg.NewRoot;
                }
            }
            else
                throw new InvalidOperationException("WorkingCopyBrowse was called without a path");

            if (!string.IsNullOrEmpty(info))
            {
                WorkingCopyExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as WorkingCopyExplorerControl;

                if (ctrl == null)
                {
                    IVisualGitPackage pkg = e.GetService<IVisualGitPackage>();
                    pkg.ShowToolWindow(VisualGitToolWindow.WorkingCopyExplorer);
                }

                ctrl = e.Selection.ActiveDialogOrFrameControl as WorkingCopyExplorerControl;

                if (ctrl != null)
                {
                    switch (e.Command)
                    {
                        case VisualGitCommand.WorkingCopyAdd:
                            ctrl.AddRoot(info);
                            break;
                        case VisualGitCommand.WorkingCopyBrowse:
                            ctrl.BrowsePath(e.Context, info);
                            break;
                    }
                }
            }
        }
    }
}
