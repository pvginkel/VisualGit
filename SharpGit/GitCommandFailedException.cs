using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitCommandFailedException : GitException
    {
        public GitCommandFailedException()
        {
        }

        public GitCommandFailedException(string message)
            : base(message)
        {
        }

        public GitCommandFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitCommandFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
