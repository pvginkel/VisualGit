using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VisualGit.ExtensionPoints.IssueTracker;

namespace VisualGit.UI.PendingChanges
{
    partial class PendingIssuesPage : PendingChangesPage
    {
        public PendingIssuesPage()
        {
            InitializeComponent();
        }

        protected override Type PageType
        {
            get
            {
                return typeof(PendingIssuesPage);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IVisualGitIssueService issueService = Context.GetService<IVisualGitIssueService>();
            if (issueService != null)
            {
                RefreshPageContents();
                issueService.IssueRepositoryChanged += new EventHandler(issueService_IssueRepositoryChanged);
            }
        }

        void issueService_IssueRepositoryChanged(object sender, EventArgs e)
        {
            RefreshPageContents();
        }

        public void RefreshPageContents()
        {
            this.Controls.Clear();
            IVisualGitIssueService issueService = Context.GetService<IVisualGitIssueService>();
            if (issueService != null)
            {
                IssueRepository repository = issueService.CurrentIssueRepository;
                IWin32Window window = null;
                if (repository != null
                    && (window = repository.Window) != null)
                {
                    Control control = Control.FromHandle(window.Handle);
                    if (control != null)
                    {
                        control.Dock = DockStyle.Fill;
                        this.Controls.Add(control);
                        return;
                    }
                }
            }
            this.Controls.Add(pleaseConfigureLabel);
        }

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			pleaseConfigureLabel.Font = new Font(Font, FontStyle.Bold);
		}

		private void pleaseConfigureLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			IVisualGitIssueService issueService = Context.GetService<IVisualGitIssueService>();

			if (issueService != null)
				issueService.ShowConnectHelp();
		}
    }
}
