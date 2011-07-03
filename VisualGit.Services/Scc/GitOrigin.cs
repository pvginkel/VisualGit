// VisualGit.Services\Scc\GitOrigin.cs
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
using System.Globalization;
using SharpGit;
using System.IO;

namespace VisualGit.Scc
{
    /// <summary>
    /// Container of a <see cref="SvnTarget"/>, its repository Uri and its repository root.
    /// </summary>
    public class GitOrigin : IEquatable<GitOrigin>, IFormattable
    {
        readonly GitTarget _target;
        readonly string _reposRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class using a GitItem
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="gitItem">The SVN item.</param>
        public GitOrigin(GitItem gitItem)
        {
            if (gitItem == null)
                throw new ArgumentNullException("gitItem");

            if (!gitItem.IsVersioned)
                throw new InvalidOperationException("Can only create a GitOrigin from versioned items");

            _target = new GitTarget(gitItem.FullPath);
            _reposRoot = gitItem.WorkingCopy.RepositoryRoot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="reposRoot">The repos root.</param>
        public GitOrigin(string path, string reposRoot)
            : this(new GitTarget(path, GitRevision.Head), reposRoot)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="path">The URI.</param>
        /// <param name="origin">The original origin to calculate from</param>
        public GitOrigin(string path, GitOrigin origin)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            else if (origin == null)
                throw new ArgumentNullException("origin");

            GitTarget target = new GitTarget(path, origin.Target.Revision);
            _target = target;
            _reposRoot = origin.RepositoryRoot;

            Debug.Assert(
                String.Equals(GitTools.GetRepositoryRoot(path), _reposRoot, FileSystemUtil.StringComparison),
                "path must be of the repository pointed to by origin"
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class.
        /// </summary>
        /// <param name="target">The URI target.</param>
        /// <param name="reposRoot">The repos root.</param>
        public GitOrigin(GitTarget target, string reposRoot)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            else if (reposRoot == null)
                throw new ArgumentNullException("reposRoot");

            _target = target;
            _reposRoot = reposRoot;

            Debug.Assert(
                String.Equals(GitTools.GetRepositoryRoot(_target.FullPath), _reposRoot, FileSystemUtil.StringComparison),
                "path must be of the repository pointed to by origin"
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitOrigin"/> class from a SvnTarget
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <param name="reposRoot">The repos root or <c>null</c> to retrieve the repository root from target</param>
        public GitOrigin(IVisualGitServiceProvider context, GitTarget target, string reposRoot)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (target == null)
                throw new ArgumentNullException("target");

            GitItem item = context.GetService<IFileStatusCache>()[target.FullPath];

            if (item == null || !item.IsVersioned)
                throw new InvalidOperationException("Can only create a GitOrigin from versioned items");

            _target = target;
            _reposRoot = item.WorkingCopy.RepositoryRoot; // BH: Prefer the actual root over the provided

            Debug.Assert(String.Equals(_reposRoot, reposRoot, FileSystemUtil.StringComparison));
        }

        /// <summary>
        /// Gets the repository root.
        /// </summary>
        /// <value>The repository root.</value>
        public string RepositoryRoot
        {
            get { return _reposRoot; }
        }

        /// <summary>
        /// Gets the target of the item
        /// </summary>
        /// <value>The target.</value>
        public GitTarget Target
        {
            get { return _target; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitOrigin);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GitOrigin other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if ((object)other == null)
                return false;

            return
                other._target == _target &&
                String.Equals(_reposRoot, other._reposRoot, FileSystemUtil.StringComparison);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _target.GetHashCode() ^ _reposRoot.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="o1">The o1.</param>
        /// <param name="o2">The o2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GitOrigin o1, GitOrigin o2)
        {
            if (ReferenceEquals(o1, o2))
                return true;

            if ((object)o1 == null || (object)o2 == null)
                return false;

            return o1.Equals(o2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="o1">The o1.</param>
        /// <param name="o2">The o2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GitOrigin o1, GitOrigin o2)
        {
            return !(o1 == o2);
        }

        /// <summary>
        /// Determines whether the origin specified the repositoryroot
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is repository root]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRepositoryRoot
        {
            get
            {
                return String.Equals(
                    Path.GetFullPath(_reposRoot),
                    Path.GetFullPath(_target.FullPath),
                    FileSystemUtil.StringComparison
                );
            }
        }

        #region IFormattable Members

        public override string ToString()
        {
            return ToString("", CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return Target.ToString();

            switch (format)
            {
                case "t":
                    return Target.ToString();
                case "f":
                    return Target.FileName;
                case "s":
                    return Target.FileName + " (" + Target.ToString() + ")";
                default:
                    return Target.ToString();
            }
        }

        #endregion
    }
}
