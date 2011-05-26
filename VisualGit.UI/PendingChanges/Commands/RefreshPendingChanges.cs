using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.UI.PendingChanges.Commits;
using VisualGit.UI.PendingChanges.Synchronize;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.RefreshPendingChanges)]
    public class RefreshPendingChanges : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            if (!e.State.SolutionExists)
            {
                e.Enabled = false;
                return;
            }
            PendingChangesPage page = GetPage(e);

            if (page == null || !page.CanRefreshList)
                e.Enabled = false;
        }

        private PendingChangesPage GetPage(BaseCommandEventArgs e)
        {
            PendingChangesPage page = e.Context.GetService<PendingCommitsPage>();

            if (page != null && page.Visible)
                return page;

            page = e.Context.GetService<RecentChangesPage>();

            if (page != null && page.Visible)
                return page;

            return null;
        }

        public void OnExecute(CommandEventArgs e)
        {
            PendingChangesPage page = GetPage(e);

            if(page != null && page.CanRefreshList)
                page.RefreshList();
        }
    }
}
