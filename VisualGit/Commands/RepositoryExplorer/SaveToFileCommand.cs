using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using VisualGit.Scc;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to save currnet file to disk from Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.SaveToFile, AlwaysAvailable=true)]
    class SaveToFileCommand : ViewRepositoryFileCommand
    {
        public override void OnExecute(CommandEventArgs e)
        {
            IGitRepositoryItem ri = null;

            foreach (IGitRepositoryItem i in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                ri = i;
                break;
            }
            if (ri == null)
                return;

            string toFile;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                string name = ri.Origin.Target.FileName;

                sfd.Filter = "All Files (*.*)|*";
                string ext = Path.GetExtension(name).Trim('.');                

                if(!string.IsNullOrEmpty(ext))
                    sfd.Filter = string.Format("{0} Files|*.{0}|{1}", ext, sfd.Filter);

                sfd.FileName = name;

                if (sfd.ShowDialog(e.Context.DialogOwner) != DialogResult.OK)
                    return;

                toFile = sfd.FileName;
            }

            SaveFile(e, ri, toFile);
        }
    }
}
