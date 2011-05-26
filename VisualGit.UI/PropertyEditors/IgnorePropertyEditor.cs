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
    /// Property editor for the predefined ignore property.
    /// </summary>
    partial class IgnorePropertyEditor : PropertyEditControl
    {
        public IgnorePropertyEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            CreateMyToolTip();
        }

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
                    string value = this.ignoreTextBox.Text.Trim();
                    return (!string.IsNullOrEmpty(value));
                }
            }
        }

        public override SvnPropertyValue PropertyItem
        {
            get
            {
                if ( !this.Valid )
                {
                    throw new InvalidOperationException(
                        "Can not get a property item when Valid is false");
                }
				
                return new SvnPropertyValue(SvnPropertyNames.SvnIgnore, ignoreTextBox.Text);
            }
            set
            {
                if (value != null)
                {
                    ignoreTextBox.Text = originalValue = value.StringValue;
                }
                else
                    ignoreTextBox.Text = originalValue = "";
            }
        }

        public override bool AllowNodeKind(SvnNodeKind kind)
        {
            return kind == SvnNodeKind.Directory;
        }

        public override string ToString()
        {
            return SvnPropertyNames.SvnIgnore;
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

        private void ignoreTextBox_TextChanged(object sender, System.EventArgs e)
        {
            string newValue = this.ignoreTextBox.Text;
            // Enables/Disables save button
            this.dirty = !newValue.Equals(this.originalValue);

            OnChanged(EventArgs.Empty);
        }

        private void CreateMyToolTip()
        {         
            // Set up the ToolTip text for the Button and Checkbox.
            conflictToolTip.SetToolTip( this.ignoreTextBox, 
                "Eks *.obj, subdir. Names of file-categories and directories to be ignored.");
        }

        /// <summary>
        /// Flag for enabling/disabling save button
        /// </summary>
        private bool dirty;

        private string originalValue = string.Empty;
    }
}

