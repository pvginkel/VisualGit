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
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            GitItem theItem = null;
            string path;

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
            GitRef currentBranch;

            using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                currentBranch = client.GetCurrentBranch(pathItem.FullPath);
            }

            if (currentBranch == null)
                return; // Should never happen on a real workingcopy

            GitRef target;
            bool force = false;

            if (e.Argument is string)
            {
                target = new GitRef((string)e.Argument);
            }
            else if (e.Argument is Uri)
                throw new NotImplementedException();
            else
                using (SwitchDialog dlg = new SwitchDialog())
                {
                    dlg.GitOrigin = new GitOrigin(pathItem);
                    dlg.Context = e.Context;

                    dlg.LocalPath = RepositoryUtil.GetRepositoryRoot(path);
                    dlg.SwitchToBranch = currentBranch;

                    if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    target = dlg.SwitchToBranch;
                    force = dlg.Force;
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
                GitSwitchResult result = null;
                GitSwitchArgs args = new GitSwitchArgs();

                e.GetService<IProgressRunner>().RunModal(
                    "Changing Current Branch",
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        args.Force = force;
                        args.AddExpectedError(GitErrorCode.CheckoutFailed);

#if false
                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, a.Synchronizer);
#endif
                        a.Client.Switch(path, target, args, out result);
                    });

                if (args.LastException != null)
                {
                    e.Context.GetService<IVisualGitDialogOwner>()
                        .MessageBox.Show(result.PostSwitchError,
                        args.LastException.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
