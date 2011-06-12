using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitMergeArgs : GitClientArgs
    {
        public GitMergeArgs()
            : base(GitCommandType.Merge)
        {
            FastForward = true;
            Strategy = GitMergeStrategy.DefaultForBranch;
        }

        public bool FastForward { get; set; }

        public GitMergeStrategy Strategy { get; set; }

        public bool SquashCommits { get; set; }

        public bool DoNotCommit { get; set; }
    }
}
