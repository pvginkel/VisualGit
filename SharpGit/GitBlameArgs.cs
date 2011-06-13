using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitBlameArgs : GitClientArgs
    {
        public GitBlameArgs()
            : base(GitCommandType.Blame)
        {
        }

        public GitRevision End { get; set; }
        public GitIgnoreSpacing IgnoreSpacing { get; set; }
    }
}
