using System.Collections.Generic;
using SharpSvn;

using VisualGit.UI;
using VisualGit.Scc;
using System.Windows.Forms;
using SharpGit;
using System;

namespace VisualGit.Commands
{
    /// <summary>
    /// A command that updates an item.
    /// </summary>
    [Command(VisualGitCommand.UpdateItemSpecific)]
    [Command(VisualGitCommand.UpdateItemLatest)]
    [Command(VisualGitCommand.UpdateItemLatestRecursive)]
    class UpdateItem : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool hasDirectory = false;
            bool hasFile = false;
            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsDirectory)
                    hasDirectory = true;
                else if (item.IsFile)
                    hasFile = true;

                if (hasFile && hasDirectory)
                    break;
            }

            if (hasDirectory && !hasFile)
            {
                // User should use the recursive folder update
                e.Enabled = false;
                return;
            }

            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (item.IsVersioned)
                    return;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            GitRevision updateTo;
            GitDepth depth;
            List<string> files = new List<string>();
            if (e.Command == VisualGitCommand.UpdateItemSpecific)
            {
                IUIShell uiShell = e.GetService<IUIShell>();

                PathSelectorInfo info = new PathSelectorInfo("Select Items to Update",
                    e.Selection.GetSelectedGitItems(true));

                info.CheckedFilter += delegate(GitItem item) { return item.IsVersioned; };
                info.VisibleFilter += delegate(GitItem item) { return item.IsVersioned; };
                info.EnableRecursive = true;
                info.RevisionStart = GitRevision.Head;
                info.Depth = GitDepth.Infinity;

                PathSelectorResult result = !Shift ? uiShell.ShowPathSelector(info) : info.DefaultResult;

                if (!result.Succeeded)
                    return;

                updateTo = result.RevisionStart;
                depth = result.Depth;
                List<GitItem> dirs = new List<GitItem>();

                foreach (GitItem item in result.Selection)
                {
                    if (!item.IsVersioned)
                        continue;

                    if (item.IsDirectory)
                    {
                        if (result.Depth < GitDepth.Infinity)
                        {
                            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

                            DialogResult dr = mb.Show(CommandStrings.CantUpdateDirectoriesNonRecursive, "", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);

                            if (dr != DialogResult.Yes)
                                return;

                            depth = GitDepth.Infinity;
                        }
                    }

                    bool found = false;
                    foreach (GitItem dir in dirs)
                    {
                        if (item.IsBelowPath(dir))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        continue;

                    files.Add(item.FullPath);

                    if (item.IsDirectory)
                        dirs.Add(item);
                }


            }
            else
            {
                updateTo = GitRevision.Head;
                depth = GitDepth.Infinity;
                List<GitItem> dirs = new List<GitItem>();

                foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
                {
                    if (!item.IsVersioned)
                        continue;

                    bool found = false;
                    foreach (GitItem p in dirs)
                    {
                        if (item.IsBelowPath(p) && p.WorkingCopy == item.WorkingCopy)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        continue;

                    files.Add(item.FullPath);

                    if (item.IsDirectory)
                        dirs.Add(item);
                }
            }

            IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();
            tracker.SaveDocuments(e.Selection.GetSelectedFiles(true));
            using (DocumentLock lck = tracker.LockDocuments(files, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            {
                SvnUpdateResult ur;
                ProgressRunnerArgs pa = new ProgressRunnerArgs();
                pa.CreateLog = true;

                throw new NotImplementedException();
#if false
                e.GetService<IProgressRunner>().RunModal(CommandStrings.UpdatingTitle, pa,
                                                         delegate(object sender, ProgressWorkerArgs ee)
                                                         {
                                                             SvnUpdateArgs ua = new SvnUpdateArgs();
                                                             ua.Depth = depth;
                                                             ua.Revision = updateTo;
                                                             e.GetService<IConflictHandler>().
                                                                 RegisterConflictHandler(ua, ee.Synchronizer);
                                                             ee.SvnClient.Update(files, ua, out ur);
                                                         });
#endif
            }
        }
    }
}
