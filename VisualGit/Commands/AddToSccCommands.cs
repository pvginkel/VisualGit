using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using VisualGit.Selection;
using VisualGit.UI;
using System.Windows.Forms;
using System.IO;
using SharpSvn;
using VisualGit.VS;
using VisualGit.UI.SccManagement;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.FileSccAddProjectToGit, HideWhenDisabled = false)]
    [Command(VisualGitCommand.FileSccAddSolutionToGit, AlwaysAvailable = true, HideWhenDisabled = false)]
    sealed class AddToSccCommands : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (!e.State.SolutionExists || (e.Command == VisualGitCommand.FileSccAddProjectToGit && e.State.EmptySolution))
            {
                e.Visible = e.Enabled = false;
                return;
            }

            if (e.State.OtherSccProviderActive)
            {
                e.Visible = e.Enabled = false;
                return; // Only one scc provider can be active at a time
            }

            IVisualGitSccService scc = e.GetService<IVisualGitSccService>();
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            if (scc == null || cache == null)
            {
                e.Visible = e.Enabled = false;
                return;
            }

            string solutionFilename = e.Selection.SolutionFilename;

            if (string.IsNullOrEmpty(solutionFilename) || !GitItem.IsValidPath(solutionFilename))
                solutionFilename = null;

            if (e.Command == VisualGitCommand.FileSccAddSolutionToGit)
            {
                if (solutionFilename == null || scc.IsSolutionManaged)
                {
                    e.Visible = e.Enabled = false; // Already handled
                    return;
                }
                GitItem item = cache[solutionFilename];

                if (!item.Exists || !item.IsFile)
                {
                    // Decide where you store the .sln first
                    e.Visible = e.Enabled = false;
                    return;
                }

                if (!item.IsVersioned)
                {
                    // If the .sln is ignored hide it in the context menus
                    // but don't hide it on 
                    e.HideOnContextMenu = item.IsIgnored && !e.Selection.IsSolutionSelected;
                }
                return;
            }

            IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();

            int n = 0;
            bool foundOne = false;
            foreach (IEnumerable<GitProject> projects in
                new IEnumerable<GitProject>[] 
                { 
                    e.Selection.GetSelectedProjects(true),
                    e.Selection.GetSelectedProjects(false) 
                })
            {
                foreach (GitProject p in projects)
                {
                    foundOne = true;

                    IGitProjectInfo pi = pfm.GetProjectInfo(p);

                    if (pi == null || !pi.IsSccBindable)
                        continue; // Not an SCC project

                    // A project is managed if the file says its managed
                    // and the project dir is managed
                    if (pi.ProjectDirectory != null && cache[pi.ProjectDirectory].IsVersioned
                        && scc.IsProjectManaged(p))
                        continue; // Nothing to do here

                    string projectFile = pi.ProjectFile;

                    if (n > 1 && projectFile != null && cache[projectFile].IsIgnored)
                        e.HideOnContextMenu = true;

                    return;
                }
                n++;
                if (foundOne)
                    break;
            }

            e.Visible = e.Enabled = false;
        }

        private static IEnumerable<GitProject> GetSelection(ISelectionContext iSelectionContext)
        {
            bool foundOne = false;
            foreach (GitProject pr in iSelectionContext.GetSelectedProjects(true))
            {
                yield return pr;
                foundOne = true;
            }

            if (foundOne)
                yield break;

            foreach (GitProject pr in iSelectionContext.GetOwnerProjects())
            {
                yield return pr;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IFileStatusCache cache = e.GetService<IFileStatusCache>();

            if (cache == null || e.Selection.SolutionFilename == null)
                return;

            GitItem item = cache[e.Selection.SolutionFilename];

            if (!HandleUnmanagedOrUnversionedSolution(e, item))
                return;

            if (e.Command == VisualGitCommand.FileSccAddSolutionToGit)
                return;

            SetProjectsManaged(e);
        }

        static bool HandleUnmanagedOrUnversionedSolution(CommandEventArgs e, GitItem solutionItem)
        {
            IVisualGitSccService scc = e.GetService<IVisualGitSccService>();
            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

            bool shouldActivate = false;
            if (!scc.IsActive)
            {
                if (e.State.OtherSccProviderActive)
                    return false; // Can't switch in this case.. Nothing to do

                // VisualGit is not the active provider, we should register as active
                shouldActivate = true;
            }

            if (scc.IsSolutionManaged && solutionItem.IsVersioned)
                return true; // Projects should still be checked

            bool confirmed = false;


            if (solutionItem.IsVersioned)
            { /* File is in Git; just enable */ }
            else if (solutionItem.IsVersionable)
            {
                if (!AddVersionableSolution(e, solutionItem, ref confirmed))
                    return false;
            }
            else
            {
                if (!CheckoutWorkingCopyForSolution(e, ref confirmed))
                    return false;
            }

            if (!confirmed && !e.DontPrompt && !e.IsInAutomation &&
                DialogResult.Yes != mb.Show(string.Format(CommandResources.MarkXAsManaged,
                Path.GetFileName(e.Selection.SolutionFilename)), "", MessageBoxButtons.YesNo))
            {
                return false;
            }

            SetSolutionManaged(shouldActivate, solutionItem, scc);

            return true;
        }

        static GitItem GetVersionedParent(GitItem child)
        {
            if (!child.IsVersionable)
                return null;

            if (!child.IsVersioned)
                return GetVersionedParent(child.Parent);
            return child;
        }

        static bool AddVersionableSolution(CommandEventArgs e, GitItem solutionItem, ref bool confirmed)
        {
            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);
            GitItem parentDir = GetVersionedParent(solutionItem);

            // File is not versioned but is inside a versioned directory
            if (!e.DontPrompt && !e.IsInAutomation)
            {
                if (!solutionItem.Parent.IsVersioned)
                {
                    AddPathToGit(e, e.Selection.SolutionFilename);

                    return true;
                }

                DialogResult rslt = mb.Show(string.Format(CommandResources.AddXToExistingWcY,
                                                          Path.GetFileName(e.Selection.SolutionFilename),
                                                          parentDir.FullPath), VisualGitId.PlkProduct, MessageBoxButtons.YesNoCancel);

                if (rslt == DialogResult.Cancel)
                    return false;
                if (rslt == DialogResult.No)
                {
                    // Checkout new working copy
                    return CheckoutWorkingCopyForSolution(e, ref confirmed);
                }
                if (rslt == DialogResult.Yes)
                {
                    // default case: Add to existing workingcopy
                    AddPathToGit(e, e.Selection.SolutionFilename);

                    return true;
                }
                return false;
            }

            confirmed = true;
            return true;
        }

        static void AddPathToGit(CommandEventArgs e, string path)
        {
            using (SvnClient cl = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                SvnAddArgs aa = new SvnAddArgs();
                aa.AddParents = true;
                cl.Add(path, aa);
            }
        }

        static void SetSolutionManaged(bool shouldActivate, GitItem item, IVisualGitSccService scc)
        {
            if (shouldActivate)
                scc.RegisterAsPrimarySccProvider();

            scc.SetProjectManaged(null, true);
            item.MarkDirty(); // This clears the solution settings cache to retrieve its properties
        }

        static bool CheckoutWorkingCopyForSolution(CommandEventArgs e, ref bool confirmed)
        {
            using (SvnClient cl = e.GetService<IGitClientPool>().GetClient())
            using (AddToGit dialog = new AddToGit())
            {
                dialog.PathToAdd = e.Selection.SolutionFilename;
                if (dialog.ShowDialog(e.Context) == DialogResult.OK)
                {
                    confirmed = true;
                    Collection<SvnInfoEventArgs> info;
                    SvnInfoArgs ia = new SvnInfoArgs();
                    ia.ThrowOnError = false;
                    if (!cl.GetInfo(dialog.RepositoryAddUrl, ia, out info))
                    {
                        // Target uri doesn't exist in the repository, let's create
                        if (!RemoteCreateDirectory(e, dialog.Text, dialog.RepositoryAddUrl, cl))
                            return false; // Create failed; bail out
                    }

                    // Create working copy
                    SvnCheckOutArgs coArg = new SvnCheckOutArgs();
                    coArg.AllowObstructions = true;
                    cl.CheckOut(dialog.RepositoryAddUrl, dialog.WorkingCopyDir, coArg);

                    // Add solutionfile so we can set properties (set managed)
                    AddPathToGit(e, e.Selection.SolutionFilename);

                    IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();
                    IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
                    IFileStatusMonitor monitor = e.GetService<IFileStatusMonitor>();

                    settings.ProjectRoot = SvnTools.GetNormalizedFullPath(dialog.WorkingCopyDir);

                    if (monitor != null && mapper != null)
                    {
                        // Make sure all visible glyphs are updated to reflect a new working copy
                        monitor.ScheduleGitStatus(mapper.GetAllFilesOfAllProjects());
                    }

                    return true;
                }

                return false; // User cancelled the "Add to Git" dialog, don't set as managed by VisualGit
            }
        }

        /// <summary>
        /// Creates the directory specified by <see cref="uri"/>
        /// Returns false if the user cancelled creating the directory, true otherwise
        /// </summary>
        /// <param name="e"></param>
        /// <param name="title">The title of the Create dialog</param>
        /// <param name="uri">The directory to be created</param>
        /// <param name="cl"></param>
        /// <returns></returns>
        static bool RemoteCreateDirectory(CommandEventArgs e, string title, Uri uri, SvnClient cl)
        {
            using (CreateDirectoryDialog createDialog = new CreateDirectoryDialog())
            {
                createDialog.Text = title; // Override dialog title with text from other dialog

                createDialog.NewDirectoryName = uri.ToString();
                createDialog.NewDirectoryReadonly = true;
                if (createDialog.ShowDialog(e.Context) == DialogResult.OK)
                {
                    // Create uri (including optional /trunk if required)
                    SvnCreateDirectoryArgs cdArg = new SvnCreateDirectoryArgs();
                    cdArg.CreateParents = true;
                    cdArg.LogMessage = createDialog.LogMessage;

                    cl.RemoteCreateDirectory(uri, cdArg);
                    return true;
                }

                return false; // bail out, we cannot continue without directory in the repository
            }
        }

        static void SetProjectsManaged(CommandEventArgs e)
        {
            IFileStatusCache cache = e.GetService<IFileStatusCache>();
            IFileStatusMonitor monitor = e.GetService<IFileStatusMonitor>();
            IVisualGitSccService scc = e.GetService<IVisualGitSccService>();
            IProjectFileMapper mapper = e.GetService<IProjectFileMapper>();
            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

            if (mapper == null)
                return;

            List<GitProject> projectsToBeManaged = new List<GitProject>();
            GitItem slnItem = cache[e.Selection.SolutionFilename];
            Uri solutionReposRoot = slnItem.WorkingCopy.RepositoryRoot;

            foreach (GitProject project in GetSelection(e.Selection))
            {
                IGitProjectInfo projInfo = mapper.GetProjectInfo(project);

                if (projInfo == null || projInfo.ProjectDirectory == null
                    || !projInfo.IsSccBindable)
                    continue; // Some projects can't be managed

                GitItem projectDir = cache[projInfo.ProjectDirectory];

                if (projectDir.WorkingCopy == slnItem.WorkingCopy)
                {
                    // This is a 'normal' project, part of the solution and in the same working copy
                    projectsToBeManaged.Add(project);
                    continue;
                }

                bool markAsManaged;
                bool writeReference;

                if (projectDir.IsVersioned)
                    continue; // We don't have to add this one
                if (projectDir.IsVersionable)
                {
                    GitItem parentDir = GetVersionedParent(projectDir);
                    Debug.Assert(parentDir != null);

                    DialogResult rslt = mb.Show(string.Format(CommandResources.AddXToExistingWcY,
                                                              Path.GetFileName(projInfo.ProjectName),
                                                              parentDir.FullPath), VisualGitId.PlkProduct, MessageBoxButtons.YesNoCancel);

                    switch (rslt)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.No:
                            if (CheckoutWorkingCopyForProject(e, projInfo, solutionReposRoot, out markAsManaged, out writeReference))
                            {
                                if (markAsManaged)
                                    scc.SetProjectManaged(project, true);
                                if (writeReference)
                                    scc.EnsureCheckOutReference(project);

                                continue;
                            }
                            break;
                        case DialogResult.Yes:
                            projectsToBeManaged.Add(project);
                            AddPathToGit(e, projInfo.ProjectFile ?? projInfo.ProjectDirectory);
                            continue;
                    }
                }
                else
                {
                    // We have to checkout (and create repository location)
                    if (CheckoutWorkingCopyForProject(e, projInfo, solutionReposRoot, out markAsManaged, out writeReference))
                    {
                        if (markAsManaged)
                            scc.SetProjectManaged(project, true);
                        if (writeReference)
                            scc.EnsureCheckOutReference(project);

                        continue;
                    }
                }
            }

            if (!AskSetManagedSelectionProjects(e, mapper, scc, projectsToBeManaged))
                return;

            foreach (GitProject project in projectsToBeManaged)
            {
                if (!scc.IsProjectManaged(project))
                {
                    scc.SetProjectManaged(project, true);

                    monitor.ScheduleGitStatus(mapper.GetAllFilesOf(project)); // Update for 'New' status
                }
            }
        }

        /// <summary>
        /// Returns true if <see cref="succeededProjects"/> should be set managed, false otherwise
        /// </summary>
        /// <param name="e"></param>
        /// <param name="mapper"></param>
        /// <param name="scc"></param>
        /// <param name="succeededProjects"></param>
        /// <returns></returns>
        static bool AskSetManagedSelectionProjects(CommandEventArgs e, IProjectFileMapper mapper, IVisualGitSccService scc, IEnumerable<GitProject> succeededProjects)
        {
            if (e.DontPrompt || e.IsInAutomation)
                return true;

            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);
            StringBuilder sb = new StringBuilder();
            bool foundOne = false;
            foreach (GitProject project in succeededProjects)
            {
                IGitProjectInfo info;
                if (!scc.IsProjectManaged(project) && null != (info = mapper.GetProjectInfo(project)))
                {
                    if (sb.Length > 0)
                        sb.Append("', '");

                    sb.Append(info.ProjectName);
                }

                foundOne = true;
            }

            if (!foundOne)
                return false; // No need to add when there are no projects

            string txt = sb.ToString();
            int li = txt.LastIndexOf("', '");
            if (li > 0)
                txt = txt.Substring(0, li + 1) + CommandResources.FileAnd + txt.Substring(li + 3);

            return DialogResult.Yes == mb.Show(string.Format(CommandResources.MarkXAsManaged,
                txt), VisualGitId.PlkProduct, MessageBoxButtons.YesNo);
        }

        /// <summary>
        /// Returns false if the AddToGitDialog has been cancelled, true otherwise
        /// </summary>
        /// <param name="e"></param>
        /// <param name="projectInfo"></param>
        /// <param name="solutionReposRoot"></param>
        /// <param name="shouldMarkAsManaged"></param>
        /// <param name="storeReference"></param>
        /// <returns></returns>
        static bool CheckoutWorkingCopyForProject(CommandEventArgs e, IGitProjectInfo projectInfo, Uri solutionReposRoot, out bool shouldMarkAsManaged, out bool storeReference)
        {
            shouldMarkAsManaged = false;
            storeReference = false;
            using (SvnClient cl = e.GetService<IGitClientPool>().GetClient())
            using (AddProjectToGit dialog = new AddProjectToGit())
            {
                dialog.Context = e.Context;
                dialog.PathToAdd = projectInfo.ProjectDirectory;
                dialog.RepositoryAddUrl = solutionReposRoot;
                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return false; // User cancelled the "Add to Git" dialog, don't set as managed by VisualGit


                Collection<SvnInfoEventArgs> info;
                SvnInfoArgs ia = new SvnInfoArgs();
                ia.ThrowOnError = false;
                if (!cl.GetInfo(dialog.RepositoryAddUrl, ia, out info))
                {
                    // Target uri doesn't exist in the repository, let's create
                    if (!RemoteCreateDirectory(e, dialog.Text, dialog.RepositoryAddUrl, cl))
                        return false; // Create failed; bail out
                }

                // Create working copy
                SvnCheckOutArgs coArg = new SvnCheckOutArgs();
                coArg.AllowObstructions = true;
                cl.CheckOut(dialog.RepositoryAddUrl, dialog.WorkingCopyDir, coArg);

                shouldMarkAsManaged = dialog.MarkAsManaged;
                storeReference = dialog.WriteCheckOutInformation;
            }
            return true;
        }
    }
}
