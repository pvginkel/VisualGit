using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public partial class MergeDialog : VSDialogForm
    {
        public MergeDialog()
        {
            InitializeComponent();
        }

        private void MergeDialog_Load(object sender, EventArgs e)
        {
            strategyBox.Image = Properties.Resources.MergeGraph;

            foreach (var strategy in MergeStrategy.All)
            {
                mergeStrategyBox.Items.Add(strategy);
            }

            mergeStrategyBox.SelectedIndex = 0;

            using (var client = Context.GetService<IGitClientPool>().GetNoUIClient())
            {
                currentBranchBox.Text = client.GetCurrentBranch(RepositoryPath).ShortName;

                mergeBranchBox.BeginUpdate();
                mergeBranchBox.Items.Clear();

                bool resolvedProvidedRevision = Revision == null;

                foreach (var @ref in client.GetRefs(RepositoryPath))
                {
                    switch (@ref.Type)
                    {
                        case GitRefType.Branch:
                        case GitRefType.RemoteBranch:
                        case GitRefType.Tag:
                            mergeBranchBox.Items.Add(@ref);
                            break;
                    }

                    if (
                        !resolvedProvidedRevision &&
                        Revision != null &&
                        String.Equals(Revision.Revision, @ref.Revision, StringComparison.OrdinalIgnoreCase)
                    ) {
                        resolvedProvidedRevision = true;
                        mergeBranchBox.SelectedIndex = mergeBranchBox.Items.Count - 1;
                    }
                }

                mergeBranchBox.EndUpdate();
            }
        }

        public string RepositoryPath { get; set; }

        private void MergeDialog_Validating(object sender, CancelEventArgs e)
        {
            if (mergeBranchBox.SelectedItem == null)
            {
                errorProvider1.SetError(mergeBranchBox, CommandStrings.SelectABranch);
                e.Cancel = true;
            }
            else
                errorProvider1.SetError(mergeBranchBox, null);
        }

        public GitRevision Revision { get; set; }

        public GitItem GitItem { get; set; }

        public GitMergeArgs Args { get; set; }

        private void okButton_Click(object sender, EventArgs e)
        {
            Args.FastForward = fastForwardRadioBox.Checked;
            Args.Strategy = ((MergeStrategy)mergeStrategyBox.SelectedItem).Strategy;
            Args.SquashCommits = squashCommitsBox.Checked;
            Args.DoNotCommit = doNotCommitBox.Checked;

            DialogResult = DialogResult.OK;
        }

        public GitRef MergeBranch
        {
            get { return (GitRef)mergeBranchBox.SelectedItem; }
        }
    }
}
