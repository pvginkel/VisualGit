﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public sealed class GitSwitchResult : GitCommandResult
    {
        public string PostSwitchError { get; internal set; }
    }
}