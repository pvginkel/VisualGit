using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public abstract class GitClientArgs
    {
        internal GitClientArgs(GitCommandType commandType)
        {
            ThrowOnError = true;
            ThrowOnCancel = true;
            CommandType = commandType;
        }

        public bool ThrowOnError { get; set; }
        public bool ThrowOnCancel { get; set; }

        public Exception LastException { get; internal set; }

        public GitCommandType CommandType { get; private set; }

        internal protected virtual void OnNotify(GitNotifyEventArgs e)
        {
            var ev = Notify;

            if (ev != null)
                Notify(this, e);
        }

        internal protected virtual void OnCancel(CancelEventArgs e)
        {
            var ev = Cancel;

            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<GitNotifyEventArgs> Notify;

        public event EventHandler<CancelEventArgs> Cancel;

        protected internal virtual void OnConflict(GitConflictEventArgs e)
        {
        }
    }
}
