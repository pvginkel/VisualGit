using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.VS
{
    public interface IVisualGitSolutionExplorerWindow
    {
        void Show();

        void EnableVisualGitIcons(bool enabled);
    }
}
