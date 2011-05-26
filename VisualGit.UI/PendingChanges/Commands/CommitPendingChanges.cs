using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.CommitPendingChanges)]
    [Command(VisualGitCommand.CommitPendingChangesKeepingLocks)]
    [Command(VisualGitCommand.PendingChangesApplyWorkingCopy)]
    class CommitPendingChanges : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            PendingCommitsPage commitPage = e.Context.GetService<PendingCommitsPage>();
            PendingIssuesPage issuesPage = e.Context.GetService<PendingIssuesPage>();
            if (commitPage == null)
            {
                e.Enabled = false;
                return;
            }

            switch (e.Command)
            {
                case VisualGitCommand.CommitPendingChanges:
                case VisualGitCommand.CommitPendingChangesKeepingLocks:
                    e.Enabled = true
                        // check if commit page or issues page is visible
                        && (false
                             || commitPage.Visible
                             || (issuesPage != null && issuesPage.Visible)
                             )
                         // make sure commit page can commit
                         && commitPage.CanCommit(e.Command == VisualGitCommand.CommitPendingChangesKeepingLocks)
                         ;
                    break;
                case VisualGitCommand.PendingChangesApplyWorkingCopy:
                    e.Enabled = commitPage.Visible && commitPage.CanApplyToWorkingCopy();
                    break;
            }
        }

        public void OnExecute(CommandEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if (page != null)
                switch (e.Command)
                {
                    case VisualGitCommand.CommitPendingChanges:
                    case VisualGitCommand.CommitPendingChangesKeepingLocks:
                        page.DoCommit(e.Command == VisualGitCommand.CommitPendingChangesKeepingLocks);
                        break;
                    case VisualGitCommand.PendingChangesApplyWorkingCopy:
                        page.ApplyToWorkingCopy();
                        break;
                }
        }
    }
}
