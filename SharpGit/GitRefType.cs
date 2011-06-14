using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitRefType
    {
        Unknown,
        Branch,
        RemoteBranch,
        Tag,
        Head,
        RefSpec,
        Revision
    }
}
