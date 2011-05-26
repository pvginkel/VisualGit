using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc.UI;

namespace VisualGit.UI.SvnLog.Commands
{
    [Command(VisualGitCommand.LogIncludeMergedRevisions, AlwaysAvailable = true)]
    class IncludeMergedRevisions : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
            {
                e.Enabled = false;
                return;
            }

            e.Checked = lc.IncludeMergedRevisions;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
                return;

            lc.IncludeMergedRevisions = !lc.IncludeMergedRevisions;
        }
    }
}
