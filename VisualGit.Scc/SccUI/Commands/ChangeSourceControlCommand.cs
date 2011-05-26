using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

using VisualGit.UI;
using VisualGit.Commands;

namespace VisualGit.Scc.SccUI.Commands
{
    [Command(VisualGitCommand.FileSccChangeSourceControl)]
    sealed class ChangeSourceControlCommand : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            if (!e.State.SolutionExists || !e.State.SccProviderActive)
            {
                e.Enabled = false;
                return;
            }
        }

        public void OnExecute(CommandEventArgs e)
        {
            IVisualGitServiceProvider context = e.Context;
            if (!EnsureAllProjectsLoaded(context))
                return;
            // TODO: Verify if all project files and the solution are open and saved

            using (ChangeSourceControl csc = new ChangeSourceControl())
            {
                csc.Context = e.Context;

                csc.ShowDialog(e.Context);
            }
        }

        List<IVsHierarchy> GetProjects(IVisualGitServiceProvider context, __VSENUMPROJFLAGS flags)
        {
            IVsSolution solution = context.GetService<IVsSolution>(typeof(SVsSolution));
            Guid gNone = Guid.Empty;
            IEnumHierarchies hierEnum;
            ErrorHandler.ThrowOnFailure(solution.GetProjectEnum((uint)flags, ref gNone, out hierEnum));

            IVsHierarchy[] hiers = new IVsHierarchy[32];
            List<IVsHierarchy> result = new List<IVsHierarchy>();

            uint fetched;
            while(ErrorHandler.Succeeded(hierEnum.Next((uint)hiers.Length, hiers, out fetched)))
            {
                if((int)fetched == hiers.Length)
                    result.AddRange(hiers);
                else if(fetched > 0)
                    for(int i = 0; i < (int)fetched; i++)
                        result.Add(hiers[i]);
                else
                    break;
            }

            return result;
        }

        bool EnsureAllProjectsLoaded(IVisualGitServiceProvider context)
        {
            List<IVsHierarchy> unloaded = GetProjects(context, __VSENUMPROJFLAGS.EPF_UNLOADEDINSOLUTION);

            if(unloaded != null && unloaded.Count > 0)
            {
                VisualGitMessageBox mb = new VisualGitMessageBox(context);

                if(DialogResult.Yes != mb.Show("The solution contains unloaded projects. Any changes you make will not affect "+
                    "the unloaded projects.\n\nIt is strongly recommended that you reload all projects before continuing.\n\n" +
                    "Would you like to continue?", "Source Control", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Exclamation))
                    return false;
            }

            return true;
        }
    }
}
