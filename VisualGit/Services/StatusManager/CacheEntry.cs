using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.Services.StatusManager
{
    internal class CacheEntry
    {
        public GitFileStatus Status { get; private set; }

        public CacheEntry(GitFileStatus status)
        {
            Status = status;
        }
    }

    internal class CacheDirectoryEntry : CacheEntry
    {
        public DirectoryCache Items { get; private set; }

        public CacheDirectoryEntry(GitFileStatus status)
            : base(status)
        {
            Items = new DirectoryCache();
        }
    }
}
