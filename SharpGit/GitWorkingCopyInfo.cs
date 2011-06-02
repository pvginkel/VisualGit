using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitWorkingCopyInfo
    {
        public GitSchedule Schedule { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public string LastChangeAuthor { get; internal set; }
        public DateTime LastChangeTime { get; internal set; }
        public long LastChangeRevision { get; internal set; }
        public string ChangeList { get; internal set; }
        public long Revision { get; internal set; }
    }
}
