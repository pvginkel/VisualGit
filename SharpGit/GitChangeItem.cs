using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitChangeItem
    {
        public GitNodeKind NodeKind { get; internal set; }
        public long CopyFromRevision { get; internal set; }
        public string Path { get; internal set; }
    }
}
