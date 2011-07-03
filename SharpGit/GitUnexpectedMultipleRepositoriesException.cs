using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitUnexpectedMultipleRepositoriesException : GitException
    {
        public GitUnexpectedMultipleRepositoriesException()
        {
        }

        public GitUnexpectedMultipleRepositoriesException(string message)
            : base(message)
        {
        }

        public GitUnexpectedMultipleRepositoriesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitUnexpectedMultipleRepositoriesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
