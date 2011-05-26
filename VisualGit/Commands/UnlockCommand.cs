using System.Collections.Generic;
using SharpSvn;

using VisualGit.Scc;
using VisualGit.UI;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to unlock the selected items.
    /// </summary>
    [Command(VisualGitCommand.Unlock, HideWhenDisabled = true)]
    class UnlockCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            foreach (SvnItem item in e.Selection.GetSelectedSvnItems(true))
            {
                if (item.IsLocked)
                    return;

            }
            e.Enabled = false; // No need to unlock anything if we are not versioned or not locked
        }

        public override void OnExecute(CommandEventArgs e)
        {
            PathSelectorInfo psi = new PathSelectorInfo("Select Files to Unlock", e.Selection.GetSelectedSvnItems(true));

            psi.VisibleFilter += delegate(SvnItem item)
                                     {
                                         return item.IsLocked;
                                     };

            psi.CheckedFilter += delegate(SvnItem item)
                                     {
                                         return item.IsLocked;
                                     };

            PathSelectorResult psr;
            if (!Shift)
            {
                IUIShell uiShell = e.GetService<IUIShell>();

                psr = uiShell.ShowPathSelector(psi);
            }
            else
                psr = psi.DefaultResult;

            if (!psr.Succeeded)
                return;

            List<string> files = new List<string>();

            foreach (SvnItem item in psr.Selection)
            {
                files.Add(item.FullPath);
            }

            if (files.Count == 0)
                return;

            e.GetService<IProgressRunner>().RunModal(
                "Unlocking",
                delegate(object sender, ProgressWorkerArgs ee)
                    {
                        SvnUnlockArgs ua = new SvnUnlockArgs();

                        ee.Client.Unlock(files, ua);
                    });
        }
    }
}
