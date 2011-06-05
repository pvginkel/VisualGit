using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    public enum GitPullTagOption
    {
        Unset,
        AutoFollow,
        NoTags,
        FetchTags
    }
}
