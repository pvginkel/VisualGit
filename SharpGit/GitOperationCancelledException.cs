using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitOperationCancelledException : GitException
    {
        public GitOperationCancelledException()
        {
        }

        public GitOperationCancelledException(string message)
            : base(message)
        {
        }

        public GitOperationCancelledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitOperationCancelledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
