using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitResolveArgs : GitClientArgs
    {
        public GitResolveArgs()
            : base(GitCommandType.Resolved)
        {
            Depth = GitDepth.Empty;
        }

        public GitDepth Depth { get; set; }
        internal GitConflictEventArgs ConflictArgs { get; set; }
    }
}
