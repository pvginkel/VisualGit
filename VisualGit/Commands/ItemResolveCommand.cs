using System;

using SharpSvn;

using VisualGit.Scc;
using VisualGit.UI;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemResolveMerge)]
    [Command(VisualGitCommand.ItemResolveMineFull)]
    [Command(VisualGitCommand.ItemResolveTheirsFull)]
    [Command(VisualGitCommand.ItemResolveMineConflict)]
    [Command(VisualGitCommand.ItemResolveTheirsConflict)]
    [Command(VisualGitCommand.ItemResolveBase)]
    [Command(VisualGitCommand.ItemResolveWorking)]
    [Command(VisualGitCommand.ItemResolveMergeTool)]
    class ItemResolveCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool foundOne = false;
            bool canDiff = true;
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (!item.IsConflicted)
                    continue;

                foundOne = true;

                if (item.IsTreeConflicted)
                    switch (e.Command)
                    {
                        case VisualGitCommand.ItemResolveMerge:
                        case VisualGitCommand.ItemResolveMergeTool:
                        case VisualGitCommand.ItemResolveMineFull:
                        case VisualGitCommand.ItemResolveTheirsFull:
                        case VisualGitCommand.ItemResolveMineConflict:
                        case VisualGitCommand.ItemResolveTheirsConflict:
                        case VisualGitCommand.ItemResolveBase:
                            e.Enabled = false; // Git can't handle these and neither can we.
                            return;
                        case VisualGitCommand.ItemResolveWorking:
                        default:
                            break;
                    }

                if (!item.IsTextFile)
                {
                    canDiff = false;
                }
            }

            if (!foundOne)
                e.Enabled = false;
            else if (!canDiff && (e.Command == VisualGitCommand.ItemResolveTheirsConflict || e.Command == VisualGitCommand.ItemResolveMineConflict))
                e.Enabled = false;
            else if (e.Command == VisualGitCommand.ItemResolveMergeTool)
                e.Enabled = false;
            else if (e.Command == VisualGitCommand.ItemResolveMergeTool && string.IsNullOrEmpty(e.GetService<IVisualGitConfigurationService>().Instance.MergeExePath))
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            switch (e.Command)
            {
                case VisualGitCommand.ItemResolveMerge:
                    Resolved(e);
                    break;
                case VisualGitCommand.ItemResolveMergeTool:
                    throw new NotImplementedException();
                case VisualGitCommand.ItemResolveMineFull:
                    Resolve(e, SvnAccept.MineFull);
                    break;
                case VisualGitCommand.ItemResolveTheirsFull:
                    Resolve(e, SvnAccept.TheirsFull);
                    break;
                case VisualGitCommand.ItemResolveWorking:
                    Resolve(e, SvnAccept.Merged);
                    break;
                case VisualGitCommand.ItemResolveBase:
                    Resolve(e, SvnAccept.Base);
                    break;
                case VisualGitCommand.ItemResolveMineConflict:
                    Resolve(e, SvnAccept.Mine);
                    break;
                case VisualGitCommand.ItemResolveTheirsConflict:
                    Resolve(e, SvnAccept.Theirs);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        static void Resolve(CommandEventArgs e, SvnAccept accept)
        {
            HybridCollection<string> paths = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (!item.IsConflicted)
                    continue;

                if (!paths.Contains(item.FullPath))
                    paths.Add(item.FullPath);
            }


            IVisualGitOpenDocumentTracker documentTracker = e.GetService<IVisualGitOpenDocumentTracker>();
            documentTracker.SaveDocuments(paths); // Make sure all files are saved before updating/merging!

            using (DocumentLock lck = documentTracker.LockDocuments(paths, DocumentLockType.NoReload))
            using (lck.MonitorChangesForReload())
            using (SvnClient client = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                SvnResolveArgs a = new SvnResolveArgs();
                a.Depth = SvnDepth.Empty;

                foreach (string p in paths)
                {
                    client.Resolve(p, accept, a);
                }
            }
        }

        static void Resolved(CommandEventArgs e)
        {
            using (SvnClient client = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
                {
                    if (!item.IsConflicted)
                        continue;

                    client.Resolved(item.FullPath);
                }
            }
        }
    }
}

