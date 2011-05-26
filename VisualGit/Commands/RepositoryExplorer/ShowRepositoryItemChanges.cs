using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using SharpSvn;

namespace VisualGit.Commands.RepositoryExplorer
{
    [Command(VisualGitCommand.RepositoryShowChanges, AlwaysAvailable = true)]
    [Command(VisualGitCommand.RepositoryCompareWithWc, AlwaysAvailable = true)]
    class ShowRepositoryItemChanges : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitRepositoryItem reposItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (reposItem != null && reposItem.Origin != null && reposItem.NodeKind != SharpSvn.SvnNodeKind.Directory
                && reposItem.Revision.RevisionType == SharpSvn.SvnRevisionType.Number)
            {
                if (e.Command == VisualGitCommand.RepositoryCompareWithWc)
                {
                    if (!(reposItem.Origin.Target is SvnPathTarget))
                    {
                        e.Enabled = false;
                        return;
                    }
                }

                return;
            }

            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();
            IGitRepositoryItem reposItem = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (reposItem == null)
                return;

            SvnRevision from;
            SvnRevision to;
            if (e.Command == VisualGitCommand.RepositoryCompareWithWc)
            {
                from = reposItem.Revision;
                to = SvnRevision.Working;
            }
            else
            {
                from = reposItem.Revision.Revision - 1;
                to = reposItem.Revision;
            }
            VisualGitDiffArgs da = new VisualGitDiffArgs();

            if (to == SvnRevision.Working)
            {
                da.BaseFile = diff.GetTempFile(reposItem.Origin.Target, from, true);

                if (da.BaseFile == null)
                    return; // User canceled

                da.MineFile = ((SvnPathTarget)reposItem.Origin.Target).FullPath;
            }
            else
            {
                string[] files = diff.GetTempFiles(reposItem.Origin.Target, from, to, true);

                if (files == null)
                    return; // User canceled
                da.BaseFile = files[0];
                da.MineFile = files[1];
                System.IO.File.SetAttributes(da.MineFile, System.IO.FileAttributes.ReadOnly | System.IO.FileAttributes.Normal);
            }

            da.BaseTitle = diff.GetTitle(reposItem.Origin.Target, from);
            da.MineTitle = diff.GetTitle(reposItem.Origin.Target, to);
            diff.RunDiff(da);
        }
    }
}
