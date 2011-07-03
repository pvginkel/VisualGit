using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitRepositoryLockedException : GitException
    {
        public GitRepositoryLockedException()
        {
        }

        public GitRepositoryLockedException(string message)
            : base(message)
        {
        }

        public GitRepositoryLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitRepositoryLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
