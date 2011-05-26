using System;
using System.Collections.Generic;
using SharpSvn;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeSourceManuallyRemovePage : MergeSourceBasePage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeSourceManuallyRemovePage()
        {
            IsPageComplete = false;
            Text = MergeStrings.MergeSourceHeaderTitle;
            Description = MergeStrings.MergeSourceManuallyRemovePageHeaderMessage;

            EnableSelectButton(false);
            InitializeComponent();
        }

        /// <see cref="VisualGit.UI.MergeWizard.MergeSourceBasePage" />
        internal override MergeWizard.MergeType MergeType
        {
            get { return MergeWizard.MergeType.ManuallyRemove; }
        }

        protected override void OnPageChanging(WizardPageChangingEventArgs e)
        {
            base.OnPageChanging(e);

            Wizard.LogMode = VisualGit.UI.SvnLog.LogMode.MergesMerged;
        }

        internal override ICollection<Uri> GetMergeSources(SvnItem target)
        {
            SvnMergeItemCollection items = Wizard.MergeUtils.GetAppliedMerges(target);

            List<Uri> rslt = new List<Uri>(items == null ? 0 : items.Count);

            if (items != null)
            {
                foreach (SvnMergeItem i in items)
                    rslt.Add(i.Uri);
            }

            return rslt;
        }
    }
}
