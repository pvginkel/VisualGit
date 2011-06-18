using VisualGit.UI;
using System;
using System.Collections.Generic;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Shows differences compared to local text base.
    /// </summary>
    [Command(VisualGitCommand.DiffLocalItem)]
    [Command(VisualGitCommand.ItemCompareBase)]
    [Command(VisualGitCommand.ItemCompareCommitted)]
    [Command(VisualGitCommand.ItemCompareLatest)]
    [Command(VisualGitCommand.ItemComparePrevious)]
    [Command(VisualGitCommand.ItemCompareSpecific)]
    [Command(VisualGitCommand.ItemShowChanges)]
    [Command(VisualGitCommand.DocumentShowChanges)]
    public sealed class DiffLocalItem : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            if (e.Command == VisualGitCommand.DocumentShowChanges)
            {
                GitItem sel = e.Selection.ActiveDocumentItem;

                if (sel == null || sel.IsDirectory ||!sel.IsLocalDiffAvailable)
                    e.Enabled = false;

                return;
            }

            bool noConflictDiff = e.Command == VisualGitCommand.ItemShowChanges;

            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (item.IsDirectory)
                {
                    e.Enabled = false;
                    return;
                }
                if (item.IsVersioned && (item.Status.State != GitStatus.Added || item.Status.IsCopied))
                {
                    if (e.Command == VisualGitCommand.ItemCompareBase 
                        || e.Command == VisualGitCommand.ItemShowChanges
                        )
                    {
                        if (!item.IsLocalDiffAvailable)
                        {
                            // skip if local diff is not available
                            // single-select -> don't show 'Show Changes" option
                            // multi-select -> show the option, exclude these items during execution
                            continue;
                        }
                    }

                    if (noConflictDiff && item.IsConflicted)
                    {
                        // Use advanced diff to get a diff, or 'Edit Conflict' to resolve it
                        continue;
                    }

                    return;
                }
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitItem> selectedFiles = new List<GitItem>();
            bool selectionHasDeleted = false;

            if (e.Command == VisualGitCommand.DocumentShowChanges)
            {
                GitItem item = e.Selection.ActiveDocumentItem;

                if(item == null)
                    return;
                selectedFiles.Add(item);
            }
            else
                foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                {
                    if (!item.IsVersioned || (item.Status.State == GitStatus.Added && !item.Status.IsCopied))
                        continue;

                    if ( e.Command == VisualGitCommand.ItemCompareBase
                         || e.Command == VisualGitCommand.ItemShowChanges)
                    {
                        if (!(item.IsModified || item.IsDocumentDirty)
                            || !item.IsLocalDiffAvailable // exclude if local diff is not available
                            )
                            continue;
                    }

                    if (e.Command == VisualGitCommand.DiffLocalItem)
                    {
                        selectionHasDeleted |= !NotDeletedFilter(item);
                    }

                    selectedFiles.Add(item);
                }

            GitRevisionRange revRange = null;
            switch (e.Command)
            {
                case VisualGitCommand.DiffLocalItem:
                    break; // revRange null -> show selector
                case VisualGitCommand.ItemCompareBase:
                case VisualGitCommand.ItemShowChanges:
                case VisualGitCommand.DocumentShowChanges:
                    revRange = new GitRevisionRange(GitRevision.Base, GitRevision.Working);
                    break;
                case VisualGitCommand.ItemCompareCommitted:
                    revRange = new GitRevisionRange(GitRevision.Committed, GitRevision.Working);
                    break;
                case VisualGitCommand.ItemCompareLatest:
                    revRange = new GitRevisionRange(GitRevision.Head, GitRevision.Working);
                    break;
                case VisualGitCommand.ItemComparePrevious:
                    revRange = new GitRevisionRange(GitRevision.Previous, GitRevision.Working);
                    break;
            }

            if (e.PromptUser || selectedFiles.Count > 1 || revRange == null)
            {
                PathSelectorInfo info = new PathSelectorInfo("Select item for Diff", selectedFiles);
                info.SingleSelection = false;
                info.RevisionStart = revRange == null ? GitRevision.Base : revRange.StartRevision;
                info.RevisionEnd = revRange == null ? GitRevision.Working : revRange.EndRevision;
                if (selectionHasDeleted)
                {
                    // do not allow selecting deleted items if the revision combination includes GitRevision.Working
                    info.CheckableFilter += new PathSelectorInfo.SelectableFilter(delegate(GitItem item, GitRevision from, GitRevision to)
                    {
                        if (item != null
                            && (from == GitRevision.Working
                                || to == GitRevision.Working
                                )
                            )
                        {
                            return NotDeletedFilter(item);
                        }
                        return true;
                    });
                }
                info.EnableRecursive = false;
                info.Depth = GitDepth.Infinity;

                PathSelectorResult result;
                // should we show the path selector?
                if (e.PromptUser || !Shift)
                {
                    IUIShell ui = e.GetService<IUIShell>();

                    result = ui.ShowPathSelector(info);
                    if (!result.Succeeded)
                        return;
                }
                else
                    result = info.DefaultResult;

                selectedFiles.Clear();
                selectedFiles.AddRange(result.Selection);
                revRange = new GitRevisionRange(result.RevisionStart, result.RevisionEnd);
            }

            if (revRange.EndRevision.RevisionType == GitRevisionType.Working ||
                revRange.StartRevision.RevisionType == GitRevisionType.Working)
            {
                // Save only the files needed

                IVisualGitOpenDocumentTracker tracker = e.GetService<IVisualGitOpenDocumentTracker>();
                if (tracker != null)
                    tracker.SaveDocuments(GitItem.GetPaths(selectedFiles));
            }

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();
            foreach (GitItem item in selectedFiles)
            {
                VisualGitDiffArgs da = new VisualGitDiffArgs();

                if ((item.Status.IsCopied || item.IsReplaced) &&
                    (!revRange.StartRevision.RequiresWorkingCopy || !revRange.EndRevision.RequiresWorkingCopy))
                {
                    // The file is copied, use its origins history instead of that of the new file
                    GitUriTarget copiedFrom = diff.GetCopyOrigin(item);

                    // TODO: Maybe handle Previous/Committed as history

                    if (copiedFrom != null && !revRange.StartRevision.RequiresWorkingCopy)
                    {
                        if (null == (da.BaseFile = diff.GetTempFile(copiedFrom, revRange.StartRevision, true)))
                            return; // Canceled
                        da.BaseTitle = diff.GetTitle(copiedFrom, revRange.StartRevision);
                    }

                    if (copiedFrom != null && !revRange.EndRevision.RequiresWorkingCopy)
                    {
                        if (null == (da.MineFile = diff.GetTempFile(copiedFrom, revRange.EndRevision, true)))
                            return; // Canceled
                        da.MineTitle = diff.GetTitle(copiedFrom, revRange.EndRevision);
                    }
                }

                if (da.BaseFile == null)
                {
                    if (null == (da.BaseFile = (revRange.StartRevision == GitRevision.Working) ? item.FullPath :
                        diff.GetTempFile(item, revRange.StartRevision, true)))
                    {
                        return; // Canceled
                    }

                    da.BaseTitle = diff.GetTitle(item, revRange.StartRevision);
                }

                if (da.MineFile == null)
                {
                    if (null == (da.MineFile = (revRange.EndRevision == GitRevision.Working) ? item.FullPath :
                        diff.GetTempFile(item, revRange.EndRevision, true)))
                    {
                        return; // Canceled
                    }

                    da.MineTitle = diff.GetTitle(item, revRange.EndRevision);
                }

                if (!String.Equals(da.MineFile, item.FullPath, StringComparison.OrdinalIgnoreCase))
                    da.ReadOnly = true;

                diff.RunDiff(da);
            }
        }

        static bool NotDeletedFilter(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            return !item.IsDeleteScheduled && item.Exists;
        }
    }
}
