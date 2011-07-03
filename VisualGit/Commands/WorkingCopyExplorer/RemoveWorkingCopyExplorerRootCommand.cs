// VisualGit\Commands\WorkingCopyExplorer\RemoveWorkingCopyExplorerRootCommand.cs
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
using System.Text;
using System.Collections;
using VisualGit.UI.WorkingCopyExplorer;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to remove current root from the Working Copy Explorer.
    /// </summary>
    [Command(VisualGitCommand.RemoveWorkingCopyExplorerRoot)]
    class RemoveWorkingCopyExplorerRootCommand : CommandBase
    {
       public override void OnUpdate(CommandUpdateEventArgs e)
        {
            WorkingCopyExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as WorkingCopyExplorerControl;

            if (ctrl == null)
                e.Enabled = false;
            else
                e.Enabled = ctrl.IsWcRootSelected();
        }

        public override void OnExecute(CommandEventArgs e)
        {
            WorkingCopyExplorerControl ctrl = e.Selection.ActiveDialogOrFrameControl as WorkingCopyExplorerControl;

            if (ctrl != null)
                ctrl.RemoveRoot();            
        }
    }
}
