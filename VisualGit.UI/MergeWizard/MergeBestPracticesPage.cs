using System;
using System.Drawing;
using System.Windows.Forms;
using VisualGit.UI.WizardFramework;
using SharpSvn;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeBestPracticesPage : BaseWizardPage
    {
        public static readonly WizardMessage READY_FOR_MERGE = new WizardMessage(MergeStrings.ReadyForMerge,
            WizardMessage.MessageType.None);
        public static readonly WizardMessage NOT_READY_FOR_MERGE = new WizardMessage(MergeStrings.NotReadyForMerge,
            WizardMessage.MessageType.Error);

        /// <summary>
        /// Enumeration for the best practices checks performed by VisualGit.
        /// </summary>
        public enum BestPractices
        {
            NO_LOCAL_MODIFICATIONS,
            VALID_REVISION
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeBestPracticesPage()
        {
            IsPageComplete = true;

            Text = MergeStrings.MergeBestPracticesPageHeaderTitle;
            Description = MergeStrings.MergeBestPracticesPageHeaderMessage;
            InitializeComponent();
        }

        /// <summary>
        /// Returns whether or not the best practices page needs to be displayed.
        /// This value is different than <code>MergeTypePage.ShowBestPractices</code>
        /// in that this code actually validates the WC for best practices and if all
        /// best practices are adhered to, the page doesn't need to be displayed.
        /// </summary>
        public bool DisplayBestPracticesPage
        {
            get
            {
                MergeWizard wizard = (MergeWizard)Wizard;

                if (wizard.MergeUtils == null)
                    return false; // Work around issue where the WizardFramework calls this
                // before the MergeUtils is set when instantiating the WizardDialog.

                using (SvnWorkingCopyClient client = wizard.MergeUtils.GetWcClient())
                {
                    GitItem mergeTarget = wizard.MergeTarget;

                    SvnWorkingCopyVersion wcRevision;
                    client.GetVersion(mergeTarget.FullPath, out wcRevision);

                    bool hasLocalModifications = wcRevision.Modified;

                    Message = hasLocalModifications ? NOT_READY_FOR_MERGE : READY_FOR_MERGE;

                    // Update the images based on the return of the best practices checks
                    UpdateBestPracticeStatus(BestPractices.NO_LOCAL_MODIFICATIONS, !hasLocalModifications);

                    return hasLocalModifications;
                }
            }
        }

        public void UpdateBestPracticeStatus(MergeBestPracticesPage.BestPractices bestPractice,
            bool passed)
        {
            PictureBox pBox;

            switch (bestPractice)
            {
                case MergeBestPracticesPage.BestPractices.NO_LOCAL_MODIFICATIONS:
                    pBox = noUncommitedModificationsPictureBox;
                    break;
                default:
                    pBox = null;
                    break;
            }

            if (pBox != null)
            {
                if (passed)
                    pBox.Image = MergeStrings.SuccessImage;
                else
                    pBox.Image = MergeStrings.ErrorImage;
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            Font boldFont = new Font(Font, FontStyle.Bold);
            noUncommitedModificationsLabel.Font = boldFont;
        }
    }
}
