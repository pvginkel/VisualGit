// VisualGit\Commands\ItemRevertCommand.cs
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
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.UI;
using System.Collections.Generic;
using VisualGit.UI.PathSelector;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to revert current item to last updated revision.
    /// </summary>
    [Command(VisualGitCommand.RevertItem)]
    [Command(VisualGitCommand.ItemRevertBase, HideWhenDisabled = false)]
    class RevertItemCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (item.IsModified || (item.IsVersioned && item.IsDocumentDirty) || item.IsConflicted)
                    return;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitItem> toRevert = new List<GitItem>();
            HybridCollection<string> contained = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            HybridCollection<string> checkedItems = null;

            foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
            {
                if (contained.Contains(i.FullPath))
                    continue;

                contained.Add(i.FullPath);

                if (i.IsModified || (i.IsVersioned && i.IsDocumentDirty) || i.IsConflicted)
                    toRevert.Add(i);
            }

            Predicate<GitItem> initialCheckedFilter = null;
            if (toRevert.Count > 0)
            {
                checkedItems = new HybridCollection<string>(contained, StringComparer.OrdinalIgnoreCase);

                initialCheckedFilter = delegate(GitItem item)
                    {
                        return checkedItems.Contains(item.FullPath);
                    };
            }

            foreach (GitItem i in e.Selection.GetSelectedGitItems(true))
            {
                if (contained.Contains(i.FullPath))
                    continue;

                contained.Add(i.FullPath);

                if (i.IsModified || (i.IsVersioned && i.IsDocumentDirty))
                    toRevert.Add(i);
            }

            if (e.PromptUser || (!e.DontPrompt && !Shift))
            {
                using (PendingChangeSelector pcs = new PendingChangeSelector())
                {
                    pcs.Text = CommandStrings.RevertDialogTitle;

                    pcs.PreserveWindowPlacement = true;

                    pcs.LoadItems(toRevert, null, initialCheckedFilter);

                    if (pcs.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    toRevert.Clear();
                    toRevert.AddRange(pcs.GetSelectedItems());
                }
            }


            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();

            ICollection<string> revertPaths = GitItem.GetPaths(toRevert);
            documentTracker.SaveDocuments(revertPaths);

            // perform the actual revert 
            using (DocumentLock dl = documentTracker.LockDocuments(revertPaths, DocumentLockType.NoReload))
            using (dl.MonitorChangesForReload())
            {
                e.GetService<IProgressRunner>().RunModal(CommandStrings.Reverting,
                delegate(object sender, ProgressWorkerArgs a)
                {
                    GitRevertItemArgs ra = new GitRevertItemArgs();
                    ra.Depth = GitDepth.Empty;

                    List<string> toRevertPaths = new List<string>();

                    foreach (GitItem item in toRevert)
                    {
                        toRevertPaths.Add(item.FullPath);
                    }

                    foreach (GitItem item in toRevert)
                    {
                        try
                        {
                            a.Client.RevertItem(toRevertPaths, ra);
                        }
                        catch (GitNoRepositoryException)
                        {
                            // Ignore path no repository exceptions.
                        }
                    }
                });
            }
        }
    }
}
