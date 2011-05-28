using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc.UI;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogFetchAll, AlwaysAvailable=true)]
    class FetchAll : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
            {
                e.Enabled = false;
                return;
            }
        }
        public void OnExecute(CommandEventArgs e)
        {
            ILogControl lc = e.Selection.GetActiveControl<ILogControl>();

            if (lc == null)
                return;

            lc.FetchAll();
        }
    }
}
