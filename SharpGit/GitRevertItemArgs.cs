using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitRevertItemArgs : GitClientArgs
    {
        public GitRevertItemArgs()
            : base(GitCommandType.RevertItem)
        {
        }

        public GitDepth Depth { get; set; }
    }
}
