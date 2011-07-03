using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitMoveObstructedException : GitException
    {
        public GitMoveObstructedException()
        {
        }

        public GitMoveObstructedException(string message)
            : base(message)
        {
        }

        public GitMoveObstructedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitMoveObstructedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
