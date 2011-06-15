using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using NGit;

namespace SharpGit
{
    internal sealed class IgnoreManager
    {
        private RepositoryEntry _entry;
        private string _repositoryPath;
        private Dictionary<string, IgnoreFile> _ignoreFiles = new Dictionary<string, IgnoreFile>(FileSystemUtil.StringComparer);

        public IgnoreManager(RepositoryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            _entry = entry;

            _repositoryPath = entry.Path.TrimEnd(Path.DirectorySeparatorChar);
        }

        public void RefreshPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            Debug.Assert(
                (path + Path.DirectorySeparatorChar).StartsWith(_repositoryPath + Path.DirectorySeparatorChar, FileSystemUtil.StringComparison),
                "Path is not part of the repository"
            );

            RefreshPathCore(path);
        }

        private void RefreshPathCore(string path)
        {
            if (Directory.Exists(path))
            {
                string ignorePath = Path.Combine(path, Constants.DOT_GIT_IGNORE);

                if (File.Exists(ignorePath))
                {
                    IgnoreFile file;
                    if (_ignoreFiles.TryGetValue(path, out file))
                        file.Refresh();
                    else
                        _ignoreFiles[path] = new IgnoreFile(path);
                }
                else
                {
                    _ignoreFiles.Remove(path);
                }
            }

            if (!path.Equals(_repositoryPath, FileSystemUtil.StringComparison))
                RefreshPathCore(Path.GetDirectoryName(path));
        }

        public bool IsIgnored(string fullPath, GitNodeKind kind)
        {
            string path;

            if (kind == GitNodeKind.File)
                path = Path.GetDirectoryName(fullPath);
            else if (kind == GitNodeKind.Directory)
                path = fullPath;
            else
                throw new ArgumentOutOfRangeException("kind");

            Debug.Assert(
                (path + Path.DirectorySeparatorChar).StartsWith(_repositoryPath + Path.DirectorySeparatorChar, FileSystemUtil.StringComparison),
                "Path is not part of the repository"
            );

            bool ignored = false;

            while (true)
            {
                IgnoreFile file;

                if (_ignoreFiles.TryGetValue(path, out file))
                {
                    string relativePath = fullPath.Substring(path.Length).Replace(Path.DirectorySeparatorChar, '/');

                    foreach (var rule in file.Rules)
                    {
                        if (rule.IsMatch(relativePath, kind == GitNodeKind.Directory))
                            ignored = !rule.GetNegation();
                    }
                }

                if (path.Equals(_repositoryPath, FileSystemUtil.StringComparison))
                    return ignored;

                path = Path.GetDirectoryName(path);
            }
        }
    }
}
