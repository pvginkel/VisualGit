using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitNoRepositoryException : GitException
    {
        public GitNoRepositoryException()
        {
        }

        public GitNoRepositoryException(string message)
            : base(message)
        {
        }

        public GitNoRepositoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitNoRepositoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
