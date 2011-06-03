using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;
using System.Threading;

namespace SharpGit
{
    internal sealed class RepositoryEntry
    {
        private readonly object _syncLock = new object();

        public RepositoryEntry(Repository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            Repository = repository;
        }

        public Repository Repository { get; private set; }

        public IDisposable Lock()
        {
            return new RepositoryLock(this);
        }

        private class RepositoryLock : IDisposable
        {
            private readonly RepositoryEntry _entry;
            private bool _disposed;
            private bool _locked;

            public RepositoryLock(RepositoryEntry entry)
            {
                _entry = entry;

                // Prevent deadlocking on the repository. When the repository
                // is already locked, we fail unconditionally.

                if (!Monitor.TryEnter(entry._syncLock, TimeSpan.FromSeconds(5)))
                    throw new GitRepositoryLockedException();
                else
                    _locked = true;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_locked)
                    {
                        Monitor.Exit(_entry._syncLock);
                        _locked = false;
                    }

                    _disposed = true;
                }
            }
        }
    }
}
