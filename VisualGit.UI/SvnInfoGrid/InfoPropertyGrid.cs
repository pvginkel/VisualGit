using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI.SvnInfoGrid
{
    class InfoPropertyGrid : PropertyGrid
    {
        public new ToolStripRenderer ToolStripRenderer
        {
            get { return base.ToolStripRenderer; }
            set { base.ToolStripRenderer = value; }
        }
    }
}
