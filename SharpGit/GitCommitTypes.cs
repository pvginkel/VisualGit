using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Flags]
    public enum GitCommitTypes
    {
        None = 0,
        Added = 1,
        Deleted = 2,
        ContentModified = 4,
        PropertiesModified = 8,
        Copied = 16,
        HasLockToken = 32,
    }
}
