using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitCreateRepositoryArgs : GitClientArgs
    {
        public GitCreateRepositoryArgs()
            : base(GitCommandType.CreateRepository)
        {
        }
    }
}
