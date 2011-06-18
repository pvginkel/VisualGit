using System;


using VisualGit.Scc;
using VisualGit.UI;
using SharpGit;

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
            switch (e.Command)
            {
                case VisualGitCommand.ItemResolveMineConflict:
                case VisualGitCommand.ItemResolveTheirsConflict:
                case VisualGitCommand.ItemResolveMergeTool:
                    e.Enabled = false;
                    return;
            }

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
                    throw new NotSupportedException();
                case VisualGitCommand.ItemResolveMineFull:
                    Resolve(e, GitAccept.MineFull);
                    break;
                case VisualGitCommand.ItemResolveTheirsFull:
                    Resolve(e, GitAccept.TheirsFull);
                    break;
                case VisualGitCommand.ItemResolveWorking:
                    Resolve(e, GitAccept.Merged);
                    break;
                case VisualGitCommand.ItemResolveBase:
                    Resolve(e, GitAccept.Base);
                    break;
                case VisualGitCommand.ItemResolveMineConflict:
                    throw new NotSupportedException();
                    //Resolve(e, GitAccept.Mine);
                    //break;
                case VisualGitCommand.ItemResolveTheirsConflict:
                    throw new NotSupportedException();
                    //Resolve(e, GitAccept.Theirs);
                    //break;
                default:
                    throw new NotSupportedException();
            }
        }

        static void Resolve(CommandEventArgs e, GitAccept accept)
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
            using (GitClient client = e.GetService<IGitClientPool>().GetNoUIClient())
            {
                GitResolveArgs a = new GitResolveArgs();
                a.Depth = GitDepth.Empty;

                foreach (string p in paths)
                {
                    client.Resolve(p, accept, a);
                }
            }
        }

        static void Resolved(CommandEventArgs e)
        {
            using (GitClient client = e.GetService<IGitClientPool>().GetNoUIClient())
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

