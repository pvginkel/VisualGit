using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit
{
    interface IVisualGitRuntimeInfo
    {
        bool IsInAutomation { get; }
    }
}
