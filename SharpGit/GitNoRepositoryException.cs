using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitNoRepositoryException : GitException
    {
        public GitNoRepositoryException()
            : base(GitErrorCode.PathNoRepository)
        {
        }
    }
}
