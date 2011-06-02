using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SharpGit
{
    [DebuggerDisplay("Range={StartRevision}-{EndRevision}")]
    public class GitRevisionRange
    {
        public GitRevisionRange(GitRevision start, GitRevision end)
        {
            StartRevision = start;
            EndRevision = end;
        }

        public GitRevision EndRevision { get; private set; }

        public GitRevision StartRevision { get; private set; }
    }
}
