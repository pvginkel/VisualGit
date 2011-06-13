using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using VisualGit.Diff.DiffUtils;
using SharpSvn;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpGit;

namespace VisualGit.UI.MergeWizard
{
    public partial class MergeConflictHandlerDialog : VSDialogForm
    {
        GitConflictEventArgs input;
        GitAccept resolution = GitAccept.Postpone;
        bool applyToAll = false;
        bool applyToType = false;
        bool isBinary;

        public MergeConflictHandlerDialog()
        {
            InitializeComponent();
        }

        public MergeConflictHandlerDialog(GitConflictEventArgs args)
        {
            InitializeComponent();
            this.input = args;
            this.postponeRadioButton.Checked = true;
            this.Text = args.Path.Replace('/', '\\');
            if (this.input != null)
            {
                isBinary = this.input.IsBinary;
                if (isBinary)
                {
                    applyToTypedCheckBox.Text = "All &Binary conflicts";
                }
                else
                {
                    applyToTypedCheckBox.Text = "All &Text conflicts";
                }
                ShowDifferences(this.input.MyFile, this.input.TheirFile);
            }
        }

        /// <summary>
        /// Gets the conflict resolution preference
        /// </summary>
        public GitAccept ConflictResolution
        {
            get
            {
                return resolution;
            }
            internal set
            {
                this.resolution = value;
            }
        }

        /// <summary>
        /// Gets applyToAll option.
        /// If this option is selected, the user choice will be used to resolve all conflicts.
        /// </summary>
        public bool ApplyToAll
        {
            get
            {
                return applyToAll;
            }
        }

        /// <summary>
        /// Gets applyToType option. 
        /// If this option is selected, the user choice will be used to resolve all conflicts of the same type.
        /// </summary>
        public bool ApplyToType
        {
            get
            {
                return applyToType;
            }
        }

        private void postponeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ConflictResolution = GitAccept.Postpone;
        }

        private void mineRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ConflictResolution = GitAccept.MineFull;
        }

        private void theirsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ConflictResolution = GitAccept.TheirsFull;
        }

        private void baseRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.ConflictResolution = GitAccept.Base;
        }

        /// Sets the diff data
        private void ShowDifferences(string mine, string theirs)
        {
            Collection<string> A, B;
            GetFileLines(mine, theirs, out A, out B);
            TextDiff Diff = new TextDiff(HashType.HashCode, false, false);
            EditScript Script = Diff.Execute(A, B);

            string strCaptionA = "Mine";
            string strCaptionB = "Theirs";
            //VisualGit.Diff.FileName fnA = new VisualGit.Diff.FileName(mine);
            //VisualGit.Diff.FileName fnB = new VisualGit.Diff.FileName(theirs);
            diffControl.SetData(A, B, Script, strCaptionA, strCaptionB);
        }

        private void GetFileLines(string strA, string strB, out Collection<string> A, out Collection<string> B)
        {
            if (this.isBinary)
            {
                using (FileStream AF = File.OpenRead(strA))
                using (FileStream BF = File.OpenRead(strB))
                {
                    BinaryDiff BDiff = new BinaryDiff();
                    BDiff.FootprintLength = 8;
                    AddCopyList List = BDiff.Execute(AF, BF);

                    BinaryDiffLines Lines = new BinaryDiffLines(AF, List, 8);
                    A = Lines.BaseLines;
                    B = Lines.VerLines;
                }
            }
            else
            {
                A = Functions.GetFileTextLines(strA);
                B = Functions.GetFileTextLines(strB);
            }
        }

        private void applyToCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.applyToAll = this.applyToAllCheckBox.Checked;

            // applyToType is implied if applyToAll is checked.
            this.applyToTypedCheckBox.Checked = this.applyToAll ? true : this.applyToType;
            this.applyToTypedCheckBox.Enabled = !this.applyToAll;
        }

        private void applyToTypedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.applyToType = this.applyToTypedCheckBox.Checked;
        }
    }
}
