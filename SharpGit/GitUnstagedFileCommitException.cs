using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitUnstagedFileCommitException : GitException
    {
        public GitUnstagedFileCommitException()
        {
        }

        public GitUnstagedFileCommitException(string message)
            : base(message)
        {
        }

        public GitUnstagedFileCommitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitUnstagedFileCommitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
