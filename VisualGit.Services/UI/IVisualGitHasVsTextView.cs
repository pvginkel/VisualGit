using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualGit.UI
{
    [CLSCompliant(false)]
    public interface IVisualGitHasVsTextView
    {
        IVsTextView TextView { get; }
        IVsFindTarget FindTarget { get; }
    }
}
