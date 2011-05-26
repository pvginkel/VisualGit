using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using SharpSvn;

namespace VisualGit.UI.PropertyEditors
{
    /// <summary>
    /// Property editor for executable properties.
    /// </summary>
    internal partial class NeedsLockPropertyEditor : PropertyEditControl
    {
        public NeedsLockPropertyEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            CreateMyToolTip();

            if (this.needsLockTextBox != null && !this.needsLockTextBox.IsDisposed)
            {
                this.needsLockTextBox.Text = FEEDBACK_TEXT;
            }
        }

        public override bool Valid
        {

            get { return true; }
        }

        public override SvnPropertyValue PropertyItem
        {
            get
            {
                if (!this.Valid)
                {
                    throw new InvalidOperationException(
                        "Can not get a property item when valid is false");
                }

                return new SvnPropertyValue(SvnPropertyNames.SvnNeedsLock, SvnPropertyNames.SvnBooleanValue);
            }

            set { }
        }

        public override bool AllowNodeKind(SvnNodeKind kind)
        {
            return kind == SvnNodeKind.File;
        }

        public override string ToString()
        {
            return SvnPropertyNames.SvnNeedsLock;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void CreateMyToolTip()
        {
            // Set up the ToolTip text for the Button and Textbox.
            needsLockToolTip.SetToolTip(this.needsLockTextBox, FEEDBACK_TEXT);
        }

        private static readonly string FEEDBACK_TEXT = "File needs lock.";
    }
}

