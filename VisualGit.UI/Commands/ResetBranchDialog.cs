using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VisualGit.UI.Commands
{
    public partial class ResetBranchDialog : VSDialogForm
    {
        public ResetBranchDialog()
        {
            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        public string RepositoryPath { get; set; }

        private void ResetBranchDialog_Load(object sender, EventArgs e)
        {
            Debug.Assert(Revision != null && RepositoryPath != null);

            revisionBox.Text = Revision.Revision;

            using (var client = GetService<IGitClientPool>().GetNoUIClient())
            {
                branchBox.Text = client.GetCurrentBranch(RepositoryPath).ShortName;

                GitLogArgs args = new GitLogArgs();

                args.Start = Revision;
                args.Limit = 1;
                args.Log += (s, ea) =>
                    {
                        authorBox.Text = ea.Author;
                        commitDateBox.Text = ea.AuthorTime.ToString("g");
                        logMessageBox.Text = Regex.Replace(ea.LogMessage.TrimEnd(), "\\r?\\n", " \u00B6 ");
                    };

                client.Log(RepositoryPath, args);
            }
        }

        public GitResetType ResetType
        {
            get
            {
                if (softRadioBox.Checked)
                    return GitResetType.Soft;
                else if (mixedRadioBox.Checked)
                    return GitResetType.Mixed;
                else
                    return GitResetType.Hard;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (ResetType == GitResetType.Hard)
            {
                VisualGitMessageBox mb = new VisualGitMessageBox(Context);

                var result = mb.Show(CommandStrings.YouAreAboutToDiscardAllChanges, CommandStrings.ResetCurrentBranch,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
