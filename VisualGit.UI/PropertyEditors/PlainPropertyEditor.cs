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
    /// Property editor for plain properties
    /// </summary>
    internal partial class PlainPropertyEditor : PropertyEditControl
    {
        public PlainPropertyEditor()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            CreateMyToolTip();
        }

        public override bool Valid
        {
            // Every value is valid!
            get { return true; }
        }

        public override SvnPropertyValue PropertyItem
        {
            get
            {   
                return new SvnPropertyValue(PropertyName, this.valueTextBox.Text.Replace("\r", ""));
            }
            set
            {
                if (value != null)
                {
                    PropertyName = value.Key;
                    valueTextBox.Text = value.StringValue.Replace("\r", "").Replace("\n", Environment.NewLine);
                }
                else
                    valueTextBox.Text = "";
            }
        }

        public string CurrentText
        {
            get { return valueTextBox.Text ?? ""; }
            set { valueTextBox.Text = (value ?? "").Replace("\r", "").Replace("\n", Environment.NewLine); }
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

        private void valueTextBox_TextChanged(object sender, System.EventArgs e)
        {
            OnChanged(EventArgs.Empty);
        }


        private void CreateMyToolTip()
        {
            // Set up the ToolTip text for the Button and Checkbox.
            conflictToolTip.SetToolTip( this.valueTextBox, 
                "Enter value of your self defined property" );      
        }
    }
}

