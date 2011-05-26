using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace VisualGit.Scc
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>The default implementation of this service is thread safe</remarks>
    public interface IFileStatusCache : IVisualGitServiceProvider
    {
        /// <summary>
        /// Gets the <see cref="GitItem"/> with the specified path.
        /// </summary>
        /// <value></value>
        GitItem this[string path] { get; }

        /// <summary>
        /// Marks the specified path dirty
        /// </summary>
        /// <param name="path">A file of directory</param>
        /// <remarks>If the file is in the cache</remarks>
        void MarkDirty(string path);

        /// <summary>
        /// Marks the specified paths dirty
        /// </summary>
        /// <param name="paths">The paths.</param>
        void MarkDirty(IEnumerable<string> paths);

        /// <summary>
        /// Clears the whole statuscache; called when closing the solution
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Called from <see cref="GitItem.Refresh()"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="diskNodeKind">The on-disk node kind if it is known to be correct.</param>
        void RefreshItem(GitItem item, SvnNodeKind diskNodeKind);

        /// <summary>
        /// Refreshes the nested status of the <see cref="GitItem"/>
        /// </summary>
        /// <param name="item"></param>
        void RefreshNested(GitItem item);

        /// <summary>
        /// Gets the <see cref="GitDirectory"/> of the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        GitDirectory GetDirectory(string path);

        void MarkDirtyRecursive(string path);

        IList<GitItem> GetCachedBelow(string path);
        IList<GitItem> GetCachedBelow(IEnumerable<string> paths);

        void SetSolutionContained(string path, bool contained);
    }
}
