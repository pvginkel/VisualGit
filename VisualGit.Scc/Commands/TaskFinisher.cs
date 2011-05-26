using System;
using VisualGit.Commands;

namespace VisualGit.Scc.Commands
{
    /// <summary>
    /// Handles the finishtasks special command; this command is posted to the back of the command queueue
    /// if the SCC implementation needs to perform some post processing of VSs scc actions
    /// </summary>
    [Command(VisualGitCommand.SccFinishTasks, AlwaysAvailable=true)]
    sealed class TaskFinisher : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        IVisualGitCommandService _commandService;
        ProjectTracker _projectTracker;
        VisualGitSccProvider _sccProvider;

        public void OnExecute(CommandEventArgs e)
        {
            if (_commandService == null)
                _commandService = e.GetService<IVisualGitCommandService>();
            if (_projectTracker == null)
                _projectTracker = e.GetService<ProjectTracker>(typeof(IVisualGitProjectDocumentTracker));
            if(_sccProvider == null)
                _sccProvider = e.GetService<VisualGitSccProvider>(typeof(IVisualGitSccService));            

            _commandService.TockCommand(e.Command);

            _projectTracker.OnSccCleanup(e);
            _sccProvider.OnSccCleanup(e);            
        }
    }
}
