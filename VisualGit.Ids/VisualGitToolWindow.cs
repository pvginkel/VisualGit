using System;

namespace VisualGit
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591 

    public enum VisualGitToolWindow
    {
        None=0,
        RepositoryExplorer,
        WorkingCopyExplorer,
        PendingChanges,
        Log,
        Diff,
        GitInfo
    }
}
