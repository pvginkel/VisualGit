using System.Diagnostics;
using SharpSvn;
using System.Collections.Generic;
using VisualGit.UI;
using VisualGit.UI.SvnLog;
using VisualGit.Selection;
using VisualGit.VS;
using VisualGit.Scc;
using VisualGit.Scc.UI;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to show the change log for the selected item.
    /// </summary>
    [Command(VisualGitCommand.Log)]
    [Command(VisualGitCommand.DocumentHistory)]
    [Command(VisualGitCommand.ProjectHistory)]
    [Command(VisualGitCommand.SolutionHistory)]
    [Command(VisualGitCommand.ReposExplorerLog, AlwaysAvailable = true)]
    [Command(VisualGitCommand.AnnotateShowLog, AlwaysAvailable = true)]
    sealed class LogCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            int i;

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

                        if (item.IsReplaced || item.IsAdded)
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
                case VisualGitCommand.ReposExplorerLog:
                    i = 0;
                    foreach (IGitRepositoryItem item in e.Selection.GetSelection<IGitRepositoryItem>())
                    {
                        if (item == null || item.Origin == null)
                            continue;
                        i++;
                        if (i > 1)
                            break;
                    }
                    if (i == 1)
                        return;
                    break;
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

                        if (i.IsReplaced || i.IsAdded)
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
                case VisualGitCommand.ReposExplorerLog:
                    IGitRepositoryItem item = null;
                    foreach (IGitRepositoryItem i in e.Selection.GetSelection<IGitRepositoryItem>())
                    {
                        if (i != null && i.Uri != null)
                            item = i;
                        break;
                    }

                    if (item != null)
                        PerformLog(e.Context, new GitOrigin[] { item.Origin }, null, null);
                    break;
                case VisualGitCommand.AnnotateShowLog:
                    IAnnotateSection section = EnumTools.GetSingle(e.Selection.GetSelection<IAnnotateSection>());

                    if (section == null)
                        return;

                    PerformLog(e.Context, new GitOrigin[] { section.Origin }, section.Revision, null);

                    break;
            }
        }

        static void PerformLog(IVisualGitServiceProvider context, ICollection<GitOrigin> targets, SvnRevision start, SvnRevision end)
        {
            IVisualGitPackage package = context.GetService<IVisualGitPackage>();

            package.ShowToolWindow(VisualGitToolWindow.Log);

            LogToolWindowControl logToolControl = context.GetService<ISelectionContext>().ActiveFrameControl as LogToolWindowControl;
            if (logToolControl != null)
                logToolControl.StartLog(targets, start, end);
        }
    }
}
