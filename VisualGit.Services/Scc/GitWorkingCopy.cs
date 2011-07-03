// VisualGit.Services\Scc\GitWorkingCopy.cs
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
using System.Diagnostics;
using SharpGit;

namespace VisualGit.Scc
{
    interface IGitWcReference
    {
        GitWorkingCopy WorkingCopy { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("WorkingCopy={FullPath}")]
    public sealed class GitWorkingCopy : IEquatable<GitWorkingCopy>, IGitWcReference
    {
        readonly GitItem _rootItem;
        IVisualGitServiceProvider _context;
        bool _checkedUri;
        string _repositoryRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitWorkingCopy"/> class.
        /// </summary>
        /// <param name="rootItem">The root item.</param>
        GitWorkingCopy(IVisualGitServiceProvider context, GitItem rootItem)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (rootItem == null)
                throw new ArgumentNullException("rootItem");

            _context = context;
            _rootItem = rootItem;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _rootItem.FullPath; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            return Equals(obj as GitWorkingCopy);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FullPath);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GitWorkingCopy other)
        {
            if((object)other == null)
                return false;
            
            return StringComparer.OrdinalIgnoreCase.Equals(other.FullPath, FullPath);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="wc1">The WC1.</param>
        /// <param name="wc2">The WC2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GitWorkingCopy wc1, GitWorkingCopy wc2)
        {
            bool n1 = (object)wc1 == null;
            bool n2 = (object)wc2 == null;

            if (n1 || n2)
                return n1 && n2;

            return wc1.Equals(wc2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="wc1">The WC1.</param>
        /// <param name="wc2">The WC2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GitWorkingCopy wc1, GitWorkingCopy wc2)
        {
            bool n1 = (object)wc1 == null;
            bool n2 = (object)wc2 == null;

            if (n1 || n2)
                return !(n1 && n2);

            return !wc1.Equals(wc2);
        }

        GitWorkingCopy IGitWcReference.WorkingCopy
        {
            get { return this; }
        }

        internal static IGitWcReference CalculateWorkingCopy(IVisualGitServiceProvider context, GitItem gitItem)
        {
            if (gitItem == null)
                throw new ArgumentNullException("gitItem");

            // We can assume the GitItem is a directory; this is verified in GitItem.WorkingCopy
            GitItem parent = gitItem.Parent;

            if (parent != null)
            {
                if (!gitItem.IsVersioned)
                    return parent;

                if (parent.IsVersioned)
                    return parent;
            }

            return new GitWorkingCopy(context, gitItem);
        }

        public string RepositoryRoot
        {
            get { return _repositoryRoot ?? (_repositoryRoot = GetRepositoryRoot()); }
        }

        private string GetRepositoryRoot()
        {
            if (_checkedUri)
                return null;

            _checkedUri = true;

            string repositoryRoot;

            GitTools.TryGetRepositoryRoot(FullPath, out repositoryRoot);

            return repositoryRoot;
        }
    }
}
