using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public sealed class GitLogArgs : GitClientArgs
    {
        public GitLogArgs()
            : base(GitCommandType.Log)
        {
        }

        public bool RetrieveMergedRevisions { get; set; }
        public bool StrictNodeHistory { get; set; }
        public int Limit { get; set; }
        public GitRevision End { get; set; }
        public GitRevision Start { get; set; }
        public GitRevision OperationalRevision { get; set; }
        public bool RetrieveChangedPaths { get; set; }

        internal void OnLog(GitLogEventArgs e)
        {
            var ev = Log;

            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<GitLogEventArgs> Log;
    }
}
