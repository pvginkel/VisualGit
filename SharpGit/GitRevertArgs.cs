using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitRevertArgs : GitClientArgs, IGitConflictsClientArgs
    {
        public GitRevertArgs()
            : base(GitCommandType.Revert)
        {
        }

        public bool CreateCommit { get; set; }

        public event EventHandler<GitConflictEventArgs> Conflict;

        protected internal override void OnConflict(GitConflictEventArgs e)
        {
            var ev = Conflict;

            if (ev != null)
                ev(this, e);
        }
    }
}
