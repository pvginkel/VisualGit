using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitPushArgs : GitTransportClientArgs
    {
        public GitPushArgs()
            : base(GitCommandType.Push)
        {
        }

        public GitRef LocalBranch { get; set; }

        public string Remote { get; set; }

        public string RemoteUri { get; set; }

        public GitRef Tag { get; set; }

        public bool Force { get; set; }

        public bool AllBranches { get; set; }

        public bool AllTags { get; set; }
    }
}
