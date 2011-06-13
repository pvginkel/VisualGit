using System;
using System.Collections.Generic;

using VisualGit.Commands;

using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.Selection;
using VisualGit.UI.Commands;
using SharpGit;

namespace VisualGit.UI.MergeWizard.Commands
{
    [Command(VisualGitCommand.ItemMerge)]
    [Command(VisualGitCommand.ProjectMerge)]
    [Command(VisualGitCommand.SolutionMerge)]
    class Merge : ICommandHandler
    {
        /// <see cref="VisualGit.Commands.ICommandHandler.OnUpdate" />
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            IFileStatusCache statusCache;
            int n = 0;
            switch (e.Command)
            {
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

            switch (e.Command)
            {
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

            string repositoryPath = RepositoryUtil.GetRepositoryRoot(gitItems[0].FullPath);
            GitRef mergeBranch;
            var args = new GitMergeArgs();

            using (var dialog = new MergeDialog())
            {
                dialog.Context = e.Context;
                dialog.RepositoryPath = repositoryPath;
                dialog.GitItem = gitItems[0];
                dialog.Args = args;

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                mergeBranch = dialog.MergeBranch;
            }

            IEnumerable<string> selectedFiles = e.Selection.GetSelectedFiles(true);
            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();

            using (DocumentLock lck = tracker.LockDocuments(selectedFiles, DocumentLockType.ReadOnly))
            using (lck.MonitorChangesForReload())
            {
                e.GetService<IProgressRunner>().RunModal(
                    "Changing Current Branch",
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);

                        a.Client.Merge(repositoryPath, mergeBranch, args);
                    });

                if (args.LastException != null)
                {
                    e.Context.GetService<IVisualGitDialogOwner>()
                        .MessageBox.Show(args.LastException.Message,
                        args.LastException.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
