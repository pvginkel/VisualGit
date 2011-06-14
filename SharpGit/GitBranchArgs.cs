using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitBranchArgs : GitClientArgs
    {
        public GitBranchArgs()
            : base(GitCommandType.Branch)
        {
        }

        public bool Force { get; set; }
        public GitRevision Revision { get; set; }
    }
}
