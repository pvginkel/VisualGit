using System;
using System.Collections.Generic;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeSourceManuallyRecordPage : MergeSourceBasePage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeSourceManuallyRecordPage()
        {
            IsPageComplete = false;
            Text = MergeStrings.MergeSourceHeaderTitle;
            Description = MergeStrings.MergeSourceManuallyRecordPageHeaderMessage;
            InitializeComponent();
        }

        /// <see cref="VisualGit.UI.MergeWizard.MergeSourceBasePage" />
        internal override MergeWizard.MergeType MergeType
        {
            get { return MergeWizard.MergeType.ManuallyRecord; }
        }

        protected override void OnPageChanging(WizardPageChangingEventArgs e)
        {
            base.OnPageChanging(e);

            Wizard.LogMode = VisualGit.UI.SvnLog.LogMode.MergesEligible;
        }
    }
}
