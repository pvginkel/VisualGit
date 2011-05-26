using System;
using System.Runtime.InteropServices;

namespace VisualGit
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591 

    [Guid(VisualGitId.CommandSet)]
    public enum VisualGitToolBar
    {
        None=0,
        ToolBarFirst = 0x7FFFFFF,
        PendingChanges,
        SourceControl,
        LogViewer,
        GitInfo
    }
}
