// VisualGit\Commands\SolutionPullCommand.cs
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
using System.Linq;
using System.Text;
using VisualGit.VS;
using SharpGit;
using System.Windows.Forms;
using VisualGit.UI.Commands;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.PendingChangesPull)]
    [Command(VisualGitCommand.PendingChangesPullEx)]
    class SolutionPullCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();
            if (settings == null || string.IsNullOrEmpty(settings.ProjectRoot))
            {
                e.Enabled = false;
                return;
            }

            if (!settings.ProjectRootGitItem.IsVersioned)
                e.Enabled = false;
        }

        private IEnumerable<GitItem> GetAllRoots(BaseCommandEventArgs e)
        {
            IVisualGitProjectLayoutService pls = e.GetService<IVisualGitProjectLayoutService>();

            var result = new Dictionary<string, GitItem>(FileSystemUtil.StringComparer);

            foreach (var gitItem in pls.GetUpdateRoots(null))
            {
                if (gitItem.IsVersioned)
                    result[gitItem.FullPath] = gitItem;
            }

            return result.Values;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            GitPullArgs args = new GitPullArgs();

            string repositoryRoot;
            var repositoryRoots = new HashSet<string>(FileSystemUtil.StringComparer);

            foreach (var projectRoot in GetAllRoots(e))
            {
                if (
                    GitTools.TryGetRepositoryRoot(projectRoot.FullPath, out repositoryRoot) &&
                    !repositoryRoots.Contains(repositoryRoot)
                )
                    repositoryRoots.Add(repositoryRoot);
            }

            if (repositoryRoots.Count > 1)
            {
                throw new InvalidOperationException("Pulling of multiple repository roots is not supported");
            }

            repositoryRoot = repositoryRoots.Single();

            if (e.Command == VisualGitCommand.PendingChangesPullEx)
            {
                if (!QueryParameters(e, repositoryRoot, args))
                    return;
            }
            else
            {
                args.MergeStrategy = GitMergeStrategy.DefaultForBranch;
            }

            GitPullResult result = null;

            ProgressRunnerArgs pa = new ProgressRunnerArgs();
            pa.CreateLog = true;
            pa.TransportClientArgs = args;

            // Get a list of all documents below the specified paths that are open in editors inside VS
            HybridCollection<string> lockPaths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();

            foreach (string file in documentTracker.GetDocumentsBelow(repositoryRoot))
            {
                if (!lockPaths.Contains(file))
                    lockPaths.Add(file);
            }

            documentTracker.SaveDocuments(lockPaths); // Make sure all files are saved before merging!

            using (DocumentLock lck = documentTracker.LockDocuments(lockPaths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            {
                GitException exception = null;

                e.GetService<IProgressRunner>().RunModal(CommandStrings.PullingSolution, pa,
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);

                        try
                        {
                            a.Client.Pull(repositoryRoot, args, out result);
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

        private bool QueryParameters(CommandEventArgs e, string repositoryRoot, GitPullArgs args)
        {
            using (var dialog = new PullDialog())
            {
                dialog.Args = args;
                dialog.RepositoryPath = repositoryRoot;

                return dialog.ShowDialog(e.Context) == DialogResult.OK;
            }
        }
    }
}
