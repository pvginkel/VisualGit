using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitTagArgs : GitClientArgs
    {
        public GitTagArgs()
            : base(GitCommandType.Tag)
        {
        }

        public string Message { get; set; }
        public GitRevision Revision { get; set; }
        public bool Force { get; set; }
    }
}
