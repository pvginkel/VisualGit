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
    [Command(VisualGitCommand.ProjectTag)]
    [Command(VisualGitCommand.SolutionTag)]
    class BranchTagSolutionCommand : CommandBase
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
                case VisualGitCommand.SolutionTag:
                    IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
                    if (ss == null)
                        return null;

                    string root = ss.ProjectRoot;

                    if (string.IsNullOrEmpty(root))
                        return null;

                    item = e.GetService<IFileStatusCache>()[root];
                    break;
                case VisualGitCommand.ProjectBranch:
                case VisualGitCommand.ProjectTag:
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

            switch (e.Command)
            {
                case VisualGitCommand.SolutionBranch:
                case VisualGitCommand.ProjectBranch:
                    PerformExecuteBranch(e, root);
                    break;

                case VisualGitCommand.SolutionTag:
                case VisualGitCommand.ProjectTag:
                    PerformExecuteTag(e, root);
                    break;
            }

        }

        private void PerformExecuteBranch(CommandEventArgs e, GitItem root)
        {
            using (CreateBranchDialog dlg = new CreateBranchDialog())
            {
                dlg.GitOrigin = new GitOrigin(root);

                if (e.Command == VisualGitCommand.ProjectBranch)
                    dlg.Text = CommandStrings.BranchProject;

                while (true)
                {
                    if (DialogResult.OK != dlg.ShowDialog(e.Context))
                        return;

                    bool retry = false;
                    bool ok = false;
                    ProgressRunnerResult rr =
                        e.GetService<IProgressRunner>().RunModal("Creating Branch",
                        delegate(object sender, ProgressWorkerArgs ee)
                        {
                            string branchName = dlg.BranchName;

                            if (NameExists(branchName, ee, root))
                            {
                                DialogResult dr = DialogResult.None;

                                ee.Synchronizer.Invoke((VisualGitAction)
                                    delegate
                                    {
                                        VisualGitMessageBox mb = new VisualGitMessageBox(ee.Context);
                                        dr = mb.Show(string.Format("The branch '{0}' already exists.", branchName),
                                            "Branch Exists", MessageBoxButtons.RetryCancel);
                                    }, null);

                                if (dr == DialogResult.Retry)
                                {
                                    // show dialog again to let user modify the branch URL
                                    retry = true;
                                }
                            }
                            else
                            {
                                GitBranchArgs args = new GitBranchArgs();

                                args.Force = dlg.Force;
                                args.Revision = dlg.Revision;
                                args.SwitchToBranch = dlg.SwitchToBranch;

                                ee.Client.Branch(
                                    RepositoryUtil.GetRepositoryRoot(root.FullPath),
                                    branchName,
                                    args
                                );
                            }
                        });

                    if (!retry)
                        break;
                }
            }
        }

        private void PerformExecuteTag(CommandEventArgs e, GitItem root)
        {
            using (CreateTagDialog dlg = new CreateTagDialog())
            {
                if (e.Command == VisualGitCommand.ProjectTag)
                    dlg.Text = CommandStrings.TagProject;

                while (true)
                {
                    if (DialogResult.OK != dlg.ShowDialog(e.Context))
                        return;

                    bool retry = false;
                    bool ok = false;
                    ProgressRunnerResult rr =
                        e.GetService<IProgressRunner>().RunModal("Creating Tag",
                            delegate(object sender, ProgressWorkerArgs ee)
                            {
                                string tagName = dlg.TagName;

                                if (NameExists(tagName, ee, root))
                                {
                                    DialogResult dr = DialogResult.None;

                                    ee.Synchronizer.Invoke((VisualGitAction)
                                        delegate
                                        {
                                            VisualGitMessageBox mb = new VisualGitMessageBox(ee.Context);
                                            dr = mb.Show(string.Format("The tag '{0}' already exists.", tagName),
                                                "Tag Exists", MessageBoxButtons.RetryCancel);
                                        }, null);

                                    if (dr == DialogResult.Retry)
                                    {
                                        // show dialog again to let user modify the branch URL
                                        retry = true;
                                    }
                                }
                                else
                                {
                                    GitTagArgs args = new GitTagArgs();

                                    if (dlg.AnnotatedTag)
                                        args.Message = dlg.LogMessage;

                                    ee.Client.Tag(
                                        RepositoryUtil.GetRepositoryRoot(root.FullPath),
                                        tagName,
                                        args
                                    );
                                }
                            });

                    if (!retry)
                        break;
                }
            }
        }

        private bool NameExists(string name, ProgressWorkerArgs ee, GitItem root)
        {
            foreach (var @ref in ee.Client.GetRefs(root.FullPath))
            {
                switch (@ref.Type)
                {
                    case GitRefType.Branch:
                    case GitRefType.Tag:
                        if (String.Equals(@ref.ShortName, name, StringComparison.Ordinal))
                            return true;
                        break;
                }
            }

            return false;
        }
    }
}
