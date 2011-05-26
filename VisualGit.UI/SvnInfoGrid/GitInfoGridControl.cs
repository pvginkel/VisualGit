﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI.SvnInfoGrid
{
	public partial class GitInfoGridControl : VisualGitToolWindowControl
	{
		public GitInfoGridControl()
		{
			InitializeComponent();
		}

        protected override void OnLoad(EventArgs e)
        {
            ToolStripRenderer renderer = null;
            System.Windows.Forms.Design.IUIService ds = Context.GetService<System.Windows.Forms.Design.IUIService>();
            if (ds != null)
            {
                renderer = ds.Styles["VsToolWindowRenderer"] as ToolStripRenderer;
            }

            if (renderer != null)
                grid.ToolStripRenderer = renderer;

            base.OnLoad(e);
        }
	}
}