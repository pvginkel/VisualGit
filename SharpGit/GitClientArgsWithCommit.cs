using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public abstract class GitClientArgsWithCommit : GitClientArgs
    {
        protected GitClientArgsWithCommit(GitCommandType commandType)
            : base(commandType)
        {
        }

        public string LogMessage { get; set; }

        public event EventHandler<GitCommittingEventArgs> Committing;

        internal protected virtual void OnCommitting(GitCommittingEventArgs e)
        {
            var ev = Committing;

            if (ev != null)
                ev(this, e);
        }
    }
}
