using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitNotifyState
    {
        None,
        Unknown,
        //Unchanged,
        //Missing,
        //Obstructed,
        Changed,
        //Merged,
        //Conflicted,
    }
}
