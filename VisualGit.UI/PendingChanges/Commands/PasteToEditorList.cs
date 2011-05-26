using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Commands;
using VisualGit.Scc;
using System.Windows.Forms;
using VisualGit.UI.PendingChanges.Commits;

namespace VisualGit.UI.PendingChanges.Commands
{
    [Command(VisualGitCommand.PcLogEditorPasteFileList, HideWhenDisabled=false)]
    [Command(VisualGitCommand.PcLogEditorPasteRecentLog, HideWhenDisabled=false)]
    class PasteToEditorList : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            LogMessageEditor lme = e.Selection.GetActiveControl<LogMessageEditor>();

            if (lme == null || lme.ReadOnly || lme.PasteSource == null)
                e.Enabled = e.Visible = false;
            else if (e.Command == VisualGitCommand.PcLogEditorPasteFileList && !lme.PasteSource.HasPendingChanges)
                e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            LogMessageEditor lme = e.Selection.GetActiveControl<LogMessageEditor>();

            if (lme == null || lme.PasteSource == null)
                return;

            switch (e.Command)
            {
                case VisualGitCommand.PcLogEditorPasteFileList:
                    OnPasteList(e, lme);
                    break;
                case VisualGitCommand.PcLogEditorPasteRecentLog:
                    OnPasteRecent(e, lme);
                    break;
            }
        }

        void OnPasteList(CommandEventArgs e, LogMessageEditor lme)
        {
            StringBuilder sb = new StringBuilder();
            foreach (PendingChange pci in lme.PasteSource.PendingChanges)
            {
                sb.AppendFormat("* {0}", pci.RelativePath);
                sb.AppendLine();
            }

            lme.PasteText(sb.ToString());
        }

        void OnPasteRecent(CommandEventArgs e, LogMessageEditor lme)
        {
            using (RecentMessageDialog rmd = new RecentMessageDialog())
            {
                rmd.Context = e.Context;

                if (DialogResult.OK != rmd.ShowDialog(e.Context))
                    return;

                string text = rmd.SelectedText;
                if (!string.IsNullOrEmpty(text))
                    lme.PasteText(text);
            }
        }
    }
}
