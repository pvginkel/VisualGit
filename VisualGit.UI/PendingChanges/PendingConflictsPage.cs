using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VisualGit.VS;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.UI.PendingChanges
{
    partial class PendingConflictsPage : PendingChangesPage
    {
        public PendingConflictsPage()
        {
            InitializeComponent();
        }

        protected override Type PageType
        {
            get
            {
                return typeof(PendingConflictsPage);
            }
        }

        bool _loaded;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            conflictView.Context = Context;

            if (!SystemInformation.HighContrast)
            {
                IVisualGitVSColor clr = Context.GetService<IVisualGitVSColor>();
                Color c;
                if (clr != null && clr.TryGetColor(__VSSYSCOLOREX.VSCOLOR_TITLEBAR_INACTIVE, out c))
                {
                    resolvePannel.BackColor = c;
                }

                if (clr != null && clr.TryGetColor(__VSSYSCOLOREX.VSCOLOR_TITLEBAR_INACTIVE_TEXT, out c))
                {
                    resolvePannel.ForeColor = c;
                }
            }

            conflictView.ColumnWidthChanged += new ColumnWidthChangedEventHandler(conflictView_ColumnWidthChanged);
            IDictionary<string, int> widths = ConfigurationService.GetColumnWidths(GetType());
            conflictView.SetColumnWidths(widths);

            ResizeToFit();
            _loaded = true;
        }

        protected void conflictView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            IDictionary<string, int> widths = conflictView.GetColumnWidths();
            ConfigurationService.SaveColumnsWidths(GetType(), widths);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_loaded)
                ResizeToFit();
        }

        private void ResizeToFit()
        {
            conflictEditSplitter.SplitterDistance += conflictEditSplitter.Panel2.Height - resolveLinkLabel.Bottom - resolveLinkLabel.Margin.Bottom;
        }
    }
}
