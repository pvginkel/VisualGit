using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitExportArgs : GitClientArgs
    {
        public GitExportArgs()
            : base(GitCommandType.Export)
        {
        }

        public GitRevision Revision { get; set; }

        public GitDepth Depth { get; set; }
    }
}
