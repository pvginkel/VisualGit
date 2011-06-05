using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;
using System.Threading;

namespace SharpGit
{
    internal sealed class RepositoryEntry : IDisposable
    {
        private bool _disposed;
        private readonly string _repositoryPath;
        private readonly object _syncLock = new object();

        public RepositoryEntry(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            _repositoryPath = repositoryPath;

            Repository = CreateNew();
        }

        public Repository Repository { get; private set; }

        private Repository CreateNew()
        {
            var builder = new RepositoryBuilder();

            builder.ReadEnvironment();
            builder.FindGitDir(_repositoryPath);

            return builder.Build();
        }

        public IDisposable Lock()
        {
            return new RepositoryLock(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Repository != null)
                {
                    Repository.Close();
                    Repository = null;
                }

                _disposed = true;
            }
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
