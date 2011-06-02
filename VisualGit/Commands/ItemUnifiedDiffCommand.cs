using System;
using System.Text;
using SharpSvn;
using VisualGit.UI;
using System.IO;
using VisualGit.VS;
using Microsoft.VisualStudio.Shell;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.UnifiedDiff)]
    class ItemUnifiedDiffCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (GitItem item in e.Selection.GetSelectedGitItems(true))
            {
                if (item.IsVersioned)
                    return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            PathSelectorResult result = ShowDialog(e);
            if (!result.Succeeded)
                return;

            SvnRevisionRange revRange = new SvnRevisionRange(result.RevisionStart, result.RevisionEnd);

            IVisualGitTempFileManager tempfiles = e.GetService<IVisualGitTempFileManager>();
            string tempFile = tempfiles.GetTempFile(".patch");

            IVisualGitSolutionSettings ss = e.GetService<IVisualGitSolutionSettings>();
            string slndir = ss.ProjectRoot;
            string slndirP = slndir + "\\";

            SvnDiffArgs args = new SvnDiffArgs();
            args.IgnoreAncestry = true;
            args.NoDeleted = false;
            args.Depth = result.Depth;

            using (MemoryStream stream = new MemoryStream())
            {
                e.Context.GetService<IProgressRunner>().RunModal("Diffing",
                    delegate(object sender, ProgressWorkerArgs ee)
                    {
                        foreach (GitItem item in result.Selection)
                        {
                            GitWorkingCopy wc;
                            if (!string.IsNullOrEmpty(slndir) &&
                                item.FullPath.StartsWith(slndirP, StringComparison.OrdinalIgnoreCase))
                                args.RelativeToPath = slndir;
                            else if ((wc = item.WorkingCopy) != null)
                                args.RelativeToPath = wc.FullPath;
                            else
                                args.RelativeToPath = null;

                            ee.SvnClient.Diff(item.FullPath, revRange, args, stream);
                        }

                        stream.Flush();
                        stream.Position = 0;
                    });
                using (StreamReader sr = new StreamReader(stream))
                {
                    File.WriteAllText(tempFile, sr.ReadToEnd(), Encoding.UTF8);
                    VsShellUtilities.OpenDocument(e.Context, tempFile);
                }
            }
        }

        static PathSelectorResult ShowDialog(CommandEventArgs e)
        {
            PathSelectorInfo info = new PathSelectorInfo("Select items for diffing", e.Selection.GetSelectedGitItems(true));
            IUIShell uiShell = e.GetService<IUIShell>();
            info.VisibleFilter += delegate { return true; };
            info.CheckedFilter += delegate(GitItem item) { return item.IsFile && (item.IsModified || item.IsDocumentDirty); };

            info.RevisionStart = SvnRevision.Base;
            info.RevisionEnd = SvnRevision.Working;

            // should we show the path selector?
            if (!Shift)
            {
                return uiShell.ShowPathSelector(info);
            }
            return info.DefaultResult;
        }
    }
}
