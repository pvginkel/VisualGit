using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualGit.UI.Commands;
using System.Windows.Forms;
using SharpGit;
using VisualGit.VS;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.FileSccOpenFromGit)]
    class CloneRepository : CommandBase
    {
        public override void OnExecute(CommandEventArgs e)
        {
            using (CloneDialog dialog = new CloneDialog())
            {
                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                using (var client = e.GetService<IGitClientPool>().GetNoUIClient())
                {
                }

                GitCloneArgs args = new GitCloneArgs();

                ProgressRunnerArgs pa = new ProgressRunnerArgs();
                pa.CreateLog = true;
                pa.TransportClientArgs = args;

                string destination = Path.GetFullPath(dialog.Destination);
                GitException exception = null;

                e.GetService<IProgressRunner>().RunModal(CommandStrings.CloningRepository, pa,
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        try
                        {
                            a.Client.Clone(dialog.Remote, dialog.RemoteRef, destination, args);
                        }
                        catch (GitException ex)
                        {
                            exception = ex;
                        }
                    });

                if (exception != null)
                {
                    e.GetService<IVisualGitErrorHandler>().OnWarning(exception);
                }
                else
                {
                    IVsSolution2 sol = e.GetService<IVsSolution2>(typeof(SVsSolution));

                    sol.OpenSolutionViaDlg(destination, 1);
                }
            }
        }
    }
}
