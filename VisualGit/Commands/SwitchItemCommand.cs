using System;
using System.Windows.Forms;

using SharpSvn;

using VisualGit.Scc;
using VisualGit.Selection;
using VisualGit.UI.Commands;
using VisualGit.VS;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to switch current item to a different URL.
    /// </summary>
    [Command(VisualGitCommand.SwitchItem)]
    [Command(VisualGitCommand.SolutionSwitchDialog)]
    [Command(VisualGitCommand.SwitchProject)]
    class SwitchItemCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {

            IFileStatusCache statusCache;
            switch (e.Command)
            {
                case VisualGitCommand.SolutionSwitchDialog:
                    IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();
                    if (solutionSettings == null || string.IsNullOrEmpty(solutionSettings.ProjectRoot))
                    {
                        e.Enabled = false;
                        return;
                    }
                    statusCache = e.GetService<IFileStatusCache>();
                    GitItem solutionItem = statusCache[solutionSettings.ProjectRoot];
                    if (!solutionItem.IsVersioned)
                    {
                        e.Enabled = false;
                        return;
                    }
                    break;

                case VisualGitCommand.SwitchProject:
                    statusCache = e.GetService<IFileStatusCache>();
                    IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();
                    foreach (GitProject item in e.Selection.GetSelectedProjects(true))
                    {
                        IGitProjectInfo pi = pfm.GetProjectInfo(item);

                        if (pi == null || pi.ProjectDirectory == null)
                        {
                            e.Enabled = false;
                            return;
                        }

                        GitItem projectItem = statusCache[pi.ProjectDirectory];
                        if (!projectItem.IsVersioned)
                        {
                            e.Enabled = false;
                            return;
                        }
                    }
                    break;

                case VisualGitCommand.SwitchItem:
                    bool foundOne = false, error = false;
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                    {
                        if (item.IsVersioned && !foundOne)
                            foundOne = true;
                        else
                        {
                            error = true;
                            break;
                        }
                    }

                    e.Enabled = foundOne && !error;
                    break;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            GitItem theItem = null;
            string path;
            bool allowObstructions = false;

            string projectRoot = e.GetService<IVisualGitSolutionSettings>().ProjectRoot;

            if (e.Command == VisualGitCommand.SolutionSwitchDialog)
                path = projectRoot;
            else if (e.Command == VisualGitCommand.SwitchProject)
            {
                IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
                path = null;

                foreach (GitProject item in e.Selection.GetSelectedProjects(true))
                {
                    IGitProjectInfo pi = mapper.GetProjectInfo(item);

                    if (pi == null)
                        continue;

                    path = pi.ProjectDirectory;
                    break;
                }

                if (string.IsNullOrEmpty(path))
                    return;
            }
            else
            {
                foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                {
                    if (item.IsVersioned)
                    {
                        theItem = item;
                        break;
                    }
                    return;
                }
                path = theItem.FullPath;
            }

            IFileStatusCache statusCache = e.GetService<IFileStatusCache>();

            GitItem pathItem = statusCache[path];
            Uri uri = pathItem.Uri;

            if (uri == null)
                return; // Should never happen on a real workingcopy

            GitUriTarget target;
            GitRevision revision = GitRevision.None;

            if (e.Argument is string)
            {
                target = GitUriTarget.FromString((string)e.Argument);
                revision = (target.Revision != GitRevision.None) ? target.Revision : GitRevision.Head;
            }
            else if (e.Argument is Uri)
                target = (Uri)e.Argument;
            else
                using (SwitchDialog dlg = new SwitchDialog())
                {
                    dlg.Context = e.Context;

                    dlg.LocalPath = path;
                    dlg.RepositoryRoot = e.GetService<IFileStatusCache>()[path].WorkingCopy.RepositoryRoot;
                    dlg.SwitchToUri = uri;
                    dlg.Revision = GitRevision.Head;

                    if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    target = dlg.SwitchToUri;
                    revision = dlg.Revision;
                    allowObstructions = dlg.AllowUnversionedObstructions;
                }

            // Get a list of all documents below the specified paths that are open in editors inside VS
            HybridCollection<string> lockPaths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();

            foreach (string file in documentTracker.GetDocumentsBelow(path))
            {
                if (!lockPaths.Contains(file))
                    lockPaths.Add(file);
            }

            documentTracker.SaveDocuments(lockPaths); // Make sure all files are saved before merging!

            using (DocumentLock lck = documentTracker.LockDocuments(lockPaths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            {
                Uri newRepositoryRoot = null;
                e.GetService<IProgressRunner>().RunModal(
                    "Switching",
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        SvnSwitchArgs args = new SvnSwitchArgs();

                        throw new NotImplementedException();

#if false
                        args.AllowObstructions = allowObstructions;
                        args.AddExpectedError(SvnErrorCode.SVN_ERR_WC_INVALID_SWITCH);

                        if (revision != GitRevision.None)
                            args.Revision = revision;

                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);
                        if (!a.SvnClient.Switch(path, target, args))
                        {
                            if (args.LastException.SvnErrorCode != SvnErrorCode.SVN_ERR_WC_INVALID_SWITCH)
                                return;

                            // source/target repository is different, check if we can fix this by relocating
                            SvnInfoEventArgs iea;
                            if (a.SvnClient.GetInfo(target, out iea))
                            {
                                if (pathItem.WorkingCopy.RepositoryId != iea.RepositoryId)
                                {
                                    e.Context.GetService<IVisualGitDialogOwner>()
                                        .MessageBox.Show("Cannot switch to different repository because the repository UUIDs are different",
                                        "Cannot switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else if (pathItem.WorkingCopy.RepositoryRoot != iea.RepositoryRoot)
                                {
                                    newRepositoryRoot = iea.RepositoryRoot;
                                }
                                else if (pathItem.WorkingCopy.RepositoryId == Guid.Empty)
                                {
                                    // No UUIDs and RepositoryRoot equal. Throw/show error?

                                    throw args.LastException;
                                }
                            }
                        }
#endif
                    });

                if (newRepositoryRoot != null && DialogResult.Yes == e.Context.GetService<IVisualGitDialogOwner>()
                   .MessageBox.Show(string.Format("The repository root specified is different from the one in your " +
                   "working copy. Would you like to relocate from '{0}' to '{1}'?",
                   pathItem.WorkingCopy.RepositoryRoot, newRepositoryRoot),
                   "Relocate", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    // We can fix this by relocating
                    try
                    {
                        e.GetService<IProgressRunner>().RunModal(
                            "Relocating",
                            delegate(object sender, ProgressWorkerArgs a)
                            {
                                a.SvnClient.Relocate(path, pathItem.WorkingCopy.RepositoryRoot, newRepositoryRoot);
                            });
                    }
                    finally
                    {
                        statusCache.MarkDirtyRecursive(path);
                        e.GetService<IFileStatusMonitor>().ScheduleGlyphUpdate(GitItem.GetPaths(statusCache.GetCachedBelow(path)));
                    }


                    if (DialogResult.Yes == e.Context.GetService<IVisualGitDialogOwner>()
                        .MessageBox.Show(string.Format("Would you like to try to switch '{0}' to '{1}' again?",
                        path, target),
                        "Switch", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        // Try to switch again
                        e.GetService<IProgressRunner>().RunModal(
                        "Switching",
                        delegate(object sender, ProgressWorkerArgs a)
                        {
                            SvnSwitchArgs args = new SvnSwitchArgs();

                            throw new NotImplementedException();
#if false
                            if (revision != GitRevision.None)
                                args.Revision = revision;

                            args.AllowObstructions = allowObstructions;

                            e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);
                            a.SvnClient.Switch(path, target, args);
#endif
                        });
                    }
                }
            }
        }
    }
}
