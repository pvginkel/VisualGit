using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitNotifyEventArgs : EventArgs
    {
        // public SvnMergeRange MergeRange { get; }
        public string FullPath { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public GitNotifyState ContentState { get; internal set; }
        public GitNotifyAction Action { get; internal set; }
        public GitCommandType CommandType { get; internal set; }
        public GitException Error { get; internal set; }
        public long Revision { get; internal set; }
    }
}
