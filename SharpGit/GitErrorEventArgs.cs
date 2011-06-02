using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public class GitErrorEventArgs : EventArgs
    {
        public GitErrorEventArgs(GitException exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            Exception = exception;
        }

        public GitException Exception { get; private set; }
    }
}
