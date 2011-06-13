using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitCloneArgs : GitTransportClientArgs
    {
        public GitCloneArgs()
            : base(GitCommandType.Clone)
        {
        }
    }
}
