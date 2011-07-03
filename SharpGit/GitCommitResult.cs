using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitCommitResult : GitCommandResult
    {
        public GitRevision Revision { get; internal set; }
    }
}
