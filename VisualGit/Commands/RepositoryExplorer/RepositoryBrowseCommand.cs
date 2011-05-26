using System;
using VisualGit.UI;
using VisualGit.UI.RepositoryExplorer;
using System.Windows.Forms;
using VisualGit.Scc;
using SharpSvn;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to add a new URL to the Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.RepositoryBrowse, ArgumentDefinition = "u|d", AlwaysAvailable = true)]
    class RepositoryBrowseCommand : CommandBase
    {
        public override void OnExecute(CommandEventArgs e)
        {
            IUIShell shell = e.GetService<IUIShell>();
            Uri info;

            if (e.Argument is string)
            {
                string arg = (string)e.Argument;

                info = null;
                if (GitItem.IsValidPath(arg, true))
                {
                    GitItem item = e.GetService<IFileStatusCache>()[arg];

                    if (item.IsVersioned)
                    {
                        info = item.Uri;

                        if (item.IsFile)
                            info = new Uri(info, "./");
                    }
                }

                if (info == null)
                    info = new Uri((string)e.Argument);
            }
            else if (e.Argument is Uri)
                info = (Uri)e.Argument;
            else
                using (AddRepositoryRootDialog dlg = new AddRepositoryRootDialog())
                {
                    if (dlg.ShowDialog(e.Context) != DialogResult.OK || dlg.Uri == null)
                        return;

                    info = dlg.Uri;
                }

            if (info != null)
            {
                RepositoryExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as RepositoryExplorerControl;

                if (ctrl == null)
                {
                    IVisualGitPackage pkg = e.GetService<IVisualGitPackage>();
                    pkg.ShowToolWindow(VisualGitToolWindow.RepositoryExplorer);
                }

                ctrl = e.Selection.ActiveDialogOrFrameControl as RepositoryExplorerControl;

                if (ctrl != null)
                    ctrl.AddRoot(info);
            }
        }
    }
}
