using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualGit.VS
{
    [CLSCompliant(false)]
    public interface IVisualGitVSColor
    {
        bool TryGetColor(__VSSYSCOLOREX vsColor, out Color color);
    }
}
