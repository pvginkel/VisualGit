using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using SharpGit;

namespace VisualGit.UI.GitLog.Commands
{
    [Command(VisualGitCommand.LogCompareWithWorkingCopy, AlwaysAvailable = true)]
    public class CompareWithWorkingCopy : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            if (item != null)
            {
                ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                if (logWindow != null)
                {
                    GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);

                    if (origin != null)
                    {
                        GitPathTarget pt = origin.Target as GitPathTarget;

                        if (pt != null)
                        {
                            GitItem gitItem = e.GetService<IFileStatusCache>()[pt.FullPath];

                            if (gitItem != null && !gitItem.IsDirectory)
                            {
                                if (null == e.Selection.GetActiveControl<ILogControl>())
                                    e.Enabled = false;

                                return;
                            }
                        }
                    }
                }
            }

            e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            // All checked in OnUpdate            
            ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();
            GitOrigin origin = EnumTools.GetSingle(logWindow.Origins);
            IGitLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();

            VisualGitDiffArgs da = new VisualGitDiffArgs();

            da.BaseFile = diff.GetTempFile(origin.Target, item.Revision, true);
            if (da.BaseFile == null)
                return; // User cancel
            da.MineFile = ((GitPathTarget)origin.Target).FullPath;
            da.BaseTitle = string.Format("Base (r{0})", item.Revision);
            da.MineTitle = "Working";

            diff.RunDiff(da);
        }
    }
}
