using System.Windows.Forms;
using SharpSvn;

using VisualGit.UI.Commands;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to export a Subversion repository or local folder.
    /// </summary>
    [Command(VisualGitCommand.Export, HideWhenDisabled = false)]
    class ExportCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            SvnItem i = EnumTools.GetSingle(e.Selection.GetSelectedSvnItems(false));

            if (i == null)
                e.Enabled = false;
        }
        public override void OnExecute(CommandEventArgs e)
        {
            using (ExportDialog dlg = new ExportDialog(e.Context))
            {
                dlg.OriginPath = EnumTools.GetSingle(e.Selection.GetSelectedSvnItems(false)).FullPath;

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
