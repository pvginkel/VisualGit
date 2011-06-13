using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.Scc;
using SharpSvn;
using SharpGit;

namespace VisualGit.UI.Commands
{
    public partial class SwitchDialog : VSDialogForm
    {
        private GitRef _repositoryBranch;

        public SwitchDialog()
        {
            InitializeComponent();
        }

        public bool Force
        {
            get { return forceBox.Checked; }
            set { forceBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        public string LocalPath
        {
            get { return pathBox.Text; }
            set { pathBox.Text = value; }
        }

        GitOrigin _gitOrigin;
        public GitOrigin GitOrigin
        {
            get { return _gitOrigin; }
            set { _gitOrigin = value; versionBox.GitOrigin = _gitOrigin; }
        }

        /// <summary>
        /// Gets or sets the switch to branch.
        /// </summary>
        /// <value>The switch to branch.</value>
        public GitRef SwitchToBranch
        {
            get
            {
                if (localBranchRadioBox.Checked)
                    return localBranchBox.SelectedItem as GitRef;
                else if (trackingBranchRadioBox.Checked)
                    return trackingBranchBox.SelectedItem as GitRef;
                else if (tagRadioBox.Checked)
                    return tagBox.SelectedItem as GitRef;
                else if (versionBox.Revision != null)
                    return new GitRef(versionBox.Revision.ToString());
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    switch (value.Type)
                    {
                        case GitRefType.Branch:
                            localBranchBox.SelectedItem = value;
                            localBranchRadioBox.Checked = true;
                            break;

                        case GitRefType.RemoteBranch:
                            trackingBranchBox.SelectedItem = value;
                            trackingBranchRadioBox.Checked = true;
                            break;

                        case GitRefType.Tag:
                            tagBox.SelectedItem = value;
                            tagRadioBox.Checked = true;
                            break;

                        default:
                            versionBox.Revision = value;
                            revisionRadioBox.Checked = true;
                            break;
                    }
                }
            }
        }

        private void SwitchDialog_Shown(object sender, EventArgs e)
        {
            using (var client = GetService<IGitClientPool>().GetNoUIClient())
            {
                _repositoryBranch = client.GetCurrentBranch(LocalPath);

                localBranchBox.BeginUpdate();
                localBranchBox.Items.Clear();
                trackingBranchBox.BeginUpdate();
                trackingBranchBox.Items.Clear();
                tagBox.BeginUpdate();
                tagBox.Items.Clear();

                foreach (var @ref in client.GetRefs(LocalPath))
                {
                    switch (@ref.Type)
                    {
                        case GitRefType.Branch:
                            localBranchBox.Items.Add(@ref);
                            break;

                        case GitRefType.RemoteBranch:
                            trackingBranchBox.Items.Add(@ref);
                            break;

                        case GitRefType.Tag:
                            tagBox.Items.Add(@ref);
                            break;
                    }
                }

                localBranchBox.EndUpdate();
                trackingBranchBox.EndUpdate();
                tagBox.EndUpdate();
            }

            if (SwitchToBranch == null)
                SwitchToBranch = _repositoryBranch;
        }

        private void SwitchDialog_Validating(object sender, CancelEventArgs e)
        {
            errorProvider.SetError(localBranchBox, null);
            errorProvider.SetError(trackingBranchBox, null);
            errorProvider.SetError(tagBox, null);
            errorProvider.SetError(versionBox, null);

            if (SwitchToBranch == _repositoryBranch)
            {
                Control selectedControl;

                if (localBranchRadioBox.Checked)
                    selectedControl = localBranchBox;
                else if (trackingBranchRadioBox.Checked)
                    selectedControl = trackingBranchBox;
                else if (tagRadioBox.Checked)
                    selectedControl = tagBox;
                else
                    selectedControl = versionBox;

                errorProvider.SetError(selectedControl, CommandStrings.SelectABranchTagOrRevision);

                e.Cancel = true;
            }
        }

        protected override void OnContextChanged(EventArgs e)
        {
            base.OnContextChanged(e);

            versionBox.Context = Context;
        }
    }
}
