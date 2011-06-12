using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitResolveArgs : GitClientArgs
    {
        public GitResolveArgs()
            : base(GitCommandType.Resolved)
        {
        }
    }
}
