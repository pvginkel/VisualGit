using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitListBranchResult : GitCommandResult
    {
        public GitListBranchResult()
        {
            Branches = new List<GitBranchRef>();
        }

        public IList<GitBranchRef> Branches { get; private set; }
    }
}
