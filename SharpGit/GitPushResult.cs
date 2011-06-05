using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitPushResult : GitCommandResult
    {
        public string PostPushError { get; internal set; }
    }
}
