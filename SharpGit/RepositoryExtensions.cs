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
