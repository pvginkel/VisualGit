using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SharpSvn;

using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeOptionsPage : BaseWizardPage
    {
        private Dictionary<SvnDepth, string> mergeDepths;

        public enum ConflictResolutionOption
        {
            PROMPT,
            MARK,
            MINE,
            THEIRS,
            BASE
        }

        public MergeOptionsPage()
        {
            IsPageComplete = true;

            Text = MergeStrings.MergeOptionsHeaderTitle;
            this.Description = MergeStrings.MergeOptionsHeaderMessage;
            InitializeComponent();
        }

        /// <summary>
        /// Gets/Sets how automatic conflicts
        /// for binary files should be handled.
        /// </summary>
        public ConflictResolutionOption BinaryConflictResolution
        {
            get { return _binaryConflictResolution; }
            set { _binaryConflictResolution = value; }
        }

        /// <summary>
        /// Gets/Sets how automatic conflicts
        /// for text files should be handled.
        /// </summary>
        public ConflictResolutionOption TextConflictResolution
        {
            get { return _textConflictResolution; }
            set { _textConflictResolution = value; }
        }

        /// <summary>
        /// Gets/Sets whether or not ancestry is ignored.
        /// </summary>
        public bool IgnoreAncestry
        {
            get { return _ignoreAncestry; }
            set { _ignoreAncestry = value; }
        }

        /// <summary>
        /// Gets/Sets whether or not unversioned obstructions
        /// are allowed.
        /// </summary>
        public bool AllowUnversionedObstructions
        {
            get { return _allowUnversionedObstructions; }
            set { _allowUnversionedObstructions = value; }
        }

        /// <summary>
        /// Gets/Sets the depth.
        /// </summary>
        public SvnDepth Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        private SvnDepth _depth = SvnDepth.Unknown;
        private bool _allowUnversionedObstructions = false;
        private bool _ignoreAncestry = false;
        private ConflictResolutionOption _binaryConflictResolution = ConflictResolutionOption.PROMPT;
        private ConflictResolutionOption _textConflictResolution = ConflictResolutionOption.PROMPT;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Wizard.PageChanged += new EventHandler(WizardDialog_PageChangeEvent);
        }

        #region UI Events
        private void MergeOptionsPage_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                // Moved from Constructor to _Load for timing reasons.
                mergeDepths = Wizard.MergeUtils.MergeDepths;

                // Decided against using BindingSource due to rendering time
                // and the requirement of threading to keep the UI from 
                // "freezing" when initially displayed.
                foreach (KeyValuePair<SvnDepth, string> kvp in mergeDepths)
                {
                    depthComboBox.Items.Add(kvp.Value);
                }

                UIUtils.ResizeDropDownForLongestEntry(depthComboBox);

                depthComboBox.SelectedIndex = 0;
            }
        }

        private void textConflictsPromptRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TextConflictResolution = MergeOptionsPage.ConflictResolutionOption.PROMPT;
        }

        private void textConflictsMarkRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TextConflictResolution = MergeOptionsPage.ConflictResolutionOption.MARK;
        }

        private void textConflictsMyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TextConflictResolution = MergeOptionsPage.ConflictResolutionOption.MINE;
        }

        private void textConflictsRepositoryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TextConflictResolution = MergeOptionsPage.ConflictResolutionOption.THEIRS;
        }

        private void textConflictsBaseRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TextConflictResolution = MergeOptionsPage.ConflictResolutionOption.BASE;
        }

        private void binaryConflictsPromptRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BinaryConflictResolution = MergeOptionsPage.ConflictResolutionOption.PROMPT;
        }

        private void binaryConflictsMarkRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BinaryConflictResolution = MergeOptionsPage.ConflictResolutionOption.MARK;
        }

        private void binaryConflictsMyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BinaryConflictResolution = MergeOptionsPage.ConflictResolutionOption.MINE;
        }

        private void binaryConflictsRepositoryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BinaryConflictResolution = MergeOptionsPage.ConflictResolutionOption.THEIRS;
        }

        private void binaryConflictsBaseRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BinaryConflictResolution = MergeOptionsPage.ConflictResolutionOption.BASE;
        }

        private void ignoreAncestryCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            IgnoreAncestry = ((CheckBox)sender).Checked;
        }

        private void unversionedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AllowUnversionedObstructions = ((CheckBox)sender).Checked;
        }

        private void depthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            SvnDepth depth = SvnDepth.Unknown;

            if (box.Text == MergeStrings.GitDepthInfinity)
                depth = SvnDepth.Infinity;
            else if (box.Text == MergeStrings.GitDepthChildren)
                depth = SvnDepth.Children;
            else if (box.Text == MergeStrings.GitDepthFiles)
                depth = SvnDepth.Files;
            else if (box.Text == MergeStrings.GitDepthEmpty)
                depth = SvnDepth.Empty;

            Depth = depth;
        }
        #endregion

        private void WizardDialog_PageChangeEvent(object sender, EventArgs e)
        {
            if (Wizard.CurrentPage == this)
            {
                PopulateUI();
            }
        }

        private void PopulateUI()
        {
            // clear the message, 
            Message = null;
        }
    }
}
