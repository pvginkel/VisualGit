using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitCouldNotLockException : GitException
    {
        public GitCouldNotLockException()
            : base(GitErrorCode.CouldNotLock)
        {
        }
    }
}
