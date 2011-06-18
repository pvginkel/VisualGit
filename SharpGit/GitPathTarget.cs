using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    public sealed class GitPathTarget : GitTarget
    {
        readonly string _path;
        readonly string _fullPath;

        private static string GetFullTarget(string path)
        {
            return Path.GetFullPath(path);
        }

        public static string GetTargetPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (!IsNotUri(path))
                throw new ArgumentException("Path may not be an uri", "path");

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

        public GitPathTarget(string path, GitRevision revision)
            : base(revision)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (!IsNotUri(path))
                throw new ArgumentException("Path may not be an uri", "path");

            _path = GetTargetPath(path);
            _fullPath = GetFullTarget(_path);
        }

        public GitPathTarget(string path)
            : this(path, GitRevision.None)
        {
        }

        public GitPathTarget(string path, string revision)
            : this(path, new GitRevision(revision))
        {
        }

        public GitPathTarget(string path, DateTime date)
            : this(path, new GitRevision(date))
        {
        }

        public override string TargetName { get { return _path; } }

        public override string FileName { get { return Path.GetFileName(_path); } }

        internal override string GitTargetName
        {
            get
            {
                return _path.Replace(System.IO.Path.DirectorySeparatorChar, '/').TrimEnd('/');
            }
        }

        public string TargetPath { get { return _path; } }

        public string FullPath { get { return _fullPath; } }

        public static ICollection<GitPathTarget> Map(IEnumerable<string> paths)
        {
            var result = new List<GitPathTarget>();

            foreach (string path in paths)
            {
                result.Add(path);
            }

            return result;
        }

        public static new GitPathTarget FromString(string value)
        {
            return new GitPathTarget(value);
        }

        public static implicit operator GitPathTarget(string value)
        {
            return value != null ? new GitPathTarget(value) : null;
        }

        internal override GitRevision GetGitRevision(GitRevision fileNoneValue, GitRevision uriNoneValue)
        {
            if (Revision.RevisionType != GitRevisionType.None)
                return Revision;
            else
                return fileNoneValue;
        }

        private static bool IsNotUri(string path)
        {
            if (String.IsNullOrEmpty(path))
                return false;

            // Use the same stupid algorithm subversion uses to choose between Uri's and paths
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                switch (c)
                {
                    case '\\':
                    case '/':
                        return true;

                    case ':':
                        if (i < path.Length - 2)
                        {
                            if ((path[i + 1] == '/') && (path[i + 2] == '/'))
                                return false;
                        }
                        return true;

                    default:
                        if (!Char.IsLetter(c))
                            return true;
                        break;
                }
            }
            return true;
        }
    }
}
