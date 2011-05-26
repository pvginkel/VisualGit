using System;
using VisualGit.Commands;

namespace VisualGit.Scc.Commands
{
    [Command(VisualGitCommand.TickRefreshPendingTasks, AlwaysAvailable=true)]
    sealed class PendingChangeTicker : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            // NOOP
        }

        IVisualGitCommandService _commandService;
        PendingChangeManager _pendingChanges;        

        public void OnExecute(CommandEventArgs e)
        {
            if (_commandService == null)
                _commandService = e.GetService<IVisualGitCommandService>();
            if (_pendingChanges == null)
                _pendingChanges = e.GetService<PendingChangeManager>(typeof(IPendingChangesManager));            

            _commandService.TockCommand(e.Command);

            _pendingChanges.OnTickRefresh();
        }
    }
}
