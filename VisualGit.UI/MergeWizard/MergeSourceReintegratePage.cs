using System;
using System.Collections.Generic;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeSourceReintegratePage : MergeSourceBasePage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeSourceReintegratePage()
        {
            IsPageComplete = false;
            Text = MergeStrings.MergeSourceHeaderTitle;
            Description = MergeStrings.MergeSourceReintegratePageHeaderMessage;
            InitializeComponent();
        }

        /// <see cref="VisualGit.UI.MergeWizard.MergeSourceBasePage" />
        internal override MergeWizard.MergeType MergeType
        {
            get { return MergeWizard.MergeType.Reintegrate; }
        }

        protected override void OnPageChanging(WizardPageChangingEventArgs e)
        {
            base.OnPageChanging(e);

            Wizard.LogMode = VisualGit.UI.SvnLog.LogMode.MergesEligible;
        }

        internal override ICollection<Uri> GetMergeSources(SvnItem target)
        {
            return new List<Uri>();
        }
    }
}
