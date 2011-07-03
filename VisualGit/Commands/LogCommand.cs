// VisualGit\Commands\LogCommand.cs
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

using System.Diagnostics;
using System.Collections.Generic;
using VisualGit.UI;
using VisualGit.UI.GitLog;
using VisualGit.Selection;
using VisualGit.VS;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using SharpGit;
using System;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to show the change log for the selected item.
    /// </summary>
    [Command(VisualGitCommand.Log)]
    [Command(VisualGitCommand.DocumentHistory)]
    [Command(VisualGitCommand.ProjectHistory)]
    [Command(VisualGitCommand.SolutionHistory)]
    [Command(VisualGitCommand.AnnotateShowLog, AlwaysAvailable = true)]
    sealed class LogCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            switch (e.Command)
            {
                case VisualGitCommand.ProjectHistory:
                    GitProject p = EnumTools.GetFirst(e.Selection.GetSelectedProjects(false));
                    if (p == null)
                        break;

                    IGitProjectInfo pi = e.GetService<IProjectFileMapper>().GetProjectInfo(p);

                    if (pi == null || string.IsNullOrEmpty(pi.ProjectDirectory))
                        break; // No project location

                    if (e.GetService<IFileStatusCache>()[pi.ProjectDirectory].HasCopyableHistory)
                        return; // Ok, we have history!                                           

                    break; // No history

                case VisualGitCommand.SolutionHistory:
                    IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();

                    if (ss == null || string.IsNullOrEmpty(ss.ProjectRoot))
                        break;

                    if (e.GetService<IFileStatusCache>()[ss.ProjectRoot].HasCopyableHistory)
                        return; // Ok, we have history!

                    break; // No history
                case VisualGitCommand.DocumentHistory:
                    GitItem docitem = e.Selection.ActiveDocumentItem;
                    if (docitem != null && docitem.HasCopyableHistory)
                        return;
                    break; // No history
                case VisualGitCommand.Log:
                    int itemCount = 0;
                    int needsRemoteCount = 0;
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                    {
                        if (!item.IsVersioned)
                        {
                            e.Enabled = false;
                            return;
                        }

                        if (item.IsAdded)
                        {
                            if (item.HasCopyableHistory)
                                needsRemoteCount++;
                            else
                            {
                                e.Enabled = false;
                                return;
                            }
                        }
                        itemCount++;
                    }
                    if (itemCount == 0 || (needsRemoteCount != 0 && itemCount > 1))
                    {
                        e.Enabled = false;
                        return;
                    }
                    if (needsRemoteCount >= 1)
                    {
                        // One remote log
                        Debug.Assert(needsRemoteCount == 1);
                        return;
                    }
                    
                    // Local log only
                    return;
                case VisualGitCommand.AnnotateShowLog:
                    if (EnumTools.GetSingle(e.Selection.GetSelection<IAnnotateSection>()) != null)
                        return;
                    break;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitOrigin> selected = new List<GitOrigin>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();

            switch (e.Command)
            {
                case VisualGitCommand.Log:
                    IVisualGitDiffHandler diffHandler = e.GetService<IVisualGitDiffHandler>();
                    List<GitOrigin> items = new List<GitOrigin>();
                    foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
                    {
                        Debug.Assert(i.IsVersioned);

                        if (i.IsAdded)
                        {
                            if (!i.HasCopyableHistory)
                                continue;

                            items.Add(new GitOrigin(diffHandler.GetCopyOrigin(i), i.WorkingCopy.RepositoryRoot));
                            continue;
                        }

                        items.Add(new GitOrigin(i));
                    }
                    PerformLog(e.Context, items, null, null);
                    break;
                case VisualGitCommand.SolutionHistory:
                    IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();

                    PerformLog(e.Context, new GitOrigin[] { new GitOrigin(cache[settings.ProjectRoot]) }, null, null);
                    break;
                case VisualGitCommand.ProjectHistory:
                    IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
                    foreach (GitProject p in e.Selection.GetSelectedProjects(false))
                    {
                        IGitProjectInfo info = mapper.GetProjectInfo(p);

                        if (info != null)
                            selected.Add(new GitOrigin(cache[info.ProjectDirectory]));
                    }

                    PerformLog(e.Context, selected, null, null);
                    break;
                case VisualGitCommand.DocumentHistory:
                    GitItem docItem = e.Selection.ActiveDocumentItem;
                    Debug.Assert(docItem != null);

                    PerformLog(e.Context, new GitOrigin[] { new GitOrigin(docItem) }, null, null);
                    break;
                case VisualGitCommand.AnnotateShowLog:
                    IAnnotateSection section = EnumTools.GetSingle(e.Selection.GetSelection<IAnnotateSection>());

                    if (section == null)
                        return;

                    PerformLog(e.Context, new GitOrigin[] { section.Origin }, section.Revision, null);
                    break;
            }
        }

        static void PerformLog(IVisualGitServiceProvider context, ICollection<GitOrigin> targets, GitRevision start, GitRevision end)
        {
            IVisualGitPackage package = context.GetService<IVisualGitPackage>();

            package.ShowToolWindow(VisualGitToolWindow.Log);

            LogToolWindowControl logToolControl = context.GetService<ISelectionContext>().ActiveFrameControl as LogToolWindowControl;
            if (logToolControl != null)
                logToolControl.StartLog(targets, start, end);
        }
    }
}
