using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SharpSvn;
using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.VS;
using VisualGit.Scc.UI;


namespace VisualGit.UI.SvnLog.Commands
{
    [Command(VisualGitCommand.LogChangeLogMessage, AlwaysAvailable = true)]
    class ChangeLogMessage : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            int count = 0;
            foreach (IGitLogItem i in e.Selection.GetSelection<IGitLogItem>())
            {
                count++;

                if (count > 1)
                    break;
            }
            if (count != 1)
                e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            IVisualGitSolutionSettings slnSettings = e.GetService<IVisualGitSolutionSettings>();
            List<IGitLogItem> logItems = new List<IGitLogItem>(e.Selection.GetSelection<IGitLogItem>());
            if (logItems.Count != 1)
                return;

            using (EditLogMessageDialog dialog = new EditLogMessageDialog())
            {
                dialog.Context = e.Context;
                dialog.LogMessage = logItems[0].LogMessage;

                if (dialog.ShowDialog(e.Context) == DialogResult.OK)
                {
                    if (dialog.LogMessage == logItems[0].LogMessage)
                        return; // No changes

                    IVisualGitConfigurationService config = e.GetService<IVisualGitConfigurationService>();

                    if (config != null)
                    {
                        if (dialog.LogMessage != null && dialog.LogMessage.Trim().Length > 0)
                            config.GetRecentLogMessages().Add(dialog.LogMessage);
                    }

                    using (SvnClient client = e.GetService<IGitClientPool>().GetClient())
                    {
                        SvnSetRevisionPropertyArgs sa = new SvnSetRevisionPropertyArgs();
                        sa.AddExpectedError(SvnErrorCode.SVN_ERR_REPOS_DISABLED_FEATURE);
                        client.SetRevisionProperty(logItems[0].RepositoryRoot, logItems[0].Revision, SvnPropertyNames.SvnLog, dialog.LogMessage, sa);

                        if (sa.LastException != null &&
                            sa.LastException.SvnErrorCode == SvnErrorCode.SVN_ERR_REPOS_DISABLED_FEATURE)
                        {
                            VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);

                            mb.Show(sa.LastException.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            return;
                        }
                    }

                    ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                    if (logWindow != null)
                    {
                        // TODO: Somehow repair scroll position/number of items loaded
                        logWindow.Restart();
                    }
                }
            }
        }
    }
}
