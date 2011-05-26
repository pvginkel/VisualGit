using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SharpSvn;

namespace VisualGit.UI.PropertyEditors
{
    /// <summary>
    /// Editor for the mime-type properties.
    /// </summary>
    partial class MimeTypePropertyEditor : PropertyEditControl
    {
        public MimeTypePropertyEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            CreateMyToolTip();
        }

        /// <summary>
        /// Indicates whether the property is valid.
        /// </summary>
        public override bool Valid
        {
            get
            {
                if (!this.dirty)
                {
                    return false;
                }
                else 
                {
                    return validateMimeType.IsMatch(this.mimeTextBox.Text);
                }
            } 
        }

        /// <summary>
        /// Sets and gets the property item.
        /// </summary>
        public override SvnPropertyValue PropertyItem
        {
            get
            {
                if ( !this.Valid )
                {
                    throw new InvalidOperationException(
                        "Can not get a property item when Valid is false");
                }	
                return new SvnPropertyValue(SvnPropertyNames.SvnMimeType, mimeTextBox.Text);
            }

            set
            {
                if (value != null)
                {
                    mimeTextBox.Text = originalValue = value.StringValue;
                }
                else
                    mimeTextBox.Text = originalValue = "";
            }
        }

        public override bool AllowNodeKind(SvnNodeKind kind)
        {
            return kind == SvnNodeKind.File;
        }

        /// <summary>
        /// Indicates the type of property.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SvnPropertyNames.SvnMimeType;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }
        /// <summary>
        /// Dispatches the Changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mimeTextBox_TextChanged(object sender, System.EventArgs e)
        {
            // Enables save button
            string newValue = this.mimeTextBox.Text;
            this.dirty = !newValue.Equals(this.originalValue);

            OnChanged(EventArgs.Empty);
        }

        private void CreateMyToolTip()
        {
            // Set up the ToolTip text for the Button and Checkbox.
            conflictToolTip.SetToolTip( this.mimeTextBox, 
                "Defult is text/*, everything else is binary");
        }

        
        private static readonly Regex validateMimeType = 
            new Regex(@"\w{2,}/\*{1}|(\w{2,})", RegexOptions.Compiled);
        /// <summary>
        /// Flag for enabled/disabled save button
        /// </summary>
        private bool dirty;

        private string originalValue = string.Empty;
    }
}

