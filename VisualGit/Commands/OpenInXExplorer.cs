using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemSelectInWorkingCopyExplorer)]
    [Command(VisualGitCommand.ItemSelectInRepositoryExplorer)]
    [Command(VisualGitCommand.ItemSelectInSolutionExplorer)]
    class OpenInXExplorer : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            SvnItem node = EnumTools.GetSingle(e.Selection.GetSelectedSvnItems(false));

            if (node == null && e.Selection.IsSingleNodeSelection)
                node = EnumTools.GetFirst(e.Selection.GetSelectedSvnItems(false));

            bool enable = true;
            if (node == null)
                enable = false;
            else if (e.Command == VisualGitCommand.ItemSelectInRepositoryExplorer)
                enable = node.Uri != null;
            else if (e.Command == VisualGitCommand.ItemSelectInWorkingCopyExplorer)
                enable = node.Exists;
            else if (e.Command == VisualGitCommand.ItemSelectInSolutionExplorer)
            {
                if (e.Selection.IsSolutionExplorerSelection)
                    e.Visible = enable = false;
                else if (!node.InSolution)
                    enable = false;
            }

            if (!enable)
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            SvnItem node = EnumTools.GetFirst(e.Selection.GetSelectedSvnItems(false));

            IVisualGitCommandService cmd = e.GetService<IVisualGitCommandService>();
            switch (e.Command)
            {
                case VisualGitCommand.ItemSelectInRepositoryExplorer:
                    if (node == null || node.Uri == null)
                        return;

                    if (cmd != null)
                        cmd.DirectlyExecCommand(VisualGitCommand.RepositoryBrowse, node.FullPath);
                    break;
                case VisualGitCommand.ItemSelectInWorkingCopyExplorer:
                    if (node == null || !node.Exists)
                        return;

                    if (cmd != null)
                        cmd.DirectlyExecCommand(VisualGitCommand.WorkingCopyBrowse, node.FullPath);
                    break;
                case VisualGitCommand.ItemSelectInSolutionExplorer:
                    if (node == null)
                        return;

                    IVsUIHierarchyWindow hierWindow = VsShellUtilities.GetUIHierarchyWindow(e.Context, new Guid(ToolWindowGuids80.SolutionExplorer));

                    IVsProject project = VsShellUtilities.GetProject(e.Context, node.FullPath) as IVsProject;

                    if (hierWindow != null)
                    {
                        int found;
                        uint id;
                        VSDOCUMENTPRIORITY[] prio = new VSDOCUMENTPRIORITY[1];
                        if (project != null && ErrorHandler.Succeeded(project.IsDocumentInProject(node.FullPath, out found, prio, out id)) && found != 0)
                        {
                            hierWindow.ExpandItem(project as IVsUIHierarchy, id, EXPANDFLAGS.EXPF_SelectItem);
                        }
                        else if (string.Equals(node.FullPath, e.Selection.SolutionFilename, StringComparison.OrdinalIgnoreCase))
                            hierWindow.ExpandItem(e.GetService<IVsUIHierarchy>(typeof(SVsSolution)), VSConstants.VSITEMID_ROOT, EXPANDFLAGS.EXPF_SelectItem);

                        // Now try to activate the solution explorer
                        IVsWindowFrame solutionExplorer;
                        Guid solutionExplorerGuid = new Guid(ToolWindowGuids80.SolutionExplorer);
                        IVsUIShell shell = e.GetService<IVsUIShell>(typeof(SVsUIShell));

                        if (shell != null)
                        {
                            shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref solutionExplorerGuid, out solutionExplorer);

                            if (solutionExplorer != null)
                                solutionExplorer.Show();
                        }

                    }
                    break;
            }
        }
    }
}
