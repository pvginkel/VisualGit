using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using SharpSvn;
using VisualGit.UI.RepositoryExplorer;
using VisualGit.UI.SccManagement;

namespace VisualGit.Commands.RepositoryExplorer
{
    [Command(VisualGitCommand.ReposCopyTo, AlwaysAvailable=true)]
    [Command(VisualGitCommand.ReposMoveTo, AlwaysAvailable=true)]
    class CopyToOrMove : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {

            bool foundOne = false;
            bool ok = true;
            Uri reposRoot = null;
            foreach (IGitRepositoryItem item in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                if (item.Origin == null || item.Origin.IsRepositoryRoot)
                {
                    ok = false;
                    break;
                }
                else if (e.Command == VisualGitCommand.ReposMoveTo && item.Origin.Target.Revision != SvnRevision.Head)
                {
                    ok = false;
                    break;
                }

                if(reposRoot == null)
                    reposRoot = item.Origin.RepositoryRoot;
                else if(item.Origin.RepositoryRoot != reposRoot)
                {
                    ok = false;
                    break;
                }

                foundOne = true;
            }

            if(!ok || !foundOne)
            {
                e.Enabled = false;
            }                
        }

        public override void OnExecute(CommandEventArgs e)
        {
            Uri target = null;
            Uri root = null;

            List<SvnUriTarget> copyFrom = new List<SvnUriTarget>();
            foreach (IGitRepositoryItem item in e.Selection.GetSelection<IGitRepositoryItem>())
            {
                SvnUriTarget utt = item.Origin.Target as SvnUriTarget;

                if(utt == null)
                    utt = new SvnUriTarget(item.Origin.Uri, item.Origin.Target.Revision);

                copyFrom.Add(utt);

                if(root == null)
                    root = item.Origin.RepositoryRoot;

                if (target == null)
                    target = item.Origin.Uri;
                else
                {
                    Uri itemUri = SvnTools.GetNormalizedUri(item.Origin.Uri);
                    
                    Uri r = item.Origin.Uri.MakeRelativeUri(target);

                    if(r.IsAbsoluteUri)
                    {
                        target = null;
                        break;
                    }

                    string rs = r.ToString();

                    if(r.ToString().StartsWith("/", StringComparison.Ordinal))
                    {
                        target = new Uri(target, "/");
                        break;
                    }

                    while(r.ToString().StartsWith("../"))
                    {
                        target = new Uri(target, "../");
                        r = item.Origin.Uri.MakeRelativeUri(target);
                    }
                }
            }

            bool isMove = e.Command == VisualGitCommand.ReposMoveTo;
            Uri toUri;
            string logMessage;
            using (CopyToDialog dlg = new CopyToDialog())
            {
                dlg.RootUri = root;
                dlg.SelectedUri = target;

                dlg.Text = isMove ? "Move to Url" : "Copy to Url";

                if (dlg.ShowDialog(e.Context) != System.Windows.Forms.DialogResult.OK)
                    return;

                toUri = dlg.SelectedUri;
                logMessage = dlg.LogMessage;
            }

            // TODO: BH: Make sure the 2 attempts actually make sense

            e.GetService<IProgressRunner>().RunModal(isMove ? "Moving" : "Copying",
                delegate(object snd, ProgressWorkerArgs a)
                {
                    if (isMove)
                    {
                        List<Uri> uris = new List<Uri>();
                        foreach (SvnUriTarget ut in copyFrom)
                            uris.Add(ut.Uri);

                        SvnMoveArgs ma = new SvnMoveArgs();
                        ma.LogMessage = logMessage;
                        ma.CreateParents = true;

                        try
                        {
                            // First try with the full new name
                            a.SvnClient.RemoteMove(uris, toUri, ma);
                        }
                        catch (SvnFileSystemException fs)
                        {
                            if (fs.SvnErrorCode != SvnErrorCode.SVN_ERR_FS_ALREADY_EXISTS)
                                throw;

                            // If exists retry below this directory with the existing name                            
                            ma.AlwaysMoveAsChild = true;
                            a.SvnClient.RemoteMove(uris, toUri, ma);
                        }
                    }
                    else
                    {
                        SvnCopyArgs ca = new SvnCopyArgs();
                        ca.LogMessage = logMessage;
                        ca.CreateParents = true;

                        try
                        {
                            // First try with the full new name
                            a.SvnClient.RemoteCopy(copyFrom, toUri, ca);
                        }
                        catch (SvnFileSystemException fs)
                        {
                            if (fs.SvnErrorCode != SvnErrorCode.SVN_ERR_FS_ALREADY_EXISTS)
                                throw;

                            // If exists retry below this directory with the existing name
                            ca.AlwaysCopyAsChild = true;
                            a.SvnClient.RemoteCopy(copyFrom, toUri, ca);
                        }
                    }
                });

            // TODO: Send some notification to the repository explorer on this change?
        }
    }
}
