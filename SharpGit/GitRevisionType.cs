using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitRevisionType
    {
        None,
        Hash,
        Time,
        Committed,
        Previous,
        Base,
        Working,
        Head,
        Zero,
        One
    }
}
