using System;
using System.Collections.Generic;
using VisualGit.VS;
using VisualGit.Selection;
using SharpSvn;
using VisualGit.Scc;
using VisualGit.UI.Commands;
using System.Diagnostics;
using System.Windows.Forms;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.PendingChangesUpdateLatest, HideWhenDisabled = false)]
    [Command(VisualGitCommand.SolutionUpdateLatest)]
    [Command(VisualGitCommand.SolutionUpdateSpecific)]
    [Command(VisualGitCommand.ProjectUpdateLatest)]
    [Command(VisualGitCommand.ProjectUpdateSpecific)]
    [Command(VisualGitCommand.FolderUpdateSpecific)]
    [Command(VisualGitCommand.FolderUpdateLatest)]
    class SolutionUpdateCommand : CommandBase
    {
        static bool IsSolutionCommand(VisualGitCommand command)
        {
            switch (command)
            {
                case VisualGitCommand.SolutionUpdateLatest:
                case VisualGitCommand.SolutionUpdateSpecific:
                case VisualGitCommand.PendingChangesUpdateLatest:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsFolderCommand(VisualGitCommand command)
        {
            switch (command)
            {
                case VisualGitCommand.FolderUpdateLatest:
                case VisualGitCommand.FolderUpdateSpecific:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsHeadCommand(VisualGitCommand command)
        {
            switch (command)
            {
                case VisualGitCommand.SolutionUpdateLatest:
                case VisualGitCommand.ProjectUpdateLatest:
                case VisualGitCommand.PendingChangesUpdateLatest:
                case VisualGitCommand.FolderUpdateLatest:
                    return true;
                default:
                    return false;
            }
        }

        static IEnumerable<GitProject> GetSelectedProjects(BaseCommandEventArgs e)
        {
            foreach (GitProject p in e.Selection.GetSelectedProjects(false))
            {
                yield return p;
            }
        }

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (IsSolutionCommand(e.Command))
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
            else if (IsFolderCommand(e.Command))
            {
                bool forHead = IsHeadCommand(e.Command);
                bool foundOne = false;
                Uri root = null;
                foreach (GitItem dir in e.Selection.GetSelectedGitItems(false))
                {
                    if (!dir.IsDirectory || !dir.IsVersioned)
                    {
                        e.Enabled = false;
                        break;
                    }

                    foundOne = true;

                    if (!forHead)
                    {
                        Uri reposRoot = dir.WorkingCopy.RepositoryRoot;

                        if (root != reposRoot)
                        {
                            if (root == null)
                                reposRoot = root;
                            else
                            {
                                e.Enabled = false;
                                break;
                            }
                        }
                    }
                }

                if (!foundOne)
                {
                    e.Enabled = false;
                }
            }
            else
            {
                IProjectFileMapper pfm = null;
                IFileStatusCache fsc = null;

                Uri rootUrl = null;
                foreach (GitProject p in GetSelectedProjects(e))
                {
                    if (pfm == null)
                        pfm = e.GetService<IProjectFileMapper>();

                    IGitProjectInfo pi = pfm.GetProjectInfo(p);

                    if (pi == null || pi.ProjectDirectory == null)
                        continue;

                    if (fsc == null)
                        fsc = e.GetService<IFileStatusCache>();

                    GitItem rootItem = fsc[pi.ProjectDirectory];

                    if (!rootItem.IsVersioned)
                        continue;

                    if (IsHeadCommand(e.Command))
                        return; // Ok, we can update

                    if (rootUrl == null)
                        rootUrl = rootItem.WorkingCopy.RepositoryRoot;
                    else if (rootUrl != rootItem.WorkingCopy.RepositoryRoot)
                    {
                        // Multiple repositories selected; can't choose uniform version
                        e.Enabled = false;
                        return;
                    }
                }

                if (rootUrl == null)
                    e.Enabled = false;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            ILastChangeInfo ci = e.GetService<ILastChangeInfo>();

            if (ci != null)
                ci.SetLastChange(null, null);

            SvnRevision rev;
            bool allowUnversionedObstructions = false;
            bool updateExternals = true;
            bool setDepthInfinity = true;

            IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
            Uri reposRoot = null;

            if (IsHeadCommand(e.Command) || e.DontPrompt)
                rev = SvnRevision.Head;
            else if (IsSolutionCommand(e.Command))
            {
                GitItem projectItem = settings.ProjectRootGitItem;

                Debug.Assert(projectItem != null, "Has item");

                using (UpdateDialog ud = new UpdateDialog())
                {
                    ud.ItemToUpdate = projectItem;
                    ud.Revision = SvnRevision.Head;

                    if (ud.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    rev = ud.Revision;
                    allowUnversionedObstructions = ud.AllowUnversionedObstructions;
                    updateExternals = ud.UpdateExternals;
                    setDepthInfinity = ud.SetDepthInfinty;
                }
            }
            else if (IsFolderCommand(e.Command))
            {
                GitItem dirItem = EnumTools.GetFirst(e.Selection.GetSelectedGitItems(false));

                Debug.Assert(dirItem != null && dirItem.IsDirectory && dirItem.IsVersioned);

                using (UpdateDialog ud = new UpdateDialog())
                {
                    ud.Text = CommandStrings.UpdateFolder;
                    ud.FolderLabelText = CommandStrings.UpdateFolderLabel;
                    ud.ItemToUpdate = dirItem;
                    ud.Revision = SvnRevision.Head;

                    if (ud.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    rev = ud.Revision;
                    allowUnversionedObstructions = ud.AllowUnversionedObstructions;
                    updateExternals = ud.UpdateExternals;
                    setDepthInfinity = ud.SetDepthInfinty;
                }
            }
            else
            {
                // We checked there was only a single repository to select a revision 
                // from in OnUpdate, so we can suffice with only calculate the path

                GitItem si = null;
                GitOrigin origin = null;
                foreach (GitProject p in GetSelectedProjects(e))
                {
                    IGitProjectInfo pi = mapper.GetProjectInfo(p);
                    if (pi == null || pi.ProjectDirectory == null)
                        continue;

                    GitItem item = cache[pi.ProjectDirectory];
                    if (!item.IsVersioned)
                        continue;

                    if (si == null && origin == null)
                    {
                        si = item;
                        origin = new GitOrigin(item);
                        reposRoot = item.WorkingCopy.RepositoryRoot;
                    }
                    else
                    {
                        si = null;
                        string urlPath1 = origin.Uri.AbsolutePath;
                        string urlPath2 = item.Uri.AbsolutePath;

                        int i = 0;
                        while (i < urlPath1.Length && i < urlPath2.Length
                            && urlPath1[i] == urlPath2[i])
                        {
                            i++;
                        }

                        while (i > 0 && urlPath1[i - 1] != '/')
                            i--;

                        origin = new GitOrigin(new Uri(origin.Uri, urlPath1.Substring(0, i)), origin.RepositoryRoot);
                    }
                }

                Debug.Assert(origin != null);

                using (UpdateDialog ud = new UpdateDialog())
                {
                    ud.Text = CommandStrings.UpdateProject;

                    if (si != null)
                        ud.ItemToUpdate = si;
                    else
                    {
                        ud.GitOrigin = origin;
                        ud.SetMultiple(true);
                    }

                    ud.Revision = SvnRevision.Head;

                    if (ud.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    rev = ud.Revision;
                    allowUnversionedObstructions = ud.AllowUnversionedObstructions;
                    updateExternals = ud.UpdateExternals;
                    setDepthInfinity = ud.SetDepthInfinty;
                }
            }

            Dictionary<string, GitItem> itemsToUpdate = new Dictionary<string, GitItem>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Get a list of all documents below the specified paths that are open in editors inside VS
            HybridCollection<string> lockPaths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();

            foreach (GitItem item in GetAllUpdateRoots(e))
            {
                // GetAllUpdateRoots can (and probably will) return duplicates!

                if (itemsToUpdate.ContainsKey(item.FullPath) || !item.IsVersioned)
                    continue;

                GitWorkingCopy wc = item.WorkingCopy;

                if (!IsHeadCommand(e.Command) && reposRoot != null)
                {
                    // Specific revisions are only valid on a single repository!
                    if (wc != null && wc.RepositoryRoot != reposRoot)
                        continue;
                }

                List<string> inWc;

                if (!groups.TryGetValue(wc.FullPath, out inWc))
                {
                    inWc = new List<string>();
                    groups.Add(wc.FullPath, inWc);
                }

                inWc.Add(item.FullPath);
                itemsToUpdate.Add(item.FullPath, item);

                foreach (string file in documentTracker.GetDocumentsBelow(item.FullPath))
                {
                    if (!lockPaths.Contains(file))
                        lockPaths.Add(file);
                }
            }

            documentTracker.SaveDocuments(lockPaths); // Make sure all files are saved before updating/merging!

            using (DocumentLock lck = documentTracker.LockDocuments(lockPaths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            {
                SvnUpdateResult updateResult = null;

                ProgressRunnerArgs pa = new ProgressRunnerArgs();
                pa.CreateLog = true;

                string title;

                if (IsSolutionCommand(e.Command))
                    title = CommandStrings.UpdatingSolution;
                else if (IsFolderCommand(e.Command))
                    title = CommandStrings.UpdatingFolder;
                else
                    title = CommandStrings.UpdatingProject;

                e.GetService<IProgressRunner>().RunModal(title, pa,
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        PerformUpdate(e, a, rev, allowUnversionedObstructions, updateExternals, setDepthInfinity, groups.Values, out updateResult);
                    });

                if (ci != null && updateResult != null && IsSolutionCommand(e.Command))
                {
                    ci.SetLastChange("Updated to:", updateResult.Revision.ToString());
                }
            }
        }

        private static void PerformUpdate(CommandEventArgs e, ProgressWorkerArgs wa, SvnRevision rev, bool allowUnversionedObstructions, bool updateExternals, bool setDepthInfinity, IEnumerable<List<string>> groups, out SvnUpdateResult updateResult)
        {
            SvnUpdateArgs ua = new SvnUpdateArgs();
            ua.Revision = rev;
            ua.AllowObstructions = allowUnversionedObstructions;
            ua.IgnoreExternals = !updateExternals;
            ua.KeepDepth = setDepthInfinity;
            updateResult = null;

            HybridCollection<string> handledExternals = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);
            ua.Notify += delegate(object ss, SvnNotifyEventArgs ee)
            {
                if (ee.Action == SvnNotifyAction.UpdateExternal)
                {
                    if (!handledExternals.Contains(ee.FullPath))
                        handledExternals.Add(ee.FullPath);
                }
            };
            e.Context.GetService<IConflictHandler>().RegisterConflictHandler(ua, wa.Synchronizer);

            foreach (List<string> group in groups)
            {
                // Currently Git runs update per item passed and in
                // Git 1.6 passing each item separately is actually 
                // a tiny bit faster than passing them all at once. 
                // (sleep_for_timestamp fails its fast route)
                foreach (string path in group)
                {
                    if (handledExternals.Contains(path))
                        continue;

                    SvnUpdateResult result;
                    wa.SvnClient.Update(path, ua, out result);

                    if (updateResult == null)
                        updateResult = result; // Return the primary update as version for output
                }
            }
        }

        private static IEnumerable<GitItem> GetAllUpdateRoots(CommandEventArgs e)
        {
            // Duplicate handling is handled above this method!
            IVisualGitProjectLayoutService pls = e.GetService<IVisualGitProjectLayoutService>();
            if (IsSolutionCommand(e.Command))
                foreach (GitItem item in pls.GetUpdateRoots(null))
                {
                    yield return item;
                }
            else if (IsFolderCommand(e.Command))
                foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                {
                    // Everything is checked in the OnUpdate
                    yield return item;
                }
            else
                foreach (GitProject project in GetSelectedProjects(e))
                {
                    foreach (GitItem item in pls.GetUpdateRoots(project))
                    {
                        yield return item;
                    }
                }
        }
    }
}
