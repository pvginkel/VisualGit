using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitCommitItem
    {
        public GitCommitTypes CommitType { get; internal set; }
        public long CopyFromRevision { get; internal set; }
        public Uri CopyFromUri { get; internal set; }
        public long Revision { get; internal set; }
        public Uri Uri { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public string FullPath { get; internal set; }
        public string Path { get; internal set; }
    }
}
