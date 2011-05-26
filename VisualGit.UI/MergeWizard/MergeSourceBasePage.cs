using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using VisualGit.Scc;
using VisualGit.UI.SvnLog;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{

    public partial class MergeSourceBasePage : BaseWizardPage
    {
        private readonly WizardMessage INVALID_FROM_URL = new WizardMessage(MergeStrings.InvalidFromUrl,
            WizardMessage.MessageType.Error);

        readonly BindingList<string> suggestedSources = new BindingList<string>();
        readonly BindingSource bindingSource;
        /// <summary>
        /// Constructor.
        /// </summary>
        protected MergeSourceBasePage()
        {
            InitializeComponent();

            bindingSource = new BindingSource(suggestedSources, "");
            mergeFromComboBox.DataSource = bindingSource;
        }

        /// <summary>
        /// Enables/Disables the Select button.
        /// </summary>
        internal void EnableSelectButton(bool enabled)
        {
            selectButton.Enabled = enabled;
            selectButton.Visible = enabled;
        }

        internal string MergeSourceText
        {
            get { return mergeFromComboBox.Text.Trim(); }
        }

        internal SvnOrigin MergeSource
        {
            get
            {
                SvnOrigin mergeSource = null;
                string mergeFrom = mergeFromComboBox.Text.Trim();
                if (!string.IsNullOrEmpty(mergeFrom))
                {
                    Uri mergeFromUri;
                    if (Uri.TryCreate(mergeFrom, UriKind.Absolute, out mergeFromUri))
                    {
                        try
                        {
                            mergeSource = new SvnOrigin(Context, mergeFromUri, null);
                        }
                        catch
                        {
                            mergeSource = null;
                        }
                    }
                }
                return mergeSource;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            mergeFromComboBox.Text = MergeTarget.Uri.ToString();

            MergeSources retrieveMergeSources = new MergeSources(RetrieveMergeSources);

            IAsyncResult mergeRetrieveResult = retrieveMergeSources.BeginInvoke(new AsyncCallback(MergesRetrieved), retrieveMergeSources);

            wcPath.Text = MergeTarget.FullPath;
            wcUri.Text = MergeTarget.Uri.ToString();
        }

        /// <summary>
        /// Sets the merge sources for the mergeFromComboBox.
        /// </summary>
        private void SetMergeSources(ICollection<Uri> mergeSources)
        {
            if (InvokeRequired)
            {
                SetMergeSourcesCallBack c = new SetMergeSourcesCallBack(SetMergeSources);
                BeginInvoke(c, new object[] { mergeSources });
                return;
            }
            bool containsAtLeastOne = false;

            foreach (Uri u in mergeSources)
            {
                containsAtLeastOne = true;
                suggestedSources.Add(u.ToString());
            }

            if (containsAtLeastOne)
            {
                UIUtils.ResizeDropDownForLongestEntry(mergeFromComboBox);
            }
            else if (MergeType == MergeWizard.MergeType.ManuallyRemove)
            {
                Message = new WizardMessage(MergeStrings.NoRevisionsToUnblock, WizardMessage.MessageType.Error);
                mergeFromComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            }

            ((Wizard)ParentForm).EnablePageAndButtons(true);
        }

        /// <summary>
        /// Retrieves the merge sources and adds them to the <code>ComboBox</code>.
        /// </summary>
        ICollection<Uri> RetrieveMergeSources()
        {
            return GetMergeSources(MergeTarget);
        }

        SvnItem MergeTarget
        {
            get { return Wizard.MergeTarget; }
        }

        void MergesRetrieved(IAsyncResult result)
        {
            MergeSources mergeSourcesCallback = result.AsyncState as MergeSources;
            ICollection<Uri> mergeSources = (mergeSourcesCallback != null)
                ? mergeSourcesCallback.EndInvoke(result)
                : new Uri[0];
            SetMergeSources(mergeSources);
        }

        delegate void SetMergeSourcesCallBack(ICollection<Uri> mergeSources);
        delegate ICollection<Uri> MergeSources();

        /// <summary>
        /// Displays the Repository Folder Dialog
        /// </summary>
        void selectButton_Click(object sender, EventArgs e)
        {
            Uri uri = UIUtils.DisplayBrowseDialogAndGetResult(
                this,
                MergeTarget,
                MergeTarget.Uri);

            if (uri != null)
                mergeFromComboBox.Text = uri.ToString();
        }

        private void wcHistoryBtn_Click(object sender, EventArgs e)
        {
            using (LogViewerDialog dialog = new LogViewerDialog(new SvnOrigin(MergeTarget)))
            {
                dialog.LogControl.StrictNodeHistory = true;

                dialog.ShowDialog(Context);
            }
        }
    }
}
