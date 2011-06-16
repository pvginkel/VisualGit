using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VisualGit.UI.Services;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.Commands;
using Microsoft.VisualStudio;
using VisualGit.VS;
using VisualGit.Scc.UI;
using VisualGit.Selection;

namespace VisualGit.UI.PendingChanges
{
    public partial class PendingChangesToolControl : VisualGitToolWindowControl, IVisualGitHasVsTextView
    {
        readonly List<PendingChangesPage> _pages;
        readonly PendingActivationPage _activatePage;
        readonly PendingCommitsPage _commitsPage;
        readonly PendingIssuesPage _issuesPage;
        PendingChangesPage _currentPage;
        PendingChangesPage _lastPage;

        public PendingChangesToolControl()
        {
            InitializeComponent();

            _activatePage = new PendingActivationPage();
            _commitsPage = new PendingCommitsPage();
            _issuesPage = new PendingIssuesPage();

            _pages = new List<PendingChangesPage>();
            _pages.Add(_activatePage);
            _pages.Add(_commitsPage);
            _pages.Add(_issuesPage);
        }

        protected override void OnLoad(EventArgs e)
        {
            ToolStripRenderer renderer = null;
            System.Windows.Forms.Design.IUIService ds = Context.GetService<System.Windows.Forms.Design.IUIService>();
            if (ds != null)
            {
                renderer = ds.Styles["VsToolWindowRenderer"] as ToolStripRenderer;
            }

            if (renderer != null)
                pendingChangesTabs.Renderer = renderer;

            foreach (PendingChangesPage p in _pages)
            {
                p.Context = Context;
                p.ToolControl = this;

                if (!panel1.Controls.Contains(p))
                {
                    p.Enabled = p.Visible = false;
                    p.Dock = DockStyle.Fill;
                    panel1.Controls.Add(p);
                }
            }

            base.OnLoad(e);

            UpdateColors(renderer != null);

            VisualGitServiceEvents ev = Context.GetService<VisualGitServiceEvents>();

            ev.SccProviderActivated += new EventHandler(OnSccProviderActivated);
            ev.SccProviderDeactivated += new EventHandler(OnSccProviderDeactivated);

            IVisualGitCommandStates states = Context.GetService<IVisualGitCommandStates>();

            bool shouldActivate = false;

            if (states != null)
            {
                if (!states.UIShellAvailable)
                {
                    ev.UIShellActivate += new EventHandler(OnSccShellActivate);
                    shouldActivate = false;
                }
                else
                    shouldActivate = states.SccProviderActive;
            }

            _lastPage = _commitsPage;

            ShowPanel(shouldActivate ? _lastPage : _activatePage, false);
            pendingChangesTabs.Enabled = shouldActivate;
        }

        void OnSccShellActivate(object sender, EventArgs e)
        {
            IVisualGitCommandStates states = Context.GetService<IVisualGitCommandStates>();

            if (states != null && states.SccProviderActive)
            {
                OnSccProviderActivated(sender, e);
            }

        }

        void OnSccProviderDeactivated(object sender, EventArgs e)
        {
            _activatePage.ShowMessage = true;
            ShowPanel(_activatePage, false);
            pendingChangesTabs.Enabled = false;
        }

        void OnSccProviderActivated(object sender, EventArgs e)
        {
            ShowPanel(_lastPage, false);
            pendingChangesTabs.Enabled = true;
        }

        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            ToolWindowHost.CommandContext = VisualGitId.PendingChangeContextGuid;
            //ToolWindowSite.KeyboardContext = VisualGitId.PendingChangeContextGuid;
            UpdateCaption();
        }

        private void UpdateColors(bool hasRenderer)
        {
            if (Context == null || SystemInformation.HighContrast)
                return;

            // We should use the VS colors instead of the ones provided by the OS
            IVisualGitVSColor colorSvc = Context.GetService<IVisualGitVSColor>();

            Color color;
            if (colorSvc.TryGetColor(__VSSYSCOLOREX.VSCOLOR_TOOLWINDOW_BACKGROUND, out color))
                BackColor = color;

            if (colorSvc.TryGetColor(__VSSYSCOLOREX.VSCOLOR_COMMANDBAR_GRADIENT_MIDDLE, out color))
            {
                pendingChangesTabs.BackColor = color;
                pendingChangesTabs.OverflowButton.BackColor = color;
            }

            if (!hasRenderer && colorSvc.TryGetColor(__VSSYSCOLOREX.VSCOLOR_COMMANDBAR_HOVEROVERSELECTED, out color))
            {
                pendingChangesTabs.ForeColor = color;
                pendingChangesTabs.OverflowButton.ForeColor = color;
            }
        }

        void ShowPanel(PendingChangesPage page, bool select)
        {
            if (page == null)
                throw new ArgumentNullException("page");
            else if (page == _currentPage)
                return;

            bool foundPage = false;
            foreach (PendingChangesPage p in panel1.Controls)
            {
                if (p != page)
                {
                    p.Enabled = p.Visible = false;
                }
                else
                {
                    foundPage = true;
                    p.Enabled = p.Visible = true;
                }
            }

            if (!foundPage)
            {
                panel1.Controls.Add(page);
                page.Dock = DockStyle.Fill;
            }

            _currentPage = page;

            if (page != _activatePage)
                _lastPage = page;

            fileChangesButton.Checked = (_lastPage == _commitsPage);
            issuesButton.Checked = (_lastPage == _issuesPage);

            if (select)
                page.Select();

            if (Context != null)
            {
                IVisualGitCommandService cmd = Context.GetService<IVisualGitCommandService>();

                if (cmd != null)
                    cmd.UpdateCommandUI(false);

                UpdateCaption();
            }
        }

        void UpdateCaption()
        {
            if (ToolWindowHost != null)
            {
                if (_currentPage == null || string.IsNullOrEmpty(_currentPage.Text))
                    ToolWindowHost.Title = ToolWindowHost.OriginalTitle;
                else
                    ToolWindowHost.Title = ToolWindowHost.OriginalTitle + " - " + _currentPage.Text;
            }
        }

        private void fileChangesButton_Click(object sender, EventArgs e)
        {
            ShowPanel(_commitsPage, true);
        }

        private void issuesButton_Click(object sender, EventArgs e)
        {
            ShowPanel(_issuesPage, true);
        }

        #region IVisualGitHasVsTextView Members
        Microsoft.VisualStudio.TextManager.Interop.IVsTextView IVisualGitHasVsTextView.TextView
        {
            get { return _commitsPage.TextView; }
        }

        Microsoft.VisualStudio.TextManager.Interop.IVsFindTarget IVisualGitHasVsTextView.FindTarget
        {
            get { return _commitsPage.FindTarget; }
        }

        #endregion
    }
}
