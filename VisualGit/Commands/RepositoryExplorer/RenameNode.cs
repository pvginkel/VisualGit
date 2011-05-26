using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using SharpSvn;
using VisualGit.UI.RepositoryExplorer.Dialogs;
using System.Windows.Forms;

namespace VisualGit.Commands.RepositoryExplorer
{
    [Command(VisualGitCommand.RenameRepositoryItem, AlwaysAvailable=true)]
    class RenameNode : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            base.OnUpdate(e);

            IGitRepositoryItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (item != null && item.Origin != null)
            {
                if (item.Origin.Target.Revision == SvnRevision.Head
                    && !item.Origin.IsRepositoryRoot)
                {
                    return;
                }
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IGitRepositoryItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (item == null)
                return;

            string newName = item.Origin.Target.FileName;

            if (e.Argument != null)
            {
                string[] items = e.Argument as string[];

                if (items != null)
                {
                    if (items.Length == 1)
                        newName = items[0];
                    else if (items.Length > 1)
                        newName = items[1];
                }
            }

            string logMessage;
            using (RenameDialog dlg = new RenameDialog())
            {
                dlg.Context = e.Context;
                dlg.OldName = item.Origin.Target.FileName;
                dlg.NewName = newName;

                if (DialogResult.OK != dlg.ShowDialog(e.Context))
                {
                    return;
                }
                newName = dlg.NewName;
                logMessage = dlg.LogMessage;
            }

            try
            {
                Uri itemUri = SvnTools.GetNormalizedUri(item.Origin.Uri);
                e.GetService<IProgressRunner>().RunModal(CommandStrings.RenamingNodes,
                    delegate(object sender, ProgressWorkerArgs we)
                    {
                        SvnMoveArgs ma = new SvnMoveArgs();
                        ma.LogMessage = logMessage;
                        we.Client.RemoteMove(itemUri, new Uri(itemUri, newName), ma);
                    });
            }
            finally
            {
                item.RefreshItem(true);
            }
        }
    }
}
