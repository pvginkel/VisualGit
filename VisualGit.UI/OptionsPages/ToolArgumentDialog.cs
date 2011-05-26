using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using VisualGit.Scc.UI;

namespace VisualGit.UI.OptionsPages
{
    /// <summary>
    /// A dialog for use in a type editor for a string. Presents a dialog for editing the string.
    /// </summary>
    public partial class ToolArgumentDialog : VSDialogForm
    {
        public ToolArgumentDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The value entered in the dialog.
        /// </summary>
        public string Value
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }

        public void SetTemplates(IList<VisualGitDiffArgumentDefinition> templates)
        {
            macroView.Items.Clear();
            foreach (VisualGitDiffArgumentDefinition d in templates)
            {
                ListViewItem li = new ListViewItem(
                    new string[]
                    {
                        d.Key,
                        d.Description,
                        string.Join(", ", d.Aliases)
                    });

                macroView.Items.Add(li);
            }            
        }

        private void macroView_DoubleClick(object sender, EventArgs e)
        {
            ListViewHitTestInfo hti = macroView.HitTest(macroView.PointToClient(MousePosition));

            if (hti.Location != ListViewHitTestLocations.None && hti.Item != null)
            {
                PasteItem(hti.Item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (macroView.SelectedItems.Count == 1)
            {
                PasteItem(macroView.SelectedItems[0]);
            }
        }

        private void PasteItem(ListViewItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            textBox.SelectionLength = 0;
            textBox.SelectedText = string.Format("$({0})", item.Text);
            textBox.SelectionStart += textBox.SelectionLength;
            textBox.SelectionLength = 0;
        }        
    }
}
