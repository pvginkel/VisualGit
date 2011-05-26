using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Commands
{
    public interface IVisualGitCommandStates
    {
        bool UIShellAvailable { get; }

        bool CodeWindow { get; }

        bool Debugging { get; }

        bool DesignMode { get; }

        bool Dragging { get; }

        bool EmptySolution { get; }

        bool FullScreenMode { get; }

        bool NoSolution { get; }

        bool SolutionBuilding { get; }

        bool SolutionExists { get; }

        bool SolutionHasMultipleProjects { get; }

        bool SolutionHasSingleProject { get; }

        bool SccProviderActive { get; }

        bool OtherSccProviderActive { get; }

        bool GetRawOtherSccProviderActive();
    }
}
