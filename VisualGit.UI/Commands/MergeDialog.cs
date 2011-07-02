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

            UpdateEnabled();
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

                localBranchBox.BeginUpdate();
                localBranchBox.Items.Clear();
                trackingBranchBox.BeginUpdate();
                trackingBranchBox.Items.Clear();
                tagBox.BeginUpdate();
                tagBox.Items.Clear();

                // When a revision ref was provided, try to resolve it to a branch,
                // tag or remote branch.

                bool resolved = Revision == null;

                GitRef resolvedRef = null;

                foreach (var @ref in client.GetRefs(RepositoryPath))
                {
                    bool setCurrent = false;

                    if (
                        !resolved &&
                        String.Equals(Revision.Revision, @ref.Revision, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        resolvedRef = @ref;
                        resolved = true;
                        setCurrent = true;
                    }

                    switch (@ref.Type)
                    {
                        case GitRefType.Branch:
                            localBranchBox.Items.Add(@ref);
                            if (setCurrent)
                            {
                                localBranchRadioBox.Checked = true;
                                localBranchBox.SelectedIndex = localBranchBox.Items.Count - 1;
                            }
                            break;

                        case GitRefType.RemoteBranch:
                            trackingBranchBox.Items.Add(@ref);
                            if (setCurrent)
                            {
                                trackingBranchRadioBox.Checked = true;
                                trackingBranchBox.SelectedIndex = trackingBranchBox.Items.Count - 1;
                            }
                            break;

                        case GitRefType.Tag:
                            tagBox.Items.Add(@ref);
                            if (setCurrent)
                            {
                                tagRadioBox.Checked = true;
                                tagBox.SelectedIndex = tagBox.Items.Count - 1;
                            }
                            break;
                    }
                }

                if (resolvedRef != null)
                    Revision = resolvedRef;

                localBranchBox.EndUpdate();
                trackingBranchBox.EndUpdate();
                tagBox.EndUpdate();

                UpdateEnabled();
            }
        }

        public string RepositoryPath { get; set; }

        public GitRevision Revision { get; set; }

        public GitItem GitItem { get; set; }

        public GitMergeArgs Args { get; set; }

        private void okButton_Click(object sender, EventArgs e)
        {
            errorProvider.SetError(localBranchBox, null);
            errorProvider.SetError(trackingBranchBox, null);
            errorProvider.SetError(tagBox, null);

            if (MergeBranch == null)
            {
                Control selectedControl;

                if (localBranchRadioBox.Checked)
                    selectedControl = localBranchBox;
                else if (trackingBranchRadioBox.Checked)
                    selectedControl = trackingBranchBox;
                else // if (tagRadioBox.Checked)
                    selectedControl = tagBox;

                errorProvider.SetError(selectedControl, CommandStrings.SelectABranchTagOrRevision);
            }
            else
            {
                Args.FastForward = fastForwardRadioBox.Checked;
                Args.Strategy = ((MergeStrategy)mergeStrategyBox.SelectedItem).Strategy;
                Args.SquashCommits = squashCommitsBox.Checked;
                Args.DoNotCommit = doNotCommitBox.Checked;

                DialogResult = DialogResult.OK;
            }
        }

        public GitRef MergeBranch
        {
            get
            {
                if (localBranchRadioBox.Checked)
                    return localBranchBox.SelectedItem as GitRef;
                else if (trackingBranchRadioBox.Checked)
                    return trackingBranchBox.SelectedItem as GitRef;
                else if (tagRadioBox.Checked)
                    return tagBox.SelectedItem as GitRef;
                else
                    return null;
            }
        }

        private void localBranchRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            localBranchBox.Enabled = localBranchRadioBox.Checked;
            trackingBranchBox.Enabled = trackingBranchRadioBox.Checked;
            tagBox.Enabled = tagRadioBox.Checked;
        }

        private void trackingBranchRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void tagRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void revisionRadioBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }
    }
}
