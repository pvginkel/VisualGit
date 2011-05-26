using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemAddToPending)]
    [Command(VisualGitCommand.ItemRemoveFromPending)]
    [Command(VisualGitCommand.DocumentAddToPending)]
    [Command(VisualGitCommand.DocumentRemoveFromPending)]
    class ItemAddToPending : CommandBase
    {
        IEnumerable<GitItem> GetSelection(BaseCommandEventArgs e)
        {
            if (e.Command == VisualGitCommand.DocumentAddToPending || e.Command == VisualGitCommand.DocumentRemoveFromPending)
            {
                GitItem i = e.Selection.ActiveDocumentItem;
                if (i == null)
                    return new GitItem[0];
                else
                    return new GitItem[] { i };
            }
            else
                return e.Selection.GetSelectedGitItems(false);
        }

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool add;
            IPendingChangesManager pcm = null;

            add = (e.Command == VisualGitCommand.ItemAddToPending) || (e.Command == VisualGitCommand.DocumentAddToPending);

            foreach (GitItem i in GetSelection(e))
            {
                if (i.InSolution || !PendingChange.IsPending(i))
                    continue;

                if (pcm == null)
                {
                    pcm = e.GetService<IPendingChangesManager>();
                    if (pcm == null)
                        break;
                }

                if (pcm.Contains(i.FullPath) != add)
                    return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IFileStatusMonitor fsm = e.GetService<IFileStatusMonitor>();

            foreach (GitItem i in GetSelection(e))
            {
                if (i.InSolution)
                    continue;

                if (e.Command == VisualGitCommand.ItemAddToPending || e.Command == VisualGitCommand.DocumentAddToPending)
                    fsm.ScheduleMonitor(i.FullPath);
                else
                    fsm.StopMonitoring(i.FullPath);
            }
        }
    }
}
