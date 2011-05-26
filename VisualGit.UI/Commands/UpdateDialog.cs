using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.Scc;

namespace VisualGit.UI.Commands
{
    public partial class UpdateDialog : VSDialogForm
    {
        public UpdateDialog()
        {
            InitializeComponent();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FolderLabelText
        {
            get { return projectRootLabel.Text; }
            set { projectRootLabel.Text = value; }
        }

        GitItem _item;
        public GitItem ItemToUpdate
        {
            get { return _item; }
            set
            {
                _item = value;
                if (value != null)
                {
                    projectRootBox.Text = value.FullPath;
                    urlBox.Text = _item.Uri.ToString();
                    versionBox.GitOrigin = new VisualGit.Scc.GitOrigin(value);
                }
            }
        }

        public void SetMultiple(bool multiple)
        {
            projectRootLabel.Enabled = !multiple;
            if (multiple)
                projectRootBox.Text = "-";
        }

        public GitOrigin GitOrigin
        {
            get { return versionBox.GitOrigin; }
            set
            {
                versionBox.GitOrigin = value;
                if (value != null)
                    urlBox.Text = value.Uri.ToString();
            }
        }

        public SvnRevision Revision
        {
            get { return versionBox.Revision; }
            set { versionBox.Revision = value; }
        }

        public bool AllowUnversionedObstructions
        {
            get { return allowObstructions.Checked; }
            set { allowObstructions.Checked = value; }
        }

        public bool UpdateExternals
        {
            get { return !ignoreExternals.Checked; }
            set { ignoreExternals.Checked = !value; }
        }

        public bool SetDepthInfinty
        {
            get { return makeDepthInfinity.Checked; }
            set { makeDepthInfinity.Checked = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Context != null)
                versionBox.Context = Context;
        }
    }
}
