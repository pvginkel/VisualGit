using System;
using System.Collections.Generic;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to cleanup the working copy.
    /// </summary>
    [Command(VisualGitCommand.Cleanup)]
    class Cleanup : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (item.IsVersioned)
                    return;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitItem> items = new List<GitItem>(e.Selection.GetSelectedGitItems(true));

            e.GetService<IProgressRunner>().RunModal("Running Cleanup",
                delegate(object sender, ProgressWorkerArgs a)
                {
                    HybridCollection<string> wcs = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (GitItem item in items)
                    {
                        if (!item.IsVersioned)
                            continue;

                        GitWorkingCopy wc = item.WorkingCopy;

                        if (wc != null && !wcs.Contains(wc.FullPath))
                            wcs.Add(wc.FullPath);
                    }

                    SvnCleanUpArgs args = new SvnCleanUpArgs();
                    args.ThrowOnError = false;
                    foreach (string path in wcs)
                        a.Client.CleanUp(path, args);
                });
        }
    }
}
