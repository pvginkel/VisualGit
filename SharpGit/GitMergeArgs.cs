using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitMergeArgs : GitClientArgs, IGitConflictsClientArgs
    {
        public GitMergeArgs()
            : base(GitCommandType.Merge)
        {
            FastForward = true;
            Strategy = GitMergeStrategy.DefaultForBranch;
        }

        public bool FastForward { get; set; }

        public GitMergeStrategy Strategy { get; set; }

        public bool SquashCommits { get; set; }

        public bool DoNotCommit { get; set; }

        public event EventHandler<GitConflictEventArgs> Conflict;

        protected internal override void OnConflict(GitConflictEventArgs e)
        {
            var ev = Conflict;

            if (ev != null)
                ev(this, e);
        }
    }
}
