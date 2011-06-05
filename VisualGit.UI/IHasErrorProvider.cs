using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisualGit.UI
{
    internal interface IHasErrorProvider
    {
        ErrorProvider ErrorProvider { get; }
    }
}
