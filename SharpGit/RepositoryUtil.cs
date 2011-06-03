using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using NGit;
using NGit.Revwalk;
using NGit.Api.Errors;
using NGit.Dircache;
using NGit.Treewalk.Filter;
using NGit.Treewalk;

namespace SharpGit
{
    public static class RepositoryUtil
    {
        public static string GetRepositoryRoot(Uri path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            return GetRepositoryRoot(GitTools.GetAbsolutePath(path));
        }

        public static string GetRepositoryRoot(string path)
        {
            string result;

            if (!TryGetRepositoryRoot(path, out result))
                throw new GitNoRepositoryException();

            return result;
        }

        public static bool TryGetRepositoryRoot(Uri path, out string repositoryRoot)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            return TryGetRepositoryRoot(GitTools.GetAbsolutePath(path), out repositoryRoot);
        }

        public static bool TryGetRepositoryRoot(string path, out string repositoryRoot)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            path = Path.GetFullPath(path);
            string pathRoot = Path.GetPathRoot(path);

            while (true)
            {
                if (Directory.Exists(Path.Combine(path, Constants.DOT_GIT)))
                {
                    repositoryRoot = path;
                    return true;
                }

                if (
                    String.Empty.Equals(path) ||
                    String.Equals(path, pathRoot, FileSystemUtil.StringComparison)
                ) {
                    repositoryRoot = null;
                    return false;
                }

                path = Path.GetDirectoryName(path);
            }
        }

        public static bool IsBelowManagedPath(string fullPath)
        {
            string repositoryPath;
            return TryGetRepositoryRoot(fullPath, out repositoryPath);
        }

        public static bool PathMatches(string rootPath, string path, bool isSubTree, GitDepth depth)
        {
            if (rootPath == null)
                throw new ArgumentNullException("rootPath");
            if (path == null)
                throw new ArgumentNullException("path");

            if (String.Equals(rootPath, path, FileSystemUtil.StringComparison))
                return true;

            if (rootPath.Length > 0 && rootPath[rootPath.Length - 1] != '/')
                rootPath += '/';

            if (depth < GitDepth.Empty)
                throw new NotImplementedException();
            if (depth == GitDepth.Empty)
                return false;

            // If we do not recurse and we have a directory separater after
            // the root is long, we're sure we can skip it. Note we do not check
            // whether the path is actually of the root path because that's checked
            // later on.

            if (depth == GitDepth.Files && path.LastIndexOf('/') > rootPath.Length)
                return false;

            if (isSubTree)
                return rootPath.StartsWith(path, FileSystemUtil.StringComparison);
            else
                return path.StartsWith(rootPath, FileSystemUtil.StringComparison);
        }

        internal static Dictionary<RepositoryEntry, ICollection<string>> CollectPaths(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            var result = new Dictionary<RepositoryEntry, ICollection<string>>();

            foreach (string path in paths)
            {
                var repositoryEntry = RepositoryManager.GetRepository(path);

                if (repositoryEntry != null)
                {
                    ICollection<string> repositoryPaths;

                    if (!result.TryGetValue(repositoryEntry, out repositoryPaths))
                    {
                        repositoryPaths = new List<string>();
                        result.Add(repositoryEntry, repositoryPaths);
                    }

                    repositoryPaths.Add(repositoryEntry.Repository.GetRepositoryPath(path));
                }
            }

            return result;
        }

        public static GitBranchRef GetCurrentBranch(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            var repositoryEntry = RepositoryManager.GetRepository(repositoryPath);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            using (repositoryEntry.Lock())
            {
                return new GitBranchRef(repositoryEntry.Repository.GetFullBranch());
            }
        }
    }
}
