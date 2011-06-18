using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitAccept
    {
        Postpone,
        Merged,
        MineFull,
        TheirsFull,
        Base,
        Mine,
        Theirs
    }
}
