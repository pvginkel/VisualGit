﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class GitCouldNotLockException : GitException
    {
        public GitCouldNotLockException()
        {
        }

        public GitCouldNotLockException(string message)
            : base(message)
        {
        }

        public GitCouldNotLockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GitCouldNotLockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
