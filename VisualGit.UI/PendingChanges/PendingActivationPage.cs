using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VisualGit.Commands;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;

namespace VisualGit.UI.PendingChanges
{
    partial class PendingActivationPage : PendingChangesPage
    {
        public PendingActivationPage()
        {
            InitializeComponent();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            openSccSelectorLink.Font = new Font(Font, FontStyle.Bold);
        }

        private void openSccSelectorLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IVisualGitCommandService cs = Context.GetService<IVisualGitCommandService>();
            cs.PostExecCommand(new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ToolsOptions), "4393D325-DD54-4626-9527-5C1F6F333CDF");
        }

        protected override Type PageType
        {
            get
            {
                return typeof(PendingActivationPage);
            }
        }

        public bool ShowMessage
        {
            get { return openSccSelectorLink.Visible; }
            set { openSccSelectorLink.Visible = value; }
        }
    }
}
