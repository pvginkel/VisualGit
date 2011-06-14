using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitResetArgs : GitClientArgs
    {
        public GitResetArgs()
            : base(GitCommandType.Reset)
        {
        }
    }
}
