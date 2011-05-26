using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VisualGit.Scc
{
    public interface IGitDirectoryUpdate
    {
        void TickAll();
        void TickFiles();

        void Store(GitItem item);

        bool ScheduleForCleanup { get; }
    }
    /// <summary>
    /// Collection of <see cref="GitItem"/> instances of a specific directory
    /// </summary>
    /// <remarks>
    /// <para>A GitDirectory contains the directory itself and all files and directories contained directly within</para>
    /// 
    /// <para>Note: This tells us that all subdirectories are contained in the parent and in their own <see cref="GitDirectory"/></para>
    /// </remarks>
    public sealed class GitDirectory : KeyedCollection<string, GitItem>, IGitDirectoryUpdate
    {
        readonly IVisualGitServiceProvider _context;
        readonly string _fullPath;

        public GitDirectory(IVisualGitServiceProvider context, string fullPath)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException("fullPath");

            _context = context;
            _fullPath = fullPath;
        }

        protected override string GetKeyForItem(GitItem item)
        {
            return item.FullPath;
        }

        /// <summary>
        /// Gets the directory item
        /// </summary>
        /// <value>The directory.</value>
        public GitItem Directory
        {
            [DebuggerStepThrough]
            get 
            {
                if (Contains(_fullPath))
                    return this[_fullPath]; // 99.9% case
                else
                {
                    // Get the item from the status cache
                    IFileStatusCache cache = _context.GetService<IFileStatusCache>();

                    if (cache == null)
                        return null;

                    GitItem item = cache[_fullPath];

                    if (item != null)
                    {
                        if(!Contains(_fullPath))
                            Add(item); // In most cases the file is added by the cache
                    }

                    return item;
                }
            }
        }

        /// <summary>
        /// Gets the full path of the directory
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            [DebuggerStepThrough]
            get 
            {
                if (Contains(_fullPath))
                    return this[_fullPath].FullPath;
                else
                    return _fullPath;
            }
        }

        /// <summary>
        /// Tick all items
        /// </summary>
        void IGitDirectoryUpdate.TickAll()
        {
            foreach (IGitItemUpdate item in this)
            {
                item.TickItem();
            }
        }

        /// <summary>
        /// Tick all files and the directory itself
        /// </summary>
        void IGitDirectoryUpdate.TickFiles()
        {
            foreach (GitItem item in this)
            {
                if(item.IsFile || item == Directory)
                    ((IGitItemUpdate)item).TickItem();
            }

            ((IGitItemUpdate)this.Directory).TickItem();
        }

        void IGitDirectoryUpdate.Store(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Remove(item.FullPath);
            Add(item);
        }

        #region IGitDirectoryUpdate Members

        bool IGitDirectoryUpdate.ScheduleForCleanup
        {
            get
            {
                foreach (GitItem item in this)
                {
                    if (((IGitItemUpdate)item).IsItemTicked())
                        return true;
                }

                return false;
            }
        }

        #endregion
    }
}
