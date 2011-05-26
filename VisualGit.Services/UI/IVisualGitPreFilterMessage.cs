using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI
{
    public interface IVisualGitPreFilterMessage
    {
        bool PreFilterMessage(ref Message msg);
    }
}
