using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitCertificate
    {
        public GitCertificate(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            Path = path;
        }

        public string Path { get; private set; }
    }
}
