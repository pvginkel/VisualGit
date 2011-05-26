using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI.SccManagement
{
    public partial class UpdateAvailableDialog : VSDialogForm
    {
        public UpdateAvailableDialog()
        {
            InitializeComponent();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            Font boldFont = new Font(Font, FontStyle.Bold);

            headLabel.Font = boldFont;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Uri uri; // Just some minor precautions
            if (Uri.TryCreate((string)e.Link.LinkData, UriKind.Absolute, out uri) && !uri.IsFile && !uri.IsUnc)
            {
                try
                {
                    System.Diagnostics.Process.Start((string)e.Link.LinkData);
                }
                catch
                { }
            }
        }
    }
}
