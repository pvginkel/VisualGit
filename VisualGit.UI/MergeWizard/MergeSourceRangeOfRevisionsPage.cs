using System;
using System.Windows.Forms;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{

    partial class MergeSourceRangeOfRevisionsPage : MergeSourceBasePage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MergeSourceRangeOfRevisionsPage()
        {
            Text = MergeStrings.MergeSourceHeaderTitle;
            Description = MergeStrings.MergeSourceRangeOfRevisionsPageHeaderMessage;
            Load += new EventHandler(Control_Load);
            InitializeComponent();
        }

        void Control_Load(object sender, EventArgs e)
        {
            NextPageRequired = true;
        }

        /// <summary>
        /// Gets/Sets whether or not the next page is required.
        /// </summary>
        public bool NextPageRequired
        {
            get { return _needsNextPage; }
            set
            {
                _needsNextPage = value;

                if (Wizard != null)
                    Wizard.UpdateButtons();
            }
        }

        /// <see cref="VisualGit.UI.MergeWizard.MergeSourceBasePage" />
        internal override MergeWizard.MergeType MergeType
        {
            get { return MergeWizard.MergeType.RangeOfRevisions; }
        }

        protected override void OnPageChanging(WizardPageChangingEventArgs e)
        {
            base.OnPageChanging(e);

            Wizard.LogMode = VisualGit.UI.SvnLog.LogMode.MergesEligible;
        }

        private bool _needsNextPage = false;

        #region UI Events
        /// <summary>
        /// Enable the "Next" button since the revision(s) must be selected on the next page.
        /// </summary>
        private void selectRevisionsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                NextPageRequired = true;
            }
        }

        /// <summary>
        /// Disable the "Next" button since the all applicable revisions will be merged.
        /// </summary>
        private void allRevisionsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                NextPageRequired = false;
            }
        }
        #endregion
    }
}
