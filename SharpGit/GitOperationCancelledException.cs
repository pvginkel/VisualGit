using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitOperationCancelledException : GitException
    {
        public GitOperationCancelledException()
            : base(GitErrorCode.OperationCancelled)
        {
        }
    }
}
