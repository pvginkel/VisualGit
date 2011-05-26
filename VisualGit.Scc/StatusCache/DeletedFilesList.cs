using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace VisualGit.Scc.StatusCache
{
    public class DeletedGitItemList : KeyedCollection<string, GitItem>
    {
        public DeletedGitItemList()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(GitItem item)
        {
            return item.FullPath;
        }
    }
}
