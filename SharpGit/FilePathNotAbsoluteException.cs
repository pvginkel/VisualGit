using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharpGit
{
    [Serializable]
    public class FilePathNotAbsoluteException : ArgumentException
    {
        public FilePathNotAbsoluteException()
            : base(Properties.Resources.PathNotAbsolute)
        {
        }

        public FilePathNotAbsoluteException(string paramName)
            : base(Properties.Resources.PathNotAbsolute, paramName)
        {
        }

        public FilePathNotAbsoluteException(string paramName, Exception innerException)
            : base(Properties.Resources.PathNotAbsolute, paramName, innerException)
        {
        }

        protected FilePathNotAbsoluteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
