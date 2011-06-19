using System;
using System.Collections.Generic;
using System.Text;
using SharpGit;

namespace VisualGit.Scc
{
    public interface IGitRepositoryItem
    {
        /// <summary>
        /// Gets the Uri of the item (Required)
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the <see cref="GitNodeKind"/> of the item (Optional)
        /// </summary>
        GitNodeKind NodeKind { get; }
        /// <summary>
        /// Gets the <see cref="SvnRevision"/> of the item (Optional)
        /// </summary>
        GitRevision Revision { get; }

        /// <summary>
        /// Refreshes the item.
        /// </summary>
        void RefreshItem(bool refreshParent);

        /// <summary>
        /// Gets the origin.
        /// </summary>
        /// <value>The origin.</value>
        GitOrigin Origin { get; }
    }
}
