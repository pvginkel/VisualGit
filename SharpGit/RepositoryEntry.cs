// SharpGit\RepositoryEntry.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
        private readonly object _syncLock = new object();

        public RepositoryEntry(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            Path = repositoryPath;

            Repository = CreateNew();
        }

        public Repository Repository { get; private set; }
        public string Path { get; private set; }

        private Repository CreateNew()
        {
            var builder = new RepositoryBuilder();

            builder.ReadEnvironment();
            builder.FindGitDir(Path);

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
