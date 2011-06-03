using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitChangeItem
    {
        public GitNodeKind NodeKind { get; internal set; }

        public string OldRevision { get; internal set; }

        public string OldPath { get; internal set; }

        public string Path { get; internal set; }

        public GitChangeAction Action { get; internal set; }

        public override string ToString()
        {
            if (OldPath == null)
                return Path + " (" + Action + ")";
            else
                return OldPath + " -> " + Path + " (" + Action + ")";
        }
    }
}
