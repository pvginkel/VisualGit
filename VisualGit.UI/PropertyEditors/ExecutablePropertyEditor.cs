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
    partial class ExecutablePropertyEditor : PropertyEditControl
    {
        public ExecutablePropertyEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            CreateMyToolTip();
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

                return new SvnPropertyValue(SvnPropertyNames.SvnExecutable, SvnPropertyNames.SvnBooleanValue);
            }
            set
            {                
            }
        }

        public override bool AllowNodeKind(SvnNodeKind kind)
        {
            return kind == SvnNodeKind.File;
        }

        public override string ToString()
        {
            return SvnPropertyNames.SvnExecutable;
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
            conflictToolTip.SetToolTip(this.executableTextBox, "File is executable");
        }
    }



}

