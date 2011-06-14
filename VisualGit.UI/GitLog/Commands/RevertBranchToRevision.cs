using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpSvn;
using SharpGit;
using VisualGit.UI.Commands;
using System.Windows.Forms;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogRevertTo, AlwaysAvailable = true)]
    class RevertBranchToRevision : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

            if (logWindow == null)
            {
                e.Enabled = false;
                return;
            }

            GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);

            if (origin == null || !(origin.Target is GitPathTarget))
            {
                e.Enabled = false;
                return;
            }

            int count = 0;
            foreach (IGitLogItem item in e.Selection.GetSelection<IGitLogItem>())
            {
                count++;

                if (count > 1)
                    break;
            }

            e.Enabled = count == 1;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();
            IProgressRunner progressRunner = e.GetService<IProgressRunner>();

            if (logWindow == null)
                return;

            IGitLogItem logItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            if (logItem == null)
                return;

            GitResetType type;

            using (var dialog = new ResetBranchDialog())
            {
                dialog.Revision = logItem.Revision;
                dialog.RepositoryPath = GitTools.GetAbsolutePath(logItem.RepositoryRoot);

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                type = dialog.ResetType;
            }

            // Revert to revision, is revert everything after
            var revision = new GitRevisionRange(GitRevision.Working, logItem.Revision);

            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();

            HybridCollection<string> nodes = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

            foreach (GitOrigin o in logWindow.Origins)
            {
                GitPathTarget pt = o.Target as GitPathTarget;
                if (pt == null)
                    continue;

                foreach (string file in tracker.GetDocumentsBelow(pt.FullPath))
                {
                    if (!nodes.Contains(file))
                        nodes.Add(file);
                }
            }

            if (nodes.Count > 0)
                tracker.SaveDocuments(nodes); // Saves all open documents below all specified origins


            using (DocumentLock dl = tracker.LockDocuments(nodes, DocumentLockType.NoReload))
            using (dl.MonitorChangesForReload())
            {
                SvnMergeArgs ma = new SvnMergeArgs();

                progressRunner.RunModal("Reverting",
                delegate(object sender, ProgressWorkerArgs ee)
                {
                    foreach (GitOrigin item in logWindow.Origins)
                    {
                        GitPathTarget target = item.Target as GitPathTarget;

                        if (target == null)
                            continue;

                        var args = new GitResetArgs();

                        ee.Client.Reset(
                            GitTools.GetAbsolutePath(logItem.RepositoryRoot),
                            logItem.Revision,
                            type,
                            args
                        );
                    }
                });
            }
        }
    }
}
