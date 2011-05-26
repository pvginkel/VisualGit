using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.UI
{
    public interface IContextControl
    {
        IVisualGitServiceProvider Context { get; }
    }
}
