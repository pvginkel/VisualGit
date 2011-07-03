using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitClientBinaryFileException : GitException
    {
        public GitClientBinaryFileException()
        {
        }

        public GitClientBinaryFileException(string message)
            : base(message)
        {
        }

        public GitClientBinaryFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitClientBinaryFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
