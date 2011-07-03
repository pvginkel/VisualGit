using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitRemoteRefsResult : GitCommandResult
    {
        public GitRemoteRefsResult()
        {
            Items = new List<GitRef>();
        }

        public ICollection<GitRef> Items { get; private set; }
    }
}
