using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public abstract class GitTransportClientArgs : GitClientArgs
    {
        protected GitTransportClientArgs(GitCommandType commandType)
            : base(commandType)
        {
        }

        public event EventHandler<GitCredentialsEventArgs> Credentials;
        public event EventHandler<GitCredentialsEventArgs> CredentialsSupported;
        public event EventHandler<GitProgressEventArgs> Progress;

        internal protected virtual void OnCredentials(GitCredentialsEventArgs e)
        {
            var ev = Credentials;

            if (ev != null)
                ev(this, e);
        }

        internal protected virtual void OnCredentialsSupported(GitCredentialsEventArgs e)
        {
            var ev = CredentialsSupported;

            if (ev != null)
                ev(this, e);
        }

        internal protected virtual void OnProgress(GitProgressEventArgs e)
        {
            var ev = Progress;

            if (ev != null)
                ev(this, e);
        }
    }
}
