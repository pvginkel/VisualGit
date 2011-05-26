using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.PendingChangesViewLogMessage)]
    public class ViewLogMessage : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if(page == null || !page.Visible || e.Selection.ActiveDialog != null)
            {
                e.Enabled = false;
                return;
            }

            e.Checked = page.LogMessageVisible;
        }

        public void OnExecute(CommandEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if (page != null)
                page.LogMessageVisible = !page.LogMessageVisible;
        }
    }
}
