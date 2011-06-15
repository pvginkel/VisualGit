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
            ChangeLists = new GitChangeListCollection();
        }

        public GitChangeListCollection ChangeLists { get; private set; }
        public GitDepth Depth { get; set; }
    }
}
