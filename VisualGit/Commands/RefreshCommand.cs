using VisualGit.Scc;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to refresh this view.
    /// </summary>
    [Command(VisualGitCommand.Refresh)]
    class RefreshCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (null == EnumTools.GetFirst(e.Selection.GetSelectedFiles(true)))
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            // Refresh all global states on the selected files
            // * File Status Cache
            // * Glyph cache (in VS Projects)
            // * Pending changes
            // * Editor dirty state

            // Don't handle individual windows here, they can just override the refresh handler

            // See WorkingCopyExplorerControl.OnFrameCreated() for some examples on how to do that

            IFileStatusMonitor monitor = e.GetService<IFileStatusMonitor>();

            monitor.ScheduleGitStatus(e.Selection.GetSelectedFiles(true));

            IVisualGitOpenDocumentTracker dt = e.GetService<IVisualGitOpenDocumentTracker>();

            dt.RefreshDirtyState();

            IPendingChangesManager pm = e.GetService<IPendingChangesManager>();

            pm.Refresh((string)null); // Perform a full incremental refresh on the PC window            
        }
    }
}
