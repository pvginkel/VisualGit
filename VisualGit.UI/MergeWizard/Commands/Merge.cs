using System;
using System.Collections.Generic;

using VisualGit.Commands;

using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.Selection;

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
                    foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
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
                    foreach (SvnProject project in e.Selection.GetSelectedProjects(false))
                    {
                        ISvnProjectInfo projInfo = pfm.GetProjectInfo(project);
                        if (projInfo == null || string.IsNullOrEmpty(projInfo.ProjectDirectory))
                        {
                            e.Enabled = false;
                            return;
                        }
                        SvnItem projectDir = statusCache[projInfo.ProjectDirectory];
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
                    SvnItem solutionItem = statusCache[solutionSettings.ProjectRoot];
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
            List<SvnItem> svnItems = new List<SvnItem>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();

            switch (e.Command)
            {
                case VisualGitCommand.ItemMerge:
                    // TODO: Check for solution and/or project selection to use the folder instead of the file
                    foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
                    {
                        svnItems.Add(item);
                    }
                    break;
                case VisualGitCommand.ProjectMerge:
                    foreach (SvnProject p in e.Selection.GetSelectedProjects(false))
                    {
                        IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();

                        ISvnProjectInfo info = pfm.GetProjectInfo(p);
                        if (info != null && info.ProjectDirectory != null)
                        {
                            svnItems.Add(cache[info.ProjectDirectory]);
                        }
                    }
                    break;
                case VisualGitCommand.SolutionMerge:
                    svnItems.Add(cache[e.GetService<IVisualGitSolutionSettings>().ProjectRoot]);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            IEnumerable<string> selectedFiles = e.Selection.GetSelectedFiles(true);
            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();

            using (DocumentLock lck = tracker.LockDocuments(selectedFiles, DocumentLockType.ReadOnly))
            using (lck.MonitorChangesForReload())
            using (MergeWizard dialog = new MergeWizard(e.Context, svnItems[0]))
            {
                DialogResult result = dialog.ShowDialog(e.Context);
                //result = uiService.ShowDialog(dialog);

                if (result == DialogResult.OK)
                {
                    using (MergeResultsDialog mrd = new MergeResultsDialog())
                    {
                        mrd.MergeActions = dialog.MergeActions;
                        mrd.ResolvedMergeConflicts = dialog.ResolvedMergeConflicts;

                        mrd.ShowDialog(e.Context);
                    }
                }

            }
        }
    }
}
