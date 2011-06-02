using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public sealed class GitCommittingEventArgs : CancelEventArgs
    {
        public GitCommittingEventArgs()
        {
            Items = new GitCommitItemCollection();
        }

        public GitCommandType CurrentCommandType { get; internal set; }
        public GitCommitItemCollection Items { get; private set; }
        public string LogMessage { get; set; }
    }
}
