// SharpGit\RepositoryExtensions.cs
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
using NGit.Api.Errors;
using NGit.Treewalk;
using NGit.Dircache;
using System.Diagnostics;
using NGit.Revwalk;

namespace SharpGit
{
    internal static class RepositoryExtensions
    {
        public static string GetRepositoryPath(this Repository repository, string fullPath)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");
            if (repository == null)
                throw new ArgumentNullException("repository");

            string relativePath;
            string workPath = repository.WorkTree;

            Debug.Assert(
                String.Equals(fullPath.Substring(0, workPath.Length), workPath, StringComparison.OrdinalIgnoreCase),
                "Item path is not located in the repository"
            );

            relativePath = fullPath
                .Substring(workPath.Length)
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');

            return relativePath;
        }

        public static string GetAbsoluteRepositoryPath(this Repository repository, string relativePath)
        {
            if (relativePath == null)
                throw new ArgumentNullException("relativePath");
            if (repository == null)
                throw new ArgumentNullException("repository");

            return Path.Combine(
                repository.WorkTree, relativePath.Replace('/', Path.DirectorySeparatorChar)
            );
        }
    }
}
