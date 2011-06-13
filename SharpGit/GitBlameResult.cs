using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharpGit
{
    internal class GitBlameResult : GitCommandResult
    {
        public GitBlameResult()
        {
            Items = new Collection<GitBlameEventArgs>();
        }

        public Collection<GitBlameEventArgs> Items { get; private set; }
    }
}
