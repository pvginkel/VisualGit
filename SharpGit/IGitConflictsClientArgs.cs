using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public interface IGitConflictsClientArgs
    {
        event EventHandler<GitConflictEventArgs> Conflict;
    }
}
