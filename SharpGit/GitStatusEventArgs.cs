using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public sealed class GitStatusEventArgs : CancelEventArgs
    {
        public Uri Uri { get; internal set; }
        public string FullPath { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public GitStatus LocalContentStatus { get; internal set; }
        public bool LocalCopied { get; internal set; }
        public GitWorkingCopyInfo WorkingCopyInfo { get; internal set; }
        public GitConflictData TreeConflict { get; internal set; }
        public GitInternalStatus InternalContentStatus { get; set; }
    }
}
