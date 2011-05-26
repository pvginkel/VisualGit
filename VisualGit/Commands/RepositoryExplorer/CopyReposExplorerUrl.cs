using System;
using VisualGit.UI;
using VisualGit.Scc;
using Clipboard = System.Windows.Forms.Clipboard;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to copy the URL of this item to the clipboard in Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.CopyReposExplorerUrl, AlwaysAvailable=true)]
    class CopyReposExplorerUrl : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            int n = 0;
            foreach (ISvnRepositoryItem i in e.Selection.GetSelection<ISvnRepositoryItem>())
            {
                n++;
                if (n > 1 || i.Origin == null)
                {
                    e.Enabled = false;
                    return;
                }
            }
            if (n == 1)
                return;

            foreach(SvnItem item in e.Selection.GetSelectedSvnItems(false))
            {
                n++;
                if(n > 1 || item.Uri == null)
                {
                    e.Enabled = false;
                    return;
                }
            }

            if (n != 1)
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            foreach (ISvnRepositoryItem i in e.Selection.GetSelection<ISvnRepositoryItem>())
            {
                if (i.Uri != null)
                    Clipboard.SetText(i.Uri.AbsoluteUri);

                return;
            }

            foreach (SvnItem item in e.Selection.GetSelectedSvnItems(false))
            {
                if (item.Uri != null)
                    Clipboard.SetText(item.Uri.AbsoluteUri);

                return;
            }
        }
    }
}
