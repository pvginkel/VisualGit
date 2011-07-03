// VisualGit\Commands\ShowToolWindows.cs
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
