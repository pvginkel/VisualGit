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
            CommandType = commandType;
        }

        public bool ThrowOnError { get; set; }

        public GitException LastException { get; internal set; }

        public GitCommandType CommandType { get; private set; }

        protected virtual void OnNotify(GitNotifyEventArgs e)
        {
            var ev = Notify;

            if (ev != null)
                Notify(this, e);
        }

        protected virtual void OnGitError(GitErrorEventArgs e)
        {
            var ev = GitError;

            if (ev != null)
                ev(this, e);
        }

        public void AddExpectedError(params GitErrorCategory[] errorCategories)
        {
            if (errorCategories == null)
                throw new ArgumentNullException("errorCategories");

            foreach (var item in errorCategories)
            {
                AddExpectedError(item);
            }
        }

        public event EventHandler<GitErrorEventArgs> GitError;

        public event EventHandler<GitNotifyEventArgs> Notify;

        internal void SetError(GitException exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            LastException = exception;

            OnGitError(new GitErrorEventArgs(LastException));
        }

        internal void RaiseNotify(GitNotifyEventArgs e)
        {
            OnNotify(e);
        }
    }
}
