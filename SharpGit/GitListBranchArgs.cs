using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitListBranchArgs : GitClientArgs
    {
        public GitListBranchArgs()
            : base(GitCommandType.ListBranch)
        {
        }

        public bool RetrieveRemoteOnly { get; set; }
    }
}
