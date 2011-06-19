using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitLogEventArgs : GitLoggingEventArgs
    {
        public int MergeLogNestingLevel { get; internal set; }
        public bool HasChildren { get; internal set; }
    }
}
