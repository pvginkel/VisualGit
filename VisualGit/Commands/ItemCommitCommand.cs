using System.Windows.Forms;
using System.Collections.Generic;
using VisualGit.Scc;
using VisualGit.UI.SccManagement;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to commit selected items to the Git repository.
    /// </summary>
    [Command(VisualGitCommand.CommitItem)]
    class ItemCommitCommand : CommandBase
    {
        string storedLogMessage;

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem i in e.Selection.GetSelectedGitItems(true))
            {
                if (i.IsVersioned)
                {
                    if (i.IsModified || i.IsDocumentDirty)
                        return; // There might be a new version of this file
                }
                else if (i.IsIgnored)
                    continue;
                else if (i.InSolution && i.IsVersionable)
                    return; // The file is 'to be added'
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            using (ProjectCommitDialog pcd = new ProjectCommitDialog())
            {
                pcd.Context = e.Context;
                pcd.LogMessageText = storedLogMessage;

                pcd.PreserveWindowPlacement = true;

                pcd.LoadItems(e.Selection.GetSelectedGitItems(true));

                DialogResult dr = pcd.ShowDialog(e.Context);

                storedLogMessage = pcd.LogMessageText;

                if (dr != DialogResult.OK)
                    return;

                PendingChangeCommitArgs pca = new PendingChangeCommitArgs();
                pca.StoreMessageOnError = true;
                // TODO: Commit it!
                List<PendingChange> toCommit = new List<PendingChange>(pcd.GetSelection());
                pcd.FillArgs(pca);

                e.GetService<IPendingChangeHandler>().Commit(toCommit, pca);
            }

            // not in the finally, because we want to preserve the message for a 
            // non-successful commit
            storedLogMessage = null;
        }
    }
}
