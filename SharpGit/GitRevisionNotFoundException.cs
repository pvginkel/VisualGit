using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitRevisionNotFoundException : GitException
    {
        public GitRevisionNotFoundException()
        {
        }

        public GitRevisionNotFoundException(string message)
            : base(message)
        {
        }

        public GitRevisionNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitRevisionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
