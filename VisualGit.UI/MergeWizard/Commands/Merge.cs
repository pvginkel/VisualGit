// VisualGit.UI\MergeWizard\Commands\Merge.cs
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

using VisualGit.Commands;

using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.Selection;
using VisualGit.UI.Commands;
using SharpGit;
using VisualGit.Scc.UI;
using System.Diagnostics;

namespace VisualGit.UI.MergeWizard.Commands
{
    [Command(VisualGitCommand.ItemMerge)]
    [Command(VisualGitCommand.ProjectMerge)]
    [Command(VisualGitCommand.SolutionMerge)]
    [Command(VisualGitCommand.LogMergeThisRevision)]
    class Merge : ICommandHandler
    {
        /// <see cref="VisualGit.Commands.ICommandHandler.OnUpdate" />
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            IFileStatusCache statusCache;
            int n = 0;
            switch (e.Command)
            {
                case VisualGitCommand.LogMergeThisRevision:
                    ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                    if (logWindow == null)
                    {
                        e.Enabled = false;
                        return;
                    }

                    GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);

                    if (origin == null)
                    {
                        e.Enabled = false;
                        return;
                    }

                    IGitLogItem logItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

                    e.Enabled = logItem != null;
                    return;

                case VisualGitCommand.ItemMerge:
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                    {
                        if (!item.IsVersioned)
                        {
                            e.Enabled = false;
                            return;
                        }

                        n++;

                        if (n > 1)
                            break;
                    }
                    break;
                case VisualGitCommand.ProjectMerge:
                    statusCache = e.GetService<IFileStatusCache>();
                    IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();
                    foreach (GitProject project in e.Selection.GetSelectedProjects(false))
                    {
                        IGitProjectInfo projInfo = pfm.GetProjectInfo(project);
                        if (projInfo == null || string.IsNullOrEmpty(projInfo.ProjectDirectory))
                        {
                            e.Enabled = false;
                            return;
                        }
                        GitItem projectDir = statusCache[projInfo.ProjectDirectory];
                        if (!projectDir.IsVersioned)
                        {
                            e.Enabled = false;
                            return;
                        }

                        n++;

                        if (n > 1)
                            break;
                    }
                    break;
                case VisualGitCommand.SolutionMerge:
                    statusCache = e.GetService<IFileStatusCache>();
                    IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();
                    if (solutionSettings == null || string.IsNullOrEmpty(solutionSettings.ProjectRoot))
                    {
                        e.Enabled = false;
                        return;
                    }
                    GitItem solutionItem = statusCache[solutionSettings.ProjectRoot];
                    if (solutionItem.IsVersioned)
                        n = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (n != 1)
                e.Enabled = false;
        }

        /// <see cref="VisualGit.Commands.ICommandHandler.OnExecute" />
        public void OnExecute(CommandEventArgs e)
        {
            List<GitItem> gitItems = new List<GitItem>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            GitRevision revision = null;
            string repositoryPath = null;

            switch (e.Command)
            {
                case VisualGitCommand.LogMergeThisRevision:
                    ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();
                    IProgressRunner progressRunner = e.GetService<IProgressRunner>();

                    if (logWindow == null)
                        return;

                    IGitLogItem logItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

                    if (logItem == null)
                        return;

                    revision = logItem.Revision;
                    repositoryPath = logItem.RepositoryRoot;
                    break;
                case VisualGitCommand.ItemMerge:
                    // TODO: Check for solution and/or project selection to use the folder instead of the file
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                    {
                        gitItems.Add(item);
                    }
                    break;
                case VisualGitCommand.ProjectMerge:
                    foreach (GitProject p in e.Selection.GetSelectedProjects(false))
                    {
                        IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();

                        IGitProjectInfo info = pfm.GetProjectInfo(p);
                        if (info != null && info.ProjectDirectory != null)
                        {
                            gitItems.Add(cache[info.ProjectDirectory]);
                        }
                    }
                    break;
                case VisualGitCommand.SolutionMerge:
                    gitItems.Add(cache[e.GetService<IVisualGitSolutionSettings>().ProjectRoot]);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (repositoryPath == null)
            {
                Debug.Assert(gitItems.Count > 0);
                repositoryPath = GitTools.GetRepositoryRoot(gitItems[0].FullPath);
            }

            GitRef mergeBranch;
            var args = new GitMergeArgs();

            using (var dialog = new MergeDialog())
            {
                dialog.Context = e.Context;
                dialog.Revision = revision;
                dialog.RepositoryPath = repositoryPath;
                if (gitItems.Count > 0)
                    dialog.GitItem = gitItems[0];
                dialog.Args = args;

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                mergeBranch = dialog.MergeBranch;
            }
            
            // Get a list of all documents below the specified paths that are open in editors inside VS
            HybridCollection<string> lockPaths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();

            foreach (string file in documentTracker.GetDocumentsBelow(repositoryPath))
            {
                if (!lockPaths.Contains(file))
                    lockPaths.Add(file);
            }

            documentTracker.SaveDocuments(lockPaths); // Make sure all files are saved before merging!

            using (DocumentLock lck = documentTracker.LockDocuments(lockPaths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            {
                GitException exception = null;

                e.GetService<IProgressRunner>().RunModal(
                    "Changing Current Branch",
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);

                        try
                        {
                            a.Client.Merge(repositoryPath, mergeBranch, args);
                        }
                        catch (GitException ex)
                        {
                            exception = ex;
                        }
                    });

                if (exception != null)
                {
                    e.GetService<IVisualGitErrorHandler>().OnWarning(exception);
                }
            }
        }
    }
}
