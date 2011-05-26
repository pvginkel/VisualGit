using System;
using VisualGit.Commands;

namespace VisualGit.Scc.Commands
{
    [Command(VisualGitCommand.MarkProjectDirty, AlwaysAvailable = true)]
    public class MarkProjectDirty : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        IVisualGitCommandService _commandService;
        ProjectNotifier _projectNotifier;        

        public void OnExecute(CommandEventArgs e)
        {
            if (_commandService == null)
                _commandService = e.GetService<IVisualGitCommandService>();
            if (_projectNotifier == null)
                _projectNotifier = e.GetService<ProjectNotifier>(typeof(IFileStatusMonitor));            

            _commandService.TockCommand(e.Command);

            _projectNotifier.HandleEvent(e.Command);
        }
    }
}
