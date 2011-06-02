using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitDepth
    {
        Unknown = -2,
        Exclude = -1,
        Empty = 0,
        Files = 1,
        Children = 2,
        Infinity = 3,
    }
}
