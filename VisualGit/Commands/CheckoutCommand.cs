using System;
using System.Windows.Forms;
using VisualGit.VS;
using SharpSvn;

using VisualGit.Scc;
using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to checkout a Git repository.
    /// </summary>
    [Command(VisualGitCommand.Checkout, AlwaysAvailable=true)]
    class CheckoutCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitRepositoryItem single = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (single == null || single.NodeKind == SvnNodeKind.File || single.Origin == null)
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IGitRepositoryItem selected = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (selected == null)
                return;

            Uri uri = selected.Uri;
            string name = selected.Origin.Target.FileName;

            IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();

            using (CheckoutDialog dlg = new CheckoutDialog())
            {
                dlg.Context = e.Context;
                dlg.Uri = uri;
                dlg.LocalPath = System.IO.Path.Combine(ss.NewProjectLocation, name);
                
                if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                e.GetService<IProgressRunner>().RunModal("Checking Out", 
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        SvnCheckOutArgs args = new SvnCheckOutArgs();
                        args.Revision = dlg.Revision;
                        args.Depth = dlg.Recursive ? SvnDepth.Infinity : SvnDepth.Files;
                        args.IgnoreExternals = dlg.IgnoreExternals;

                        a.SvnClient.CheckOut(dlg.Uri, dlg.LocalPath, args);
                    });
            }
        }
    }
}
