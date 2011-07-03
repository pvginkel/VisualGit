// VisualGit\Commands\AddItemCommand.cs
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
using VisualGit.UI.PathSelector;
using System.Windows.Forms;
using System.Collections.Generic;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to add selected items to the working copy.
    /// </summary>
    [Command(VisualGitCommand.AddItem, ArgumentDefinition = "d")]
    class AddItemCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (item.IsVersioned)
                    continue;
                if (item.IsVersionable)
                    return; // We found an add item
            }

            e.Visible = e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            string argumentFile = e.Argument as string;
            List<GitItem> selection = new List<GitItem>();

            if (string.IsNullOrEmpty(argumentFile))
            {
                if (e.PromptUser || (!e.DontPrompt && !Shift))
                {
                    selection.AddRange(e.Selection.GetSelectedGitItems(true));

                    using (PendingChangeSelector pcs = new PendingChangeSelector())
                    {
                        pcs.Text = CommandStrings.AddDialogTitle;

                        pcs.PreserveWindowPlacement = true;

                        pcs.LoadItems(selection,
                                      delegate(GitItem item) { return !item.IsVersioned && item.IsVersionable; },
                                      delegate(GitItem item) { return !item.IsIgnored || !item.InSolution; });

                        if (pcs.ShowDialog(e.Context) != DialogResult.OK)
                            return;

                        selection.Clear();
                        selection.AddRange(pcs.GetSelectedItems());
                    }
                }
                else
                {
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
                    {
                        if (!item.IsVersioned && item.IsVersionable && !item.IsIgnored && item.InSolution)
                            selection.Add(item);
                    }
                }
            }
            else
            {
                selection.Add(e.GetService<IFileStatusCache>()[argumentFile]);
            }

            ICollection<string> paths = GitItem.GetPaths(selection);
            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();
            documentTracker.SaveDocuments(paths); // Make sure all files are saved before updating/merging!

            using (DocumentLock lck = documentTracker.LockDocuments(paths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
                e.GetService<IProgressRunner>().RunModal(CommandStrings.AddTaskDialogTitle,
                    delegate(object sender, ProgressWorkerArgs ee)
                    {
                        GitAddArgs args = new GitAddArgs();
                        args.Depth = GitDepth.Empty;
                        args.AddParents = true;

                        foreach (GitItem item in selection)
                        {
                            ee.Client.Add(item.FullPath, args);
                        }
                    });
        }
    }
}
