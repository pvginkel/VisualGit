using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Design;

namespace SharpGit
{
    public class GitUIBindArgs
    {
        public IWin32Window ParentWindow { get; set; }
        public ISynchronizeInvoke Synchronizer { get; set; }
        public Image HeaderImage { get; set; }
        public IUIService UIService { get; set; }
        public Size AutoScaleBaseSize { get; set; }
    }
}
