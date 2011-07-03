using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualGit.VS;
using SharpGit;
using VisualGit.Scc;
using System.Windows.Forms;
using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.PendingChangesPush)]
    [Command(VisualGitCommand.PendingChangesPushSpecificBranch)]
    [Command(VisualGitCommand.PendingChangesPushSpecificTag)]
    class SolutionPushCommand : CommandBase
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
            GitPushArgs args = new GitPushArgs();

            string repositoryRoot;
            var repositoryRoots = new HashSet<string>(FileSystemUtil.StringComparer);

            foreach (var projectRoot in GetAllRoots(e))
            {
                if (
                    GitTools.TryGetRepositoryRoot(projectRoot.FullPath, out repositoryRoot) &&
                    !repositoryRoots.Contains(repositoryRoot)
                )
                    repositoryRoots.Add(repositoryRoot);
            }

            if (repositoryRoots.Count > 1)
            {
                throw new InvalidOperationException("Pushing of multiple repository roots is not supported");
            }

            repositoryRoot = repositoryRoots.Single();

            switch (e.Command)
            {
                case VisualGitCommand.PendingChangesPushSpecificBranch:
                case VisualGitCommand.PendingChangesPushSpecificTag:
                    if (!QueryParameters(e, repositoryRoot, args))
                        return;
                    break;
            }

            ProgressRunnerArgs pa = new ProgressRunnerArgs();
            pa.CreateLog = true;
            pa.TransportClientArgs = args;

            GitException exception = null;

            e.GetService<IProgressRunner>().RunModal(CommandStrings.PushingSolution, pa,
                delegate(object sender, ProgressWorkerArgs a)
                {
                    using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                    {
                        try
                        {
                            client.Push(repositoryRoot, args);
                        }
                        catch (GitException ex)
                        {
                            exception = ex;
                        }
                    }
                });
            
            if (exception != null)
            {
                e.GetService<IVisualGitErrorHandler>().OnWarning(exception);
            }
        }

        private bool QueryParameters(CommandEventArgs e, string repositoryRoot, GitPushArgs args)
        {
            using (var dialog = new PushDialog())
            {
                dialog.Args = args;
                dialog.RepositoryPath = repositoryRoot;

                switch (e.Command)
                {
                    case VisualGitCommand.PendingChangesPushSpecificBranch:
                        dialog.Type = PushDialog.PushType.Branch;
                        break;

                    case VisualGitCommand.PendingChangesPushSpecificTag:
                        dialog.Type = PushDialog.PushType.Tag;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                return dialog.ShowDialog(e.Context) == DialogResult.OK;
            }
        }
    }
}
