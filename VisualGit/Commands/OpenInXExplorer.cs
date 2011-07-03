// VisualGit\Commands\OpenInXExplorer.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemSelectInWorkingCopyExplorer)]
    [Command(VisualGitCommand.ItemSelectInSolutionExplorer)]
    class OpenInXExplorer : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            GitItem node = EnumTools.GetSingle(e.Selection.GetSelectedGitItems(false));

            if (node == null && e.Selection.IsSingleNodeSelection)
                node = EnumTools.GetFirst(e.Selection.GetSelectedGitItems(false));

            bool enable = true;
            if (node == null)
                enable = false;
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
            GitItem node = EnumTools.GetFirst(e.Selection.GetSelectedGitItems(false));

            IVisualGitCommandService cmd = e.GetService<IVisualGitCommandService>();
            switch (e.Command)
            {
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
