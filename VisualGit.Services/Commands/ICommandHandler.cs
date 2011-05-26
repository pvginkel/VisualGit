using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Commands
{
    public interface ICommandHandler
    {
        void OnUpdate(CommandUpdateEventArgs e);
        void OnExecute(CommandEventArgs e);        
    }
}
