using VisualGit.VS;
using VisualGit.UI;
using VisualGit.Scc.UI;
using System.Windows.Forms;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.SolutionApplyPatch)]
    [Command(VisualGitCommand.PendingChangesApplyPatch, HideWhenDisabled=false)]
    public class ApplyPatch : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();

            if (ss != null && !string.IsNullOrEmpty(ss.ProjectRoot) && ss.ProjectRootSvnItem.IsVersioned)
            {
                IVisualGitConfigurationService cs = e.GetService<IVisualGitConfigurationService>();

                if (!string.IsNullOrEmpty(cs.Instance.PatchExePath))
                    return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();

            VisualGitPatchArgs args = new VisualGitPatchArgs();
            args.ApplyTo = ss.ProjectRoot;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Patch files( *.patch)|*.patch|Diff files (*.diff)|*.diff|" +
                    "Text files (*.txt)|*.txt|All files (*.*)|*";

                if (ofd.ShowDialog(e.Context.DialogOwner) != DialogResult.OK)
                    return;

                args.PatchFile = ofd.FileName;
            }

            diff.RunPatch(args);
        }
    }
}
