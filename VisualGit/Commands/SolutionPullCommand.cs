using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualGit.VS;
using SharpGit;
using System.Windows.Forms;
using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.PendingChangesPull)]
    [Command(VisualGitCommand.PendingChangesPullEx)]
    class SolutionPullCommand : CommandBase
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
            GitPullArgs args = new GitPullArgs();

            string repositoryRoot;
            var repositoryRoots = new HashSet<string>(FileSystemUtil.StringComparer);

            foreach (var projectRoot in GetAllRoots(e))
            {
                if (
                    RepositoryUtil.TryGetRepositoryRoot(projectRoot.FullPath, out repositoryRoot) &&
                    !repositoryRoots.Contains(repositoryRoot)
                )
                    repositoryRoots.Add(repositoryRoot);
            }

            if (repositoryRoots.Count > 1)
            {
                throw new InvalidOperationException("Pulling of multiple repository roots is not supported");
            }

            repositoryRoot = repositoryRoots.Single();

            if (e.Command == VisualGitCommand.PendingChangesPullEx)
            {
                if (!QueryParameters(e, repositoryRoot, args))
                    return;
            }
            else
            {
                args.MergeStrategy = GitMergeStrategy.DefaultForBranch;
            }

            GitPullResult result = null;

            ProgressRunnerArgs pa = new ProgressRunnerArgs();
            pa.CreateLog = true;
            pa.TransportClientArgs = args;

            args.AddExpectedError(GitErrorCode.PullFailed);

            e.GetService<IProgressRunner>().RunModal(CommandStrings.PullingSolution, pa,
                delegate(object sender, ProgressWorkerArgs a)
                {
                    using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                    {
                        client.Pull(repositoryRoot, args, out result);
                    }
                });

            if (args.LastException != null)
            {
                e.Context.GetService<IVisualGitDialogOwner>()
                    .MessageBox.Show(result.PostPullError,
                    args.LastException.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool QueryParameters(CommandEventArgs e, string repositoryRoot, GitPullArgs args)
        {
            using (var dialog = new PullDialog())
            {
                dialog.Args = args;
                dialog.RepositoryPath = repositoryRoot;

                return dialog.ShowDialog(e.Context) == DialogResult.OK;
            }
        }
    }
}
