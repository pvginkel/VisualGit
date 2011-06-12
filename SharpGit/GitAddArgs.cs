using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitAddArgs : GitClientArgs
    {
        public GitAddArgs()
            : base(GitCommandType.Add)
        {
        }

        public bool AddParents { get; set; }
        public bool Force { get; set; }
        public bool NoIgnore { get; set; }
        public GitDepth Depth { get; set; }
        internal bool Update { get; set; }
    }
}
