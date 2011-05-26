using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace VisualGit.Scc
{
    public interface ISvnRepositoryItem
    {
        /// <summary>
        /// Gets the Uri of the item (Required)
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// Gets the <see cref="SvnNodeKind"/> of the item (Optional)
        /// </summary>
        SvnNodeKind NodeKind { get; }
        /// <summary>
        /// Gets the <see cref="SvnRevision"/> of the item (Optional)
        /// </summary>
        SvnRevision Revision { get; }

        /// <summary>
        /// Refreshes the item.
        /// </summary>
        void RefreshItem(bool refreshParent);

        /// <summary>
        /// Gets the origin.
        /// </summary>
        /// <value>The origin.</value>
        SvnOrigin Origin { get; }
    }
}
