using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitCommitResult : GitCommandResult
    {
        public string PostCommitError { get; internal set; }
        public long Revision { get; internal set; }
    }
}
