using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitMergeResult
    {
        Success,
        Aborted,
        Stopped,
        Failed,
        UpToDate,
        FastForward,
        Merged,
        Conflicting,
        NotSupported
    }
}
