// SharpGit\RepositoryManager.cs
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
using System.IO;
using NGit;

namespace SharpGit
{
    internal static class RepositoryManager
    {
        private static readonly object _syncLock = new object();
        private static readonly Dictionary<string, string> _repositoryRootLookup = new Dictionary<string, string>(FileSystemUtil.StringComparer);
        private static readonly Dictionary<string, RepositoryEntry> _repositoryCache = new Dictionary<string, RepositoryEntry>(FileSystemUtil.StringComparer);

        public static RepositoryEntry GetRepository(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            lock (_syncLock)
            {
                // We expect this method to be called very very often with the
                // same path name. Because of this, we cache all path names we
                // receive and map them to a repostory root.

                string repositoryRoot;

                if (!_repositoryRootLookup.TryGetValue(path, out repositoryRoot))
                {
                    GitTools.TryGetRepositoryRoot(path, out repositoryRoot);
                    _repositoryRootLookup.Add(path, repositoryRoot);
                }

                if (repositoryRoot == null)
                    return null;

                RepositoryEntry entry;

                if (!_repositoryCache.TryGetValue(repositoryRoot, out entry))
                {
                    entry = new RepositoryEntry(repositoryRoot);

                    _repositoryCache.Add(repositoryRoot, entry);
                }

                return entry;
            }
        }
    }
}
