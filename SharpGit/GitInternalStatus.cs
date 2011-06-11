using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    [Flags]
    public enum GitInternalStatus
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unset = 0,
        /// <summary>
        /// New, staged file.
        /// </summary>
        Added = 1,
        AssumeUnchanged = 2,
        /// <summary>
        /// Modified, staged file.
        /// </summary>
        Changed = 4,
        /// <summary>
        /// Modified, unstaged file.
        /// </summary>
        Modified = 8,
        /// <summary>
        /// Deleted, unstaged file.
        /// </summary>
        Missing = 16,
        /// <summary>
        /// Deleted, staged file.
        /// </summary>
        Removed = 32,
        /// <summary>
        /// New, unstaged file.
        /// </summary>
        Untracked = 64,
        /// <summary>
        /// Ignored file.
        /// </summary>
        Ignored = 128
    }
}
