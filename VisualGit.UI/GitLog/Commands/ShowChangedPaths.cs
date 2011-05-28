using System;
using System.Collections.Generic;
using VisualGit.Commands;
using VisualGit.Scc.UI;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogShowChangedPaths, AlwaysAvailable = true)]
    class ShowChangedPaths : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
            {
                e.Enabled = false;
                return;
            }

            e.Checked = lc.ShowChangedPaths;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
                return;

            lc.ShowChangedPaths = !lc.ShowChangedPaths;
        }
    }
}
