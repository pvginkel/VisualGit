using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitStatusArgs : GitClientArgs
    {
        public GitStatusArgs()
            : base(GitCommandType.Status)
        {
        }

        public bool RetrieveAllEntries { get; set; }
        public bool RetrieveIgnoredEntries { get; set; }
        public GitDepth Depth { get; set; }
    }
}
