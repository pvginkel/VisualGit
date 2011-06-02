using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitWriteArgs : GitClientArgs
    {
        public GitWriteArgs()
            : base(GitCommandType.Write)
        {
        }

        public GitRevision Revision { get; set; }
    }
}
