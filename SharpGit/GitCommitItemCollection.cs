using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public sealed class GitCommitItemCollection : KeyedCollection<string, GitCommitItem>
    {
        protected override sealed string GetKeyForItem(GitCommitItem item)
        {
            return item.FullPath;
        }
    }
}
