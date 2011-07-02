using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public abstract class GitClientArgs
    {
        private HashSet<GitErrorCode> _expectedErrorCodes;

        internal GitClientArgs(GitCommandType commandType)
        {
            ThrowOnError = true;
            ThrowOnCancel = true;
            CommandType = commandType;
        }

        public bool ThrowOnError { get; set; }
        public bool ThrowOnCancel { get; set; }

        public GitException LastException { get; internal set; }

        public GitCommandType CommandType { get; private set; }

        internal protected virtual void OnNotify(GitNotifyEventArgs e)
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

        internal protected virtual void OnCancel(CancelEventArgs e)
        {
            var ev = Cancel;

            if (ev != null)
                ev(this, e);
        }

        public void AddExpectedError(params GitErrorCode[] errorCodes)
        {
            if (errorCodes == null)
                throw new ArgumentNullException("errorCodes");

            foreach (var item in errorCodes)
            {
                AddExpectedError(item);
            }
        }

        public void AddExpectedError(GitErrorCode errorCode)
        {
            if (errorCode == null)
                throw new ArgumentNullException("errorCode");

            if (_expectedErrorCodes == null)
                _expectedErrorCodes = new HashSet<GitErrorCode>();

            if (!_expectedErrorCodes.Contains(errorCode))
                _expectedErrorCodes.Add(errorCode);
        }

        internal bool ShouldThrow(GitErrorCode errorCode)
        {
            if (errorCode == null)
                throw new ArgumentNullException("errorCode");

            return ThrowOnError && !IsExpected(errorCode);
        }

        internal bool IsExpected(GitErrorCode errorCode)
        {
            if (errorCode == null)
                throw new ArgumentNullException("errorCode");

            return _expectedErrorCodes != null && _expectedErrorCodes.Contains(errorCode);
        }

        public event EventHandler<GitErrorEventArgs> GitError;

        public event EventHandler<GitNotifyEventArgs> Notify;

        public event EventHandler<CancelEventArgs> Cancel;

        internal void SetError(GitException exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            LastException = exception;

            OnGitError(new GitErrorEventArgs(LastException));
        }

        protected internal virtual void OnConflict(GitConflictEventArgs e)
        {
        }
    }
}
