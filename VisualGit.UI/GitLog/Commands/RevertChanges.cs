using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpSvn;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogRevertThisRevisions, AlwaysAvailable = true)]
    [Command(VisualGitCommand.LogRevertTo, AlwaysAvailable = true)]
    class RevertChanges : ICommandHandler
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

            if (origin == null || !(origin.Target is SvnPathTarget))
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

            switch (e.Command)
            {
                case VisualGitCommand.LogRevertTo:
                    if (count == 1)
                        return;
                    break;
                case VisualGitCommand.LogRevertThisRevisions:
                    if (count > 0)
                        return;
                    break;
            }
            e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();
            IProgressRunner progressRunner = e.GetService<IProgressRunner>();

            if (logWindow == null)
                return;

            List<SvnRevisionRange> revisions = new List<SvnRevisionRange>();

            if (e.Command == VisualGitCommand.LogRevertTo)
            {
                IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

                if (item == null)
                    return;

                // Revert to revision, is revert everything after
                revisions.Add(new SvnRevisionRange(SvnRevision.Working, item.Revision));
            }
            else
            {
                foreach (IGitLogItem item in e.Selection.GetSelection<IGitLogItem>())
                {
                    revisions.Add(new SvnRevisionRange(item.Revision, item.Revision - 1));
                }
            }

            if (revisions.Count == 0)
                return;

            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();

            HybridCollection<string> nodes = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

            foreach (GitOrigin o in logWindow.Origins)
            {
                SvnPathTarget pt = o.Target as SvnPathTarget;
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
                        SvnPathTarget target = item.Target as SvnPathTarget;

                        if (target == null)
                            continue;

                        ee.SvnClient.Merge(target.FullPath, target, revisions, ma);
                    }
                });
            }
        }
    }
}
