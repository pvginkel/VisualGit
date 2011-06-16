using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.GitLog;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.SccManagement
{
    public partial class CreateBranchDialog : VSContainerForm
    {
        public CreateBranchDialog()
        {
            InitializeComponent();

            versionBox.Revision = GitRevision.Head;
        }

        private GitOrigin _gitOrigin;
        public GitOrigin GitOrigin
        {
            get { return _gitOrigin; }
            set
            {
                _gitOrigin = value;

                versionBox.GitOrigin = _gitOrigin;
            }
        }

        protected override void OnContextChanged(EventArgs e)
        {
            base.OnContextChanged(e);

            versionBox.Context = Context;
        }

        public bool SwitchToBranch
        {
            get { return switchBox.Checked; }
            set { switchBox.Checked = value; }
        }

        public string BranchName
        {
            get
            {
                string branchName = branchBox.Text.Trim();

                if (String.Empty.Equals(branchName))
                    return null;
                else
                    return branchName;
            }
        }

        public bool Force
        {
            get { return forceBox.Checked; }
        }

        public GitRevision Revision
        {
            get { return versionBox.Revision; }
        }

        private void toUrlBox_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = BranchName != null;
        }
    }
}
