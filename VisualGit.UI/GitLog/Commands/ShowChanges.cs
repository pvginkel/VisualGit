using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using SharpSvn;
using VisualGit.VS;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogShowChanges, AlwaysAvailable = true)]
    class ShowChanges : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

            if (logWindow == null || logWindow.Origins == null)
            {
                e.Enabled = false;
                return;
            }

            if (UpdateForChangedFiles(e))
                return;

            UpdateForRevChanges(logWindow, e);
        }

        void UpdateForRevChanges(ILogControl logWindow, CommandUpdateEventArgs e)
        {
            GitOrigin first = EnumTools.GetSingle(logWindow.Origins);

            if (first == null)
            {
                e.Enabled = false;
                return;
            }

            SvnPathTarget pt = first.Target as SvnPathTarget;

            if (pt != null)
            {
                if (e.GetService<IFileStatusCache>()[pt.FullPath].IsDirectory)
                {
                    // We can't diff directories at this time
                    e.Enabled = false;
                    return;
                }
            }

            // Note: We can't have a local directory, but we can have a remote one.
        }

        bool UpdateForChangedFiles(CommandUpdateEventArgs e)
        {
            IGitLogChangedPathItem change = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogChangedPathItem>());

            if (change == null)
                return false;

            // Skip all the files we cannot diff
            switch (change.Action)
            {
                case SvnChangeAction.Add:
                    if (change.CopyFromRevision >= 0)
                        break; // We can retrieve this file using CopyFromPath
                    e.Enabled = false;
                    break;
                case SvnChangeAction.Delete:
                    e.Enabled = false;
                    break;
            }

            if (change.NodeKind == SvnNodeKind.Directory)
                e.Enabled = false;

            return true;
        }

        public void OnExecute(CommandEventArgs e)
        {
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

            if (PerformRevisionChanges(logWindow, e))
                return;

            PerformFileChanges(e);
        }

        bool PerformRevisionChanges(ILogControl log, CommandEventArgs e)
        {
            long min = long.MaxValue;
            long max = long.MinValue;

            int n = 0;

            HybridCollection<string> changedPaths = new HybridCollection<string>();
            foreach (IGitLogItem item in e.Selection.GetSelection<IGitLogItem>())
            {
                min = Math.Min(min, item.Revision);
                max = Math.Max(max, item.Revision);
                n++;
            }

            if (n > 0)
            {
                ExecuteDiff(e, log.Origins, new SvnRevisionRange(n == 1 ? min - 1 : min, max));
                return true;
            }

            return false;
        }

        void PerformFileChanges(CommandEventArgs e)
        {
            IGitLogChangedPathItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogChangedPathItem>());

            if (item != null)
            {
                switch (item.Action)
                {
                    case SvnChangeAction.Delete:
                        return;
                    case SvnChangeAction.Add:
                    case SvnChangeAction.Replace:
                        if (item.CopyFromRevision < 0)
                            return;
                        break;
                }

                ExecuteDiff(e, new GitOrigin[] { item.Origin }, new SvnRevisionRange(item.Revision - 1, item.Revision));
            }
        }

        void ExecuteDiff(CommandEventArgs e, ICollection<GitOrigin> targets, SvnRevisionRange range)
        {
            if (targets.Count != 1)
                return;

            SvnTarget diffTarget = EnumTools.GetSingle(targets).Target;

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();
            VisualGitDiffArgs da = new VisualGitDiffArgs();

            string[] files = diff.GetTempFiles(diffTarget, range.StartRevision, range.EndRevision, true);

            if (files == null)
                return;

            da.BaseFile = files[0];
            da.MineFile = files[1];
            da.BaseTitle = diff.GetTitle(diffTarget, range.StartRevision);
            da.MineTitle = diff.GetTitle(diffTarget, range.EndRevision);
            da.ReadOnly = true;
            diff.RunDiff(da);
        }
    }

    public static class LogHelper
    {
        public static IEnumerable<GitItem> IntersectWorkingCopyItemsWithChangedPaths(IEnumerable<GitItem> workingCopyItems, IEnumerable<string> changedPaths)
        {
            foreach (GitItem i in workingCopyItems)
            {
                foreach (string s in changedPaths)
                {
                    if (i.Uri.ToString().EndsWith(s))
                    {
                        yield return i;
                        break;
                    }
                }
            }
        }
    }
}
