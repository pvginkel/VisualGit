using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using SharpSvn;

using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    /// <summary>
    /// Abstract class allowing for extension to <code>WizardPage</code>,
    /// to be used by all merge source pages except for the "Two Different Trees"
    /// merge source page.
    /// </summary>
    public partial class MergeSourceBasePage
    {
        /// <see cref="WizardFramework.IWizardPage.IsPageComplete" />
        public override bool IsPageComplete
        {
            get
            {
                return true;
            }
        }

        protected override void OnPageChanging(WizardPageChangingEventArgs e)
        {
            base.OnPageChanging(e);

            // Set the MergeSource before the page changes
            Wizard.MergeSource = MergeSource;

            // Do not validate since this field isn't editable and its contents are
            // retrieved directly from mergeinfo.
            if (MergeType == MergeWizard.MergeType.ManuallyRemove)
                return;


            // Do not show an error while the resources are retrieved.
            //if (mergeFromComboBox.Text == Resources.LoadingMergeSources)
            //    return true;

            if (string.IsNullOrEmpty(MergeSourceText))
            {
                Message = MergeUtils.NO_FROM_URL;
                e.Cancel = true;
                return;
            }

            Uri tmpUri;
            if (!Uri.TryCreate(MergeSourceText, UriKind.Absolute, out tmpUri))
            {
                Message = MergeUtils.INVALID_FROM_URL;
                e.Cancel = true;
                return;
            }
            
            Message = null;
        }

        /// <summary>
        /// Returns the merge type for the subclass' page.
        /// </summary>
        internal virtual MergeWizard.MergeType MergeType { get { throw new NotSupportedException(); } }

        internal virtual ICollection<Uri> GetMergeSources(GitItem target)
        {
            SvnMergeSourcesCollection sources = Wizard.MergeUtils.GetSuggestedMergeSources(target);

            List<Uri> rslt = new List<Uri>(sources.Count);
            foreach (SvnMergeSource s in sources)
                rslt.Add(s.Uri);

            return rslt;
        }
    }
}
