using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;

namespace SharpGit
{
    internal sealed class RepositoryEntry
    {
        public RepositoryEntry(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            Repository = repository;
            SyncLock = new object();
        }

        public Repository Repository { get; private set; }

        public object SyncLock { get; private set; }
    }
}
