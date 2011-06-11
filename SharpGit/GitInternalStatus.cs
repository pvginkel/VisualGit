using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Flags]
    public enum GitInternalStatus
    {
        Unset = 0,
        Added = 1,
        AssumeUnchanged = 2,
        Changed = 4,
        Modified = 8,
        Missing = 16,
        Removed = 32,
        Untracked = 64,
        Ignored = 128
    }
}
