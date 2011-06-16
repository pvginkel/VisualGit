﻿using System;
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

            bool createCommit;

            using (var dialog = new RevertDialog())
            {
                dialog.Revision = logItem.Revision;
                dialog.RepositoryPath = GitTools.GetAbsolutePath(logItem.RepositoryRoot);

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                createCommit = dialog.CreateCommit;
            }

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
                progressRunner.RunModal("Reverting Revisions",
                delegate(object sender, ProgressWorkerArgs ee)
                {
                    foreach (GitOrigin item in logWindow.Origins)
                    {
                        GitPathTarget target = item.Target as GitPathTarget;

                        if (target == null)
                            continue;

                        GitRevertArgs args = new GitRevertArgs();

                        args.CreateCommit = createCommit;

                        e.GetService<IConflictHandler>().RegisterConflictHandler(args, ee.Synchronizer);

                        ee.Client.Revert(
                            GitTools.GetAbsolutePath(logItem.RepositoryRoot),
                            logItem.Revision,
                            args
                        );
                    }
                });
            }
        }
    }
}
