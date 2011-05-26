using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.UI;

namespace VisualGit.Scc.Commands
{
    [Command(VisualGitCommand.NotifyWcToNew, AlwaysAvailable=true)]
    class UnsupportedWcVersion : ICommandHandler
    {

        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        bool _skip;
        public void OnExecute(CommandEventArgs e)
        {
            if(_skip) // Only show this message once!
                return;

            _skip = true;
            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);
            mb.Show(string.Format(Resources.UnsupportedWorkingCopyFound, e.Argument));
        }
    }
}
