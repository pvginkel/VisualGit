using System;
using VisualGit.VS;
using VisualGit.Scc;
using SharpSvn;
using VisualGit.UI.SccManagement;
using System.Windows.Forms;
using VisualGit.UI;
using VisualGit.Selection;
using SharpGit;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ProjectBranch)]
    [Command(VisualGitCommand.SolutionBranch)]
    class BranchSolutionCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            GitItem item = GetRoot(e);

            if(item == null || !item.IsVersioned || item.IsDeleteScheduled || item.Status.State == GitStatus.Added || item.Uri == null)
                e.Enabled = false;
        }

        private static GitItem GetRoot(BaseCommandEventArgs e)
        {
            GitItem item = null;
            switch (e.Command)
            {
                case VisualGitCommand.SolutionBranch:
                    IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
                    if (ss == null)
                        return null;

                    string root = ss.ProjectRoot;

                    if (string.IsNullOrEmpty(root))
                        return null;

                    item = e.GetService<IFileStatusCache>()[root];
                    break;
                case VisualGitCommand.ProjectBranch:
                    GitProject p = EnumTools.GetSingle(e.Selection.GetSelectedProjects(false));
                    if(p == null)
                        break;

                    IGitProjectInfo info = e.GetService<IProjectFileMapper>().GetProjectInfo(p);

                    if (info == null || info.ProjectDirectory == null)
                        break;

                    item = e.GetService<IFileStatusCache>()[info.ProjectDirectory];
                    break;
            }

            return item;
        }

        public override void OnExecute(CommandEventArgs e)
        {         
            GitItem root = GetRoot(e);

            if (root == null)
                return;

            using (CreateBranchDialog dlg = new CreateBranchDialog())
            {
                if (e.Command == VisualGitCommand.ProjectBranch)
                    dlg.Text = CommandStrings.BranchProject;

                dlg.SrcFolder = root.FullPath;
                dlg.SrcUri = root.Uri;
                dlg.EditSource = false;

                dlg.Revision = root.Status.Revision;

                RepositoryLayoutInfo info;
                if (RepositoryUrlUtils.TryGuessLayout(e.Context, root.Uri, out info))
                    dlg.NewDirectoryName = new Uri(info.BranchesRoot, ".");

                while (true)
                {
                    if (DialogResult.OK != dlg.ShowDialog(e.Context))
                        return;

                    string msg = dlg.LogMessage;

                    bool retry = false;
                    bool ok = false;
                    ProgressRunnerResult rr =
                        e.GetService<IProgressRunner>().RunModal("Creating Branch/Tag",
                        delegate(object sender, ProgressWorkerArgs ee)
                        {
                            SvnInfoArgs ia = new SvnInfoArgs();
                            ia.ThrowOnError = false;

                            if (ee.SvnClient.Info(dlg.NewDirectoryName, ia, null))
                            {
                                DialogResult dr = DialogResult.Cancel;

                                ee.Synchronizer.Invoke((VisualGitAction)
                                    delegate
                                    {
                                        VisualGitMessageBox mb = new VisualGitMessageBox(ee.Context);
                                        dr = mb.Show(string.Format("The Branch/Tag at Url '{0}' already exists.", dlg.NewDirectoryName),
                                            "Path Exists", MessageBoxButtons.RetryCancel);
                                    }, null);

                                if (dr == DialogResult.Retry)
                                {
                                    // show dialog again to let user modify the branch URL
                                    retry = true;
                                }
                            }
                            else
                            {
                                SvnCopyArgs ca = new SvnCopyArgs();
                                ca.CreateParents = true;
                                ca.LogMessage = msg;

                                ok = dlg.CopyFromUri ?
                                    ee.SvnClient.RemoteCopy(new SvnUriTarget(dlg.SrcUri, dlg.SelectedRevision), dlg.NewDirectoryName, ca) :
                                    ee.SvnClient.RemoteCopy(new SvnPathTarget(dlg.SrcFolder), dlg.NewDirectoryName, ca);
                            }
                        });

                    if (rr.Succeeded && ok && dlg.SwitchToBranch)
                    {
                        e.GetService<IVisualGitCommandService>().PostExecCommand(VisualGitCommand.SolutionSwitchDialog, dlg.NewDirectoryName);
                    }

                    if (!retry)
                        break;
                }
            }
        }
    }
}
