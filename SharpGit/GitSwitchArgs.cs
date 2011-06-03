using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitSwitchArgs : GitClientArgs
    {
        public GitSwitchArgs()
            : base(GitCommandType.Switch)
        {
        }

        public bool Force { get; set; }

        public GitRevision StartPoint { get; set; }
    }
}
