using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitMoveArgs : GitClientArgs
    {
        public GitMoveArgs()
            : base(GitCommandType.Move)
        {
        }

        public bool Force { get; set; }
    }
}
