using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitCloneResult : GitCommandResult
    {
        public string PostCloneError { get; internal set; }
    }
}
