using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using VisualGit.Scc;
using VisualGit.UI;
using VisualGit.UI.RepositoryExplorer.Dialogs;

namespace VisualGit.Commands.RepositoryExplorer
{
    [Command(VisualGitCommand.GitNodeDelete, AlwaysAvailable=true)]
    class DeleteRepositoryItemCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (IGitRepositoryItem i in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                if (i.Origin == null || i.Origin.Target.Revision != SvnRevision.Head || i.Origin.IsRepositoryRoot)
                    break;

                return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitOrigin> items = new List<GitOrigin>();
            List<IGitRepositoryItem> refresh = new List<IGitRepositoryItem>();
            foreach (IGitRepositoryItem i in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                if (i.Origin == null || i.Origin.Target.Revision != SvnRevision.Head || i.Origin.IsRepositoryRoot)
                    break;

                items.Add(i.Origin);
                refresh.Add(i);
            }

            if(items.Count == 0)
                return;

            string logMessage;
            Uri[] uris;
            using(ConfirmDeleteDialog d = new ConfirmDeleteDialog())
            {
                d.Context = e.Context;
                d.SetUris(items);

                if (!e.DontPrompt && d.ShowDialog(e.Context) != System.Windows.Forms.DialogResult.OK)
                    return;

                logMessage = d.LogMessage;
                uris = d.Uris;
            }

            try
            {
                e.GetService<IProgressRunner>().RunModal("Deleting",
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        SvnDeleteArgs da = new SvnDeleteArgs();
                        da.LogMessage = logMessage;

                        a.SvnClient.RemoteDelete(uris, da);
                    });
            }
            finally
            {
                // TODO: Don't refresh each item; refresh each parent!
                foreach(IGitRepositoryItem r in refresh)
                {
                    r.RefreshItem(true);
                }
            }
        }
    }
}
