// VisualGit\Commands\SolutionPushCommand.cs
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
using VisualGit.Scc;
using System.Windows.Forms;
using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.PendingChangesPush)]
    [Command(VisualGitCommand.PendingChangesPushSpecificBranch)]
    [Command(VisualGitCommand.PendingChangesPushSpecificTag)]
    class SolutionPushCommand : CommandBase
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
            GitPushArgs args = new GitPushArgs();

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
                throw new InvalidOperationException("Pushing of multiple repository roots is not supported");
            }

            repositoryRoot = repositoryRoots.Single();

            switch (e.Command)
            {
                case VisualGitCommand.PendingChangesPushSpecificBranch:
                case VisualGitCommand.PendingChangesPushSpecificTag:
                    if (!QueryParameters(e, repositoryRoot, args))
                        return;
                    break;
            }

            ProgressRunnerArgs pa = new ProgressRunnerArgs();
            pa.CreateLog = true;
            pa.TransportClientArgs = args;

            GitException exception = null;

            e.GetService<IProgressRunner>().RunModal(CommandStrings.PushingSolution, pa,
                delegate(object sender, ProgressWorkerArgs a)
                {
                    using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                    {
                        try
                        {
                            client.Push(repositoryRoot, args);
                        }
                        catch (GitException ex)
                        {
                            exception = ex;
                        }
                    }
                });
            
            if (exception != null)
            {
                e.GetService<IVisualGitErrorHandler>().OnWarning(exception);
            }
        }

        private bool QueryParameters(CommandEventArgs e, string repositoryRoot, GitPushArgs args)
        {
            using (var dialog = new PushDialog())
            {
                dialog.Args = args;
                dialog.RepositoryPath = repositoryRoot;

                switch (e.Command)
                {
                    case VisualGitCommand.PendingChangesPushSpecificBranch:
                        dialog.Type = PushDialog.PushType.Branch;
                        break;

                    case VisualGitCommand.PendingChangesPushSpecificTag:
                        dialog.Type = PushDialog.PushType.Tag;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                return dialog.ShowDialog(e.Context) == DialogResult.OK;
            }
        }
    }
}
