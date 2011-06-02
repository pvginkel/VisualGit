using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{

    public sealed class GitCommittingEventArgs : CancelEventArgs
    {
        public GitCommandType CurrentCommandType { get; internal set; }
        public GitCommitItemCollection Items { get; internal set; }
        public string LogMessage { get; set; }
    }
}
