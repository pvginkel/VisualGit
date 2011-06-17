using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SharpGit
{
    [Serializable]
    public class FilePathAbsoluteException : ArgumentException
    {
        public FilePathAbsoluteException()
            : base(Properties.Resources.PathNotAbsolute)
        {
        }

        public FilePathAbsoluteException(string paramName)
            : base(Properties.Resources.PathNotAbsolute, paramName)
        {
        }

        public FilePathAbsoluteException(string paramName, Exception innerException)
            : base(Properties.Resources.PathNotAbsolute, paramName, innerException)
        {
        }

        protected FilePathAbsoluteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
