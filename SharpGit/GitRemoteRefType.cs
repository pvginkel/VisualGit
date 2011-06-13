using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Flags]
    public enum GitRemoteRefType
    {
        None = 0,
        All = Branches | Tags,
        Branches = 1,
        Tags = 2
    }
}
