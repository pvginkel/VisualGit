using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitMergeStrategy
    {
        Unset,
        DefaultForBranch,
        Merge,
        Rebase
    }
}
