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
    public partial class RevertDialog : VSDialogForm
    {
        public RevertDialog()
        {
            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        public bool CreateCommit
        {
            get { return createCommitBox.Checked; }
        }

        public string RepositoryPath { get; set; }

        private void RevertDialog_Load(object sender, EventArgs e)
        {
            Debug.Assert(Revision != null && RepositoryPath != null);

            revisionBox.Text = Revision.Revision;

            using (var client = GetService<IGitClientPool>().GetNoUIClient())
            {
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

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
