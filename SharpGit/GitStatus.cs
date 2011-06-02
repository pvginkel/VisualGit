using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitStatus
    {
        Zero,
        None,
        NotVersioned,
        Normal,
        Added,
        Missing,
        Deleted,
        Replaced,
        Modified,
        Merged,
        Conflicted,
        Ignored,
        Obstructed,
        Incomplete,
    }
}
