using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.Scc.UI;

namespace VisualGit.UI.SvnLog.Commands
{
    [Command(VisualGitCommand.LogCompareWithWorkingCopy, AlwaysAvailable = true)]
    public class CompareWithWorkingCopy : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            ISvnLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<ISvnLogItem>());

            if (item != null)
            {
                ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                if (logWindow != null)
                {
                    SvnOrigin origin = EnumTools.GetSingle(logWindow.Origins);

                    if (origin != null)
                    {
                        SvnPathTarget pt = origin.Target as SvnPathTarget;

                        if (pt != null)
                        {
                            SvnItem svnItem = e.GetService<IFileStatusCache>()[pt.FullPath];

                            if (svnItem != null && !svnItem.IsDirectory)
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
            SvnOrigin origin = EnumTools.GetSingle(logWindow.Origins);
            ISvnLogItem item = EnumTools.GetSingle(e.Selection.GetSelection<ISvnLogItem>());

            IVisualGitDiffHandler diff = e.GetService<IVisualGitDiffHandler>();

            VisualGitDiffArgs da = new VisualGitDiffArgs();
            da.BaseFile = diff.GetTempFile(origin.Target, item.Revision, true);
            if (da.BaseFile == null)
                return; // User cancel
            da.MineFile = ((SvnPathTarget)origin.Target).FullPath;
            da.BaseTitle = string.Format("Base (r{0})", item.Revision);
            da.MineTitle = "Working";

            diff.RunDiff(da);
        }
    }
}
