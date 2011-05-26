using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace VisualGit.Scc
{
    public class GitItemsEventArgs : EventArgs
    {
        readonly ReadOnlyCollection<GitItem> _changedItems;

        public GitItemsEventArgs(IList<GitItem> changedItems)
        {
            if(changedItems == null)
                throw new ArgumentNullException("changedItems");

            _changedItems = new ReadOnlyCollection<GitItem>(changedItems);
        }

        public ReadOnlyCollection<GitItem> ChangedItems
        {
            get { return _changedItems; }
        }
    }

    public interface IGitItemChange
    {
        /// <summary>
        /// Occurs when the state of one or more <see cref="GitItem"/> instances changes
        /// </summary>
        event EventHandler<GitItemsEventArgs> GitItemsChanged;

        /// <summary>
        /// Raises the <see cref="E:GitItemsChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="GitItemsEventArgs"/> instance containing the event data.</param>
        void OnGitItemsChanged(GitItemsEventArgs e);
    }    
}
