using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.Services.StatusManager
{
    internal class DirectoryCache : Dictionary<string, CacheEntry>
    {
        public DirectoryCache()
            : base(FileSystemUtil.StringComparer)
        {
        }
    }
}
