using System;
using System.Collections.Generic;
using VisualGit.Scc;
using System.IO;
using Microsoft.VisualStudio.Shell;
using SharpGit;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemResolveCasing)]
    class ItemResolveCasing : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsCasingConflicted)
                {
                    // Ok, something we can fix!
                    return;
                }
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitItem> toResolve = new List<GitItem>();

            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsCasingConflicted)
                {
                    toResolve.Add(item);
                }
            }
            try
            {
                foreach (GitItem item in toResolve)
                {
                    string svnPath = GetGitCasing(item);
                    string actualPath = GitTools.GetTruePath(item.FullPath);

                    if (svnPath == null || actualPath == null)
                        continue; // not found
                    if (!string.Equals(svnPath, actualPath, StringComparison.OrdinalIgnoreCase))
                        continue; // More than casing rename

                    string svnName = Path.GetFileName(svnPath);
                    string actualName = Path.GetFileName(actualPath);

                    if (svnName == actualName)
                        continue; // Can't fix directories!

                    IVisualGitOpenDocumentTracker odt = e.GetService<IVisualGitOpenDocumentTracker>();
                    using (odt.LockDocument(svnPath, DocumentLockType.NoReload))
                    using (odt.LockDocument(actualPath, DocumentLockType.NoReload))
                    {
                        try
                        {
                            // Try the actual rename
                            File.Move(actualPath, svnPath);
                        }
                        catch { }

                        try
                        {
                            // And try to fix the project+document system
                            VsShellUtilities.RenameDocument(e.Context, actualPath, svnPath);
                        }
                        catch
                        { }
                    }
                }
            }
            finally
            {
                e.GetService<IFileStatusMonitor>().ScheduleGitStatus(GitItem.GetPaths(toResolve));
            }
        }

        static string GetGitCasing(GitItem item)
        {
            throw new NotImplementedException();
#if false
            string name = null;
            // Find the correct casing
            using (SvnWorkingCopyClient wcc = new SvnWorkingCopyClient())
            {
                SvnWorkingCopyEntriesArgs ea = new SvnWorkingCopyEntriesArgs();
                ea.ThrowOnCancel = false;
                ea.ThrowOnError = false;

                wcc.ListEntries(item.Directory, ea,
                    delegate(object sender, SvnWorkingCopyEntryEventArgs e)
                    {
                        if (string.Equals(e.FullPath, item.FullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            name = e.FullPath;
                        }
                    });
            }

            return name;
#endif
        }

    }
}
