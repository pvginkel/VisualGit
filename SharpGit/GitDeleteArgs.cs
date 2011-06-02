using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitDeleteArgs : GitClientArgs
    {
        public GitDeleteArgs()
            : base(GitCommandType.Delete)
        {
        }

        public bool KeepLocal { get; set; }
        public bool Force { get; set; }
    }
}
