using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpGit;
using VisualGit.Services.StatusManager;

namespace VisualGit.Services
{
    [GlobalService(typeof(IGitStatusManager))]
    sealed class VisualGitStatusManager : VisualGitService, IGitStatusManager
    {
        private readonly Dictionary<string, RepositoryCache> _cache = new Dictionary<string, RepositoryCache>(FileSystemUtil.StringComparer);
        private readonly object _syncRoot = new object();

        public VisualGitStatusManager(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        public bool InvalidatePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var cache = GetCache(path);

            if (cache != null)
                cache.Invalidate();

            return cache != null;
        }

        public bool GetFileStatus(string path, GitDepth depth, Func<GitFileStatus, bool> callback)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (callback == null)
                throw new ArgumentNullException("callback");

            var cache = GetCache(path);

            if (cache != null)
                cache.GetFileStatus(path, depth, callback);

            return cache != null;
        }

        private RepositoryCache GetCache(string path)
        {
            lock (_syncRoot)
            {
                path = Path.GetFullPath(path);
                string pathRoot = Path.GetPathRoot(path);

                while (true)
                {
                    RepositoryCache cache;

                    if (_cache.TryGetValue(path, out cache))
                        return cache;

                    if (Directory.Exists(Path.Combine(path, ".git")))
                    {
                        cache = new RepositoryCache(path);
                        _cache.Add(path, cache);

                        return cache;
                    }

                    if (
                        String.IsNullOrEmpty(path) ||
                        String.Equals(path, pathRoot, FileSystemUtil.StringComparison)
                    )
                        return null;

                    path = Path.GetDirectoryName(path);
                }
            }
        }
    }
}
