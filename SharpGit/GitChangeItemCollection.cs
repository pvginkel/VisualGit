using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public sealed class GitChangeItemCollection : KeyedCollection<string, GitChangeItem>
    {
        protected override sealed string GetKeyForItem(GitChangeItem item)
        {
            return item.Path;
        }
    }
}
