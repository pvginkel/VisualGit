using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public enum GitRevisionType
    {
        /// <summary>
        /// Unset revision type.
        /// </summary>
        None,
        /// <summary>
        /// Revision based on a hash.
        /// </summary>
        Hash,
        /// <summary>
        /// Revision based on time.
        /// </summary>
        Time,
        /// <summary>
        /// Last revision a specific item has been changed, before or equal to Base.
        /// </summary>
        Committed,
        /// <summary>
        /// One version before Committed.
        /// </summary>
        Previous,
        /// <summary>
        /// Currently checked out version.
        /// </summary>
        Base,
        /// <summary>
        /// Working directory, including changes.
        /// </summary>
        Working,
        /// <summary>
        /// Head of the current branch.
        /// </summary>
        Head,
        /// <summary>
        /// First revision.
        /// </summary>
        Zero,
        /// <summary>
        /// Second revision.
        /// </summary>
        One
    }
}
