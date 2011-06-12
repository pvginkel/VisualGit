using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public class GitInfoEventArgs : GitCommandResult
    {
        public string ConflictOld { get; set; }

        public string ConflictNew { get; set; }

        public string ConflictWork { get; set; }
    }
}
