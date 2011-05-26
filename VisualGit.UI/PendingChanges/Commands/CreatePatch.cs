using VisualGit.Commands;
using System.Windows.Forms;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.PendingChangesCreatePatch)]
    class CreatePatch : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if (page == null || !page.Visible)
                e.Enabled = false;
            else
                e.Enabled = page.CanCreatePatch();
        }

        public void OnExecute(CommandEventArgs e)
        {
            PendingCommitsPage page = e.Context.GetService<PendingCommitsPage>();

            if (page != null)
            {
                string fileName = GetFileName(e.Context.DialogOwner);

                if (!string.IsNullOrEmpty(fileName))
                {
                    page.DoCreatePatch(fileName);
                }
            }
        }

        private string GetFileName(IWin32Window dialogOwner)
        {
            string fileName = null;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Patch files( *.patch)|*.patch|Diff files (*.diff)|*.diff|" +
                    "Text files (*.txt)|*.txt|All files (*.*)|*";
                dlg.AddExtension = true;

                if (dlg.ShowDialog(dialogOwner) == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
            }
            return fileName;
        }
    }
}
