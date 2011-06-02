using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    internal static class StreamExtensions
    {
        public static void CopyTo(this Stream self, Stream stream)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] buffer = new byte[4096];
            int read;

            while ((read = self.Read(buffer, 0, buffer.Length)) > 0)
                stream.Write(buffer, 0, read);
        }
    }
}
