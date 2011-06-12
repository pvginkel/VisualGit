using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitInfoArgs : GitClientArgs
    {
        public GitInfoArgs()
            : base(GitCommandType.Info)
        {
        }

        public bool PrepareMerge { get; set; }
    }
}
