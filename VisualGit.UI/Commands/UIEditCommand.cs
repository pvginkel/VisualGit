using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;

namespace VisualGit.UI.Commands
{
    [Command(VisualGitCommand.ForceUIShow, AlwaysAvailable=true)]
    sealed class UIEditCommand : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            e.GetService<CommandMapper>().EnableCustomizeMode();
            e.Enabled = e.Visible = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            throw new NotSupportedException();
        }
    }
}
