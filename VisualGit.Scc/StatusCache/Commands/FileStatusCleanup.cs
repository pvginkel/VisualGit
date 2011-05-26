using System;
using VisualGit.Commands;

namespace VisualGit.Scc.StatusCache.Commands
{
    [Command(VisualGitCommand.FileCacheFinishTasks, AlwaysAvailable = true)]
    [Command(VisualGitCommand.TickRefreshGitItems, AlwaysAvailable = true)]
    public class FileStatusCleanup : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        FileStatusCache _fileCache;
        IVisualGitCommandService _commandService;

        public void OnExecute(CommandEventArgs e)
        {
            if (_commandService == null)
                _commandService = e.GetService<IVisualGitCommandService>();
            if (_fileCache == null)
                _fileCache = e.GetService<FileStatusCache>(typeof(IFileStatusCache));

            _commandService.TockCommand(e.Command);

            if (e.Command == VisualGitCommand.FileCacheFinishTasks)
                _fileCache.OnCleanup();
            else
                _fileCache.BroadcastChanges();
        }
    }
}
