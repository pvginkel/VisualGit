using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitRepositoryLockedException : GitException
    {
        public GitRepositoryLockedException()
            : base(GitErrorCode.RepositoryLocked)
        {
        }
    }
}
