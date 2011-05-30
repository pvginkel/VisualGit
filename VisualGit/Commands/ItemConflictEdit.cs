using SharpSvn;
using VisualGit.Scc.UI;
using System.IO;
using VisualGit.UI;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemConflictEdit)]
    [Command(VisualGitCommand.DocumentConflictEdit)]
    [Command(VisualGitCommand.ItemConflictEditVisualStudio)]
    class ItemConflictEdit : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (e.Command == VisualGitCommand.DocumentConflictEdit)
            {
                GitItem item = e.Selection.ActiveDocumentItem;

                if (item != null && item.IsConflicted)
                    return;
            }
            else
                foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                {
                    if (item.IsConflicted && item.Status.State == SvnStatus.Conflicted)
                        return;
                }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            // TODO: Choose which conflict to edit if we have more than one!
            GitItem conflict = null;

            if (e.Command == VisualGitCommand.DocumentConflictEdit)
            {
                conflict = e.Selection.ActiveDocumentItem;

                if (conflict == null || !conflict.IsConflicted)
                    return;
            }
            else
                foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                {
                    if (item.IsConflicted)
                    {
                        conflict = item;
                        break;
                    }
                }

            if (conflict == null)
                return;

            conflict.MarkDirty();
            if (conflict.Status.State != SvnStatus.Conflicted)
            {
                VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

                mb.Show(string.Format(CommandStrings.TheConflictInXIsAlreadyResolved, conflict.FullPath), CommandStrings.EditConflictTitle, 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }            

            SvnInfoEventArgs conflictInfo = null;

            bool ok = false;
            ProgressRunnerResult r = e.GetService<IProgressRunner>().RunModal("Retrieving Conflict Information",
                delegate(object sender, ProgressWorkerArgs a)
                {
                    ok = a.Client.GetInfo(conflict.FullPath, out conflictInfo);
                });

            if (!ok || !r.Succeeded || conflictInfo == null)
                return;

            VisualGitMergeArgs da = new VisualGitMergeArgs();
            string dir = conflict.Directory;

            da.BaseFile = Path.Combine(dir, conflictInfo.ConflictOld ?? conflictInfo.ConflictNew);
            da.TheirsFile = Path.Combine(dir, conflictInfo.ConflictNew ?? conflictInfo.ConflictOld);

            if (!string.IsNullOrEmpty(conflictInfo.ConflictWork))
                da.MineFile = Path.Combine(dir, conflictInfo.ConflictWork);
            else
                da.MineFile = conflict.FullPath;

            da.MergedFile = conflict.FullPath;

            da.BaseTitle = "Base";
            da.TheirsTitle = "Theirs";
            da.MineTitle = "Mine";
            da.MergedTitle = conflict.Name;
            

            e.GetService<IVisualGitDiffHandler>().RunMerge(da);
        }
    }
}
