using System;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.Selection;
using VisualGit.Scc;
using VisualGit.UI.SccManagement;

namespace VisualGit.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to creates a new directory here in the Repository Explorer.
    /// </summary>
    [Command(VisualGitCommand.NewDirectory, AlwaysAvailable = true)]
    class CreateDirectoryCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitRepositoryItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (item == null
                || item.Origin == null
                || item.Origin.Target.Revision != SvnRevision.Head
                || item.NodeKind == SvnNodeKind.File)
            {
                e.Enabled = false;
            }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IGitRepositoryItem selected = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            string directoryName = "";

            using (CreateDirectoryDialog dlg = new CreateDirectoryDialog())
            {
                DialogResult result = dlg.ShowDialog(e.Context);

                directoryName = dlg.NewDirectoryName;

                if (result != DialogResult.OK || string.IsNullOrEmpty(directoryName))
                    return;

                string log = dlg.LogMessage;

                // Handle special characters like on local path
                Uri uri = SvnTools.AppendPathSuffix(selected.Uri, directoryName);

                ProgressRunnerResult prResult =
                    e.GetService<IProgressRunner>().RunModal(
                    CommandStrings.CreatingDirectories,
                    delegate(object sender, ProgressWorkerArgs ee)
                    {
                        SvnCreateDirectoryArgs args = new SvnCreateDirectoryArgs();
                        args.ThrowOnError = false;
                        args.CreateParents = true;
                        args.LogMessage = log;
                        ee.SvnClient.RemoteCreateDirectory(uri, args);
                    }
                    );

                if (prResult.Succeeded)
                {
                    selected.RefreshItem(false);
                }
            }
        }

    }
}



