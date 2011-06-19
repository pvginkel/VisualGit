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
            string result;

            if (!TryGetRepositoryRoot(path, out result))
                throw new GitNoRepositoryException();

            return result;
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

        internal static bool PathMatches(string rootPath, string path, bool isSubTree, GitDepth depth)
        {
            if (rootPath == null)
                throw new ArgumentNullException("rootPath");
            if (path == null)
                throw new ArgumentNullException("path");

            if (depth < GitDepth.Empty)
                throw new NotImplementedException();

            // Path equals to rootPath always consitutes a match.

            if (String.Equals(rootPath, path, FileSystemUtil.StringComparison))
                return true;

            if (isSubTree && path[path.Length - 1] != '/')
                path += '/';

            // Check whether we recurse into a sub tree. This checks whether
            // path is part of rootPath and only matches up to the actual folder.
            // This does not depend on Depth because if we wouldn't match this,
            // we wouldn't even get to the file.

            if (isSubTree && rootPath.StartsWith(path, FileSystemUtil.StringComparison))
                return true;

            // If we didn't match the exact file and we're not descending into
            // the folder of the exact file, we must be iterating files or
            // children to match anything beyond this point.

            if (depth <= GitDepth.Empty)
                return false;

            // Treat the rootPath like a directory. If it was a file and we'd
            // have a match, it would have already been matched by the
            // String.Equals above, so all matches below fail. Otherwise,
            // this allows us to correctly match all sub folders.

            if (rootPath.Length > 0 && rootPath[rootPath.Length - 1] != '/')
                rootPath += '/';

            // Here we check whether we're in a sub folder when we're only
            // matching the files of a specific folder.
            // If we do not recurse and we have a directory separater after
            // the root is long, we're sure we can skip it. Note we do not check
            // whether the path is actually of the root path because that's checked
            // later on.

            if (depth == GitDepth.Files && path.LastIndexOf('/') > rootPath.Length)
                return false;

            // Last, check whether the matching file is located in or below
            // the root directory.

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
    }
}
