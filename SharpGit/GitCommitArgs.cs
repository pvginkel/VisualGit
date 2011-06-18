using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitCommitArgs : GitClientArgsWithCommit
    {
        public GitCommitArgs()
            : base(GitCommandType.Commit)
        {
        }

        public GitDepth Depth { get; set; }
    }
}
