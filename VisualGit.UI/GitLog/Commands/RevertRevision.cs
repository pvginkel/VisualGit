using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpGit;
using VisualGit.UI.Commands;
using System.Windows.Forms;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogRevertThisRevision, AlwaysAvailable = true)]
    class RevertRevision : ICommandHandler
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

            if (origin == null)
            {
                e.Enabled = false;
                return;
            }

            IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            e.Enabled = item != null;
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

            bool createCommit;

            using (var dialog = new RevertDialog())
            {
                dialog.Revision = logItem.Revision;
                dialog.RepositoryPath = logItem.RepositoryRoot;

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                createCommit = dialog.CreateCommit;
            }

            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();

            HybridCollection<string> nodes = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

            foreach (GitOrigin o in logWindow.Origins)
            {
                foreach (string file in tracker.GetDocumentsBelow(o.Target.FullPath))
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
                progressRunner.RunModal("Reverting Revisions",
                delegate(object sender, ProgressWorkerArgs ee)
                {
                    foreach (GitOrigin item in logWindow.Origins)
                    {
                        GitRevertArgs args = new GitRevertArgs();

                        args.CreateCommit = createCommit;

                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, ee.Synchronizer);

                        ee.Client.Revert(
                            logItem.RepositoryRoot,
                            logItem.Revision,
                            args
                        );
                    }
                });
            }
        }
    }
}
