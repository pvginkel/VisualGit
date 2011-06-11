using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitDiffArgs : GitClientArgs
    {
        public GitDiffArgs()
            : base(GitCommandType.Diff)
        {
            IgnoreAncestry = false;
            NoDeleted = true;
        }

        public bool IgnoreAncestry { get; set; }

        public bool NoDeleted { get; set; }

        public GitDepth Depth { get; set; }

        public string RelativeToPath { get; set; }
    }
}
