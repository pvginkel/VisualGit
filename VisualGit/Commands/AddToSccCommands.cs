// VisualGit\Commands\AddToSccCommands.cs
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
using VisualGit.Scc;
using VisualGit.Selection;
using VisualGit.UI;
using System.Windows.Forms;
using System.IO;
using VisualGit.VS;
using VisualGit.UI.SccManagement;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SharpGit;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.FileSccAddSolutionToGit, AlwaysAvailable = true, HideWhenDisabled = false)]
    sealed class AddToSccCommands : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (!e.State.SolutionExists)
            {
                e.Visible = e.Enabled = false;
                return;
            }

            if (e.State.OtherSccProviderActive)
            {
                e.Visible = e.Enabled = false;
                return; // Only one scc provider can be active at a time
            }

            IVisualGitSccService scc = e.GetService<IVisualGitSccService>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            if (scc == null || cache == null)
            {
                e.Visible = e.Enabled = false;
                return;
            }

            string solutionFilename = e.Selection.SolutionFilename;

            if (string.IsNullOrEmpty(solutionFilename) || !GitItem.IsValidPath(solutionFilename))
                solutionFilename = null;

            if (solutionFilename == null || scc.IsSolutionManaged)
            {
                e.Visible = e.Enabled = false; // Already handled
                return;
            }
            GitItem item = cache[solutionFilename];

            if (!item.Exists || !item.IsFile)
            {
                // Decide where you store the .sln first
                e.Visible = e.Enabled = false;
                return;
            }

            if (!item.IsVersioned)
            {
                // If the .sln is ignored hide it in the context menus
                // but don't hide it on 
                e.HideOnContextMenu = item.IsIgnored && !e.Selection.IsSolutionSelected;
            }
            else
            {
                e.Visible = e.Enabled = false;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IFileStatusCache cache = e.GetService<IFileStatusCache>();

            if (cache == null || e.Selection.SolutionFilename == null)
                return;

            GitItem item = cache[e.Selection.SolutionFilename];

            HandleUnmanagedOrUnversionedSolution(e, item);
        }

        static bool HandleUnmanagedOrUnversionedSolution(CommandEventArgs e, GitItem solutionItem)
        {
            IVisualGitSccService scc = e.GetService<IVisualGitSccService>();
            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

            bool shouldActivate = false;
            if (!scc.IsActive)
            {
                if (e.State.OtherSccProviderActive)
                    return false; // Can't switch in this case.. Nothing to do

                // VisualGit is not the active provider, we should register as active
                shouldActivate = true;
            }

            if (scc.IsSolutionManaged && solutionItem.IsVersioned)
                return true; // Projects should still be checked

            bool confirmed = false;


            if (solutionItem.IsVersioned)
            { /* File is in Git; just enable */ }
            else
            {
                if (!CheckoutWorkingCopyForSolution(e, ref confirmed))
                    return false;
            }

            if (!confirmed && !e.DontPrompt && !e.IsInAutomation &&
                DialogResult.Yes != mb.Show(string.Format(CommandResources.MarkXAsManaged,
                Path.GetFileName(e.Selection.SolutionFilename)), "", MessageBoxButtons.YesNo))
            {
                return false;
            }

            SetSolutionManaged(shouldActivate, solutionItem, scc);

            return true;
        }

        static GitItem GetVersionedParent(GitItem child)
        {
            if (!child.IsVersionable)
                return null;

            if (!child.IsVersioned)
                return GetVersionedParent(child.Parent);
            return child;
        }

        static void SetSolutionManaged(bool shouldActivate, GitItem item, IVisualGitSccService scc)
        {
            if (shouldActivate)
                scc.RegisterAsPrimarySccProvider();

            scc.SetProjectManaged(null, true);
            item.MarkDirty(); // This clears the solution settings cache to retrieve its properties
        }

        static bool CheckoutWorkingCopyForSolution(CommandEventArgs e, ref bool confirmed)
        {
            using (AddToGit dialog = new AddToGit())
            {
                dialog.PathToAdd = e.Selection.SolutionFilename;
                if (dialog.ShowDialog(e.Context) == DialogResult.OK)
                {
                    confirmed = true;

                    GitCreateRepositoryArgs args = new GitCreateRepositoryArgs();

                    using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                    {
                        client.CreateRepository(dialog.WorkingCopyDir, args);
                    }

                    IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();
                    IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
                    IFileStatusMonitor monitor = e.GetService<IFileStatusMonitor>();

                    settings.ProjectRoot = GitTools.GetNormalizedFullPath(dialog.WorkingCopyDir);

                    if (monitor != null && mapper != null)
                    {
                        // Make sure all visible glyphs are updated to reflect a new working copy
                        monitor.ScheduleGitStatus(mapper.GetAllFilesOfAllProjects());
                    }

                    return true;
                }

                return false; // User cancelled the "Add to Git" dialog, don't set as managed by VisualGit
            }
        }
    }
}
