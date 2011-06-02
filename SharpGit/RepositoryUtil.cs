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
        public static string GetRepositoryRoot(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            path = Path.GetFullPath(path);
            string pathRoot = Path.GetPathRoot(path);

            while (true)
            {
                if (Directory.Exists(Path.Combine(path, Constants.DOT_GIT)))
                    return path;

                if (
                    String.Empty.Equals(path) ||
                    String.Equals(path, pathRoot, FileSystemUtil.StringComparison)
                )
                    return null;

                path = Path.GetDirectoryName(path);
            }
        }

        public static bool IsBelowManagedPath(string fullPath)
        {
            return GetRepositoryRoot(fullPath) != null;
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
    }
}
