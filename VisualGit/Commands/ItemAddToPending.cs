// VisualGit\Commands\ItemAddToPending.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemAddToPending)]
    [Command(VisualGitCommand.ItemRemoveFromPending)]
    [Command(VisualGitCommand.DocumentAddToPending)]
    [Command(VisualGitCommand.DocumentRemoveFromPending)]
    class ItemAddToPending : CommandBase
    {
        IEnumerable<GitItem> GetSelection(BaseCommandEventArgs e)
        {
            if (e.Command == VisualGitCommand.DocumentAddToPending || e.Command == VisualGitCommand.DocumentRemoveFromPending)
            {
                GitItem i = e.Selection.ActiveDocumentItem;
                if (i == null)
                    return new GitItem[0];
                else
                    return new GitItem[] { i };
            }
            else
                return e.Selection.GetSelectedGitItems(false);
        }

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool add;
            IPendingChangesManager pcm = null;

            add = (e.Command == VisualGitCommand.ItemAddToPending) || (e.Command == VisualGitCommand.DocumentAddToPending);

            foreach (GitItem i in GetSelection(e))
            {
                if (i.InSolution || !PendingChange.IsPending(i))
                    continue;

                if (pcm == null)
                {
                    pcm = e.GetService<IPendingChangesManager>();
                    if (pcm == null)
                        break;
                }

                if (pcm.Contains(i.FullPath) != add)
                    return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IFileStatusMonitor fsm = e.GetService<IFileStatusMonitor>();

            foreach (GitItem i in GetSelection(e))
            {
                if (i.InSolution)
                    continue;

                if (e.Command == VisualGitCommand.ItemAddToPending || e.Command == VisualGitCommand.DocumentAddToPending)
                    fsm.ScheduleMonitor(i.FullPath);
                else
                    fsm.StopMonitoring(i.FullPath);
            }
        }
    }
}
