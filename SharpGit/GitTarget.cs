// SharpGit\GitTarget.cs
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

namespace SharpGit
{
    public sealed class GitTarget : IEquatable<GitTarget>
    {
        readonly GitRevision _revision;
        readonly string _path;
        readonly string _fullPath;

        public GitTarget(string path, GitRevision revision)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (revision == null)
                _revision = GitRevision.None;
            else
                _revision = revision;

            _path = SanitizeTargetPath(path);
            _fullPath = Path.GetFullPath(_path);
        }

        public GitTarget(string path)
            : this(path, GitRevision.None)
        {
        }

        public GitTarget(string path, string revision)
            : this(path, new GitRevision(revision))
        {
        }

        public GitTarget(string path, DateTime date)
            : this(path, new GitRevision(date))
        {
        }

        public string TargetPath { get { return _path; } }

        public string FullPath { get { return _fullPath; } }

        private string SanitizeTargetPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (Path.IsPathRooted(path))
                return GitTools.GetNormalizedFullPath(path);

            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string dualSeparator = String.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);

            int nNext;
            // Remove double backslash
            while ((nNext = path.IndexOf(dualSeparator, StringComparison.Ordinal)) >= 0)
                path = path.Remove(nNext, 1);

            // Remove '\.\'
            while ((nNext = path.IndexOf("\\.\\", StringComparison.Ordinal)) >= 0)
                path = path.Remove(nNext, 2);

            while (path.StartsWith(".\\", StringComparison.Ordinal))
                path = path.Substring(2);

            if (path.EndsWith("\\.", StringComparison.Ordinal))
                path = path.Substring(0, path.Length - 2);

            path = path.TrimEnd(Path.DirectorySeparatorChar);

            if (path.Length == 0)
                path = ".";

            return path;
        }

        /// <summary>Gets the operational revision</summary>
        public GitRevision Revision
        {
            get { return _revision; }
        }

        /// <summary>Gets the target name in normalized format</summary>
        internal string GitTargetName
        {
            get { return _path.Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/'); }
        }

        public string FileName { get { return Path.GetFileName(_path); } }

        /// <summary>Gets the GitTarget as string</summary>
        public override string ToString()
        {
            if (Revision.RevisionType == GitRevisionType.None)
                return TargetPath;
            else
                return TargetPath + "@" + Revision.ToString();
        }

        public static implicit operator GitTarget(string value)
        {
            return value != null ? new GitTarget(value) : null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GitTarget);
        }

        public bool Equals(GitTarget other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (!String.Equals(other.GitTargetName, GitTargetName, FileSystemUtil.StringComparison))
                return false;

            return Revision.Equals(other.Revision);
        }

        /// <summary>Serves as a hashcode for the specified type</summary>
        public override int GetHashCode()
        {
            return TargetPath.GetHashCode();
        }

        internal GitRevision GetGitRevision(GitRevision fileNoneValue, GitRevision uriNoneValue)
        {
            if (Revision.RevisionType != GitRevisionType.None)
                return Revision;
            else
                return fileNoneValue;
        }

        public static ICollection<GitTarget> Map(IEnumerable<string> paths)
        {
            var result = new List<GitTarget>();

            foreach (string path in paths)
            {
                result.Add(path);
            }

            return result;
        }

        public static bool operator ==(GitTarget a, GitTarget b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(GitTarget a, GitTarget b)
        {
            return !(a == b);
        }
    }
}