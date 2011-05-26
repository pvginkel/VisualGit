using System.Windows.Forms;
using SharpSvn;

using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to export a Git repository or local folder.
    /// </summary>
    [Command(VisualGitCommand.Export, HideWhenDisabled = false)]
    class ExportCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            GitItem i = EnumTools.GetSingle(e.Selection.GetSelectedGitItems(false));

            if (i == null)
                e.Enabled = false;
        }
        public override void OnExecute(CommandEventArgs e)
        {
            using (ExportDialog dlg = new ExportDialog(e.Context))
            {
                dlg.OriginPath = EnumTools.GetSingle(e.Selection.GetSelectedGitItems(false)).FullPath;

                if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                SvnDepth depth = dlg.NonRecursive ? SvnDepth.Empty : SvnDepth.Infinity;

                e.GetService<IProgressRunner>().RunModal(CommandStrings.Exporting,
                    delegate(object sender, ProgressWorkerArgs wa)
                    {
                        SvnExportArgs args = new SvnExportArgs();
                        args.Depth = depth;
                        args.Revision = dlg.Revision;

                        wa.Client.Export(dlg.ExportSource, dlg.LocalPath, args);
                    });
            }
        }
    }
}
