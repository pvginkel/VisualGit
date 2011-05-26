using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.VS;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// A command that opens a file from the server in VS.NET
    /// </summary>
    [Command(VisualGitCommand.ViewInVsNet, AlwaysAvailable=true)]
    [Command(VisualGitCommand.ViewInWindows, AlwaysAvailable=true)]
    [Command(VisualGitCommand.ViewInVsText, AlwaysAvailable=true)]
    [Command(VisualGitCommand.ViewInWindowsWith, AlwaysAvailable=true)]
    class ViewInVSNetCommand : ViewRepositoryFileCommand
    {
        const int NOASSOCIATEDAPP = 1155;

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            base.OnUpdate(e);


            if (e.Enabled && e.Command == VisualGitCommand.ViewInVsNet)
            {
                ISvnRepositoryItem single = EnumTools.GetSingle(e.Selection.GetSelection<ISvnRepositoryItem>());
                IVisualGitSolutionSettings settings = e.GetService<IVisualGitSolutionSettings>();

                SvnOrigin origin = single.Origin; // Checked in parent

                string ext = Path.GetExtension(origin.Target.FileName);

                if (!string.IsNullOrEmpty(ext) && settings.OpenFileFilter.IndexOf("*" + ext, StringComparison.OrdinalIgnoreCase) < 0)
                    e.Enabled = false;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            ISvnRepositoryItem ri = null;

            foreach (ISvnRepositoryItem i in e.Selection.GetSelection<ISvnRepositoryItem>())
            {
                if (i.Origin == null)
                    continue;

                ri = i;
                break;
            }
            if (ri == null)
                return;

            string toFile = e.GetService<IVisualGitTempFileManager>().GetTempFileNamed(ri.Origin.Target.FileName);
            string ext = Path.GetExtension(toFile);

            if (!SaveFile(e, ri, toFile))
                return;

            if (e.Command == VisualGitCommand.ViewInVsNet)
                VsShellUtilities.OpenDocument(e.Context, toFile);
            else if (e.Command == VisualGitCommand.ViewInVsText)
            {
                IVsUIHierarchy hier;
                IVsWindowFrame frame;
                uint id;
                VsShellUtilities.OpenDocument(e.Context, toFile, VSConstants.LOGVIEWID_TextView, out hier, out id, out frame);
            }
            else
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;

                if (e.Command == VisualGitCommand.ViewInWindowsWith 
                    && !string.Equals(ext, ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: BH: I tested with adding quotes around {0} but got some error

                    // BH: Don't call this on .zip files in vista, as it will break the builtin
                    // zip file support in the Windows Explorer (as that isn't available in the list)

                    process.StartInfo.FileName = "rundll32.exe";
                    process.StartInfo.Arguments = string.Format("Shell32,OpenAs_RunDLL {0}", toFile);
                }
                else
                    process.StartInfo.FileName = toFile;

                try
                {
                    process.Start();
                }
                catch (Win32Exception ex)
                {
                    // no application is associated with the file type
                    if (ex.NativeErrorCode == NOASSOCIATEDAPP)
                        e.GetService<IVisualGitDialogOwner>()
                            .MessageBox.Show("Windows could not find an application associated with the file type",
                            "No associated application", MessageBoxButtons.OK);
                    else
                        throw;
                }
            }
        }        
    }
}
