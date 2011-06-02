using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitException : Exception
    {
        public GitErrorCode ErrorCode { get; set; }

        public GitErrorCategory ErrorCategory { get; private set; }

        public GitException(GitErrorCode errorCode)
            : base(errorCode.Message)
        {
            if (errorCode == null)
                throw new ArgumentNullException("errorCode");

            ErrorCode = errorCode;
        }

        public GitException(GitErrorCode errorCode, Exception innerException)
            : base(errorCode.Message, innerException)
        {
            if (errorCode == null)
                throw new ArgumentNullException("errorCode");

            ErrorCode = errorCode;
        }

        protected GitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
