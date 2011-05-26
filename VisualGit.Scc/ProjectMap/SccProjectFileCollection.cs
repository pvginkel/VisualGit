using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace VisualGit.Scc.ProjectMap
{
    sealed class SccProjectFileCollection : KeyedCollection<string, SccProjectFileReference>
    {
        public SccProjectFileCollection()
            : base(StringComparer.OrdinalIgnoreCase, 0)
        {
        }

        protected override string GetKeyForItem(SccProjectFileReference item)
        {
            return item.Filename;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out SccProjectFileReference value)
        {
            if (Dictionary != null)
                return Dictionary.TryGetValue(key, out value);

            foreach (SccProjectFileReference p in Items)
            {
                if (Comparer.Equals(GetKeyForItem(p), key))
                {
                    value = p;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
