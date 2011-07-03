// VisualGit.Services\Scc\IFileStatusCache.cs
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
using System.Text;
using SharpGit;

namespace VisualGit.Scc
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>The default implementation of this service is thread safe</remarks>
    public interface IFileStatusCache : IVisualGitServiceProvider
    {
        /// <summary>
        /// Gets the <see cref="GitItem"/> with the specified path.
        /// </summary>
        /// <value></value>
        GitItem this[string path] { get; }

        /// <summary>
        /// Marks the specified path dirty
        /// </summary>
        /// <param name="path">A file of directory</param>
        /// <remarks>If the file is in the cache</remarks>
        void MarkDirty(string path);

        /// <summary>
        /// Marks the specified paths dirty
        /// </summary>
        /// <param name="paths">The paths.</param>
        void MarkDirty(IEnumerable<string> paths);

        /// <summary>
        /// Clears the whole statuscache; called when closing the solution
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Called from <see cref="GitItem.Refresh()"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="diskNodeKind">The on-disk node kind if it is known to be correct.</param>
        void RefreshItem(GitItem item, GitNodeKind diskNodeKind);

        /// <summary>
        /// Gets the <see cref="GitDirectory"/> of the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        GitDirectory GetDirectory(string path);

        void MarkDirtyRecursive(string path);

        IList<GitItem> GetCachedBelow(string path);
        IList<GitItem> GetCachedBelow(IEnumerable<string> paths);

        void SetSolutionContained(string path, bool contained);
    }
}
