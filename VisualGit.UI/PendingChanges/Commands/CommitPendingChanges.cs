// VisualGit.UI\PendingChanges\Commands\CommitPendingChanges.cs
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
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.CommitPendingChanges)]
    [Command(VisualGitCommand.PendingChangesApplyWorkingCopy)]
    class CommitPendingChanges : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            PendingCommitsPage commitPage = e.Context.GetService<PendingCommitsPage>();
            if (commitPage == null)
            {
                e.Enabled = false;
                return;
            }

            switch (e.Command)
            {
                case VisualGitCommand.CommitPendingChanges:
                    e.Enabled = true
                        // check if commit page or issues page is visible
                        && (false
                             || commitPage.Visible
                             )
                         // make sure commit page can commit
                         && commitPage.CanCommit()
                         ;
                    break;
                case VisualGitCommand.PendingChangesApplyWorkingCopy:
                    e.Enabled = commitPage.Visible && commitPage.CanApplyToWorkingCopy();
                    break;
            }
        }

        public void OnExecute(CommandEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if (page != null)
                switch (e.Command)
                {
                    case VisualGitCommand.CommitPendingChanges:
                        page.DoCommit();
                        break;
                    case VisualGitCommand.PendingChangesApplyWorkingCopy:
                        page.ApplyToWorkingCopy();
                        break;
                }
        }
    }
}
