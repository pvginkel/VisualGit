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


        public static void UnstageFiles(this Repository repository, IEnumerable<string> files)
        {
            try
            {
                RepositoryState state = repository.GetRepositoryState();

                // resolve the ref to a commit
                ObjectId commitId;

                commitId = repository.Resolve(Constants.HEAD);

                RevCommit commit;
                RevWalk rw = new RevWalk(repository);
                try
                {
                    commit = rw.ParseCommit(commitId);
                }
                finally
                {
                    rw.Release();
                }

                // write the ref
                RefUpdate ru = repository.UpdateRef(Constants.HEAD);

                ru.SetNewObjectId(commitId);

                string refName = Repository.ShortenRefName(Constants.HEAD);
                string message = refName + ": updating " + Constants.HEAD;

                ru.SetRefLogMessage(message, false);

                if (ru.ForceUpdate() == RefUpdate.Result.LOCK_FAILURE)
                    throw new JGitInternalException("Cannot lock ref");

                var fileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string file in files)
                    fileSet.Add(file);
                var fileMap = new Dictionary<string, DirCacheEntry>();

                DirCache dc = null;
                try
                {
                    var dirCache = repository.ReadDirCache();
                    dc = repository.LockDirCache();
                    dc.Clear();
                    DirCacheBuilder dcb = dc.Builder();

                    var objectReader = repository.NewObjectReader();

                    try
                    {
                        // Get all dir cache entries from the last commit tree
                        // to restore them later on.

                        TreeWalk tw = new TreeWalk(objectReader);

                        tw.AddTree(new CanonicalTreeParser(new byte[0], objectReader, commit.Tree.ToObjectId()));
                        tw.Recursive = true;

                        while (tw.Next())
                        {
                            var entry = CreateDirCacheEntry(tw);
                            if (fileSet.Contains(entry.PathString))
                                fileMap.Add(entry.PathString, entry);
                        }

                        // Walk over the current dir cache; remove what doesn't
                        // exist and put back in what we've lost.

                        for (int i = 0, entryCount = dirCache.GetEntryCount(); i < entryCount; i++)
                        {
                            var entry = dirCache.GetEntry(i);

                            if (fileSet.Contains(entry.PathString))
                            {
                                // If the entry was in the old dir cache, we can
                                // just add it again. Otherwise, we leave it out.
                                DirCacheEntry oldEntry;
                                if (fileMap.TryGetValue(entry.PathString, out oldEntry))
                                {
                                    // The file is in both the old and the new dir cache.
                                    // Replace the new entry with the old one.
                                    fileMap.Remove(entry.PathString);
                                    dcb.Add(oldEntry);
                                }
                            }
                            else
                                dcb.Add(entry);
                        }

                        // Last, put in all entries that were in the old dir
                        // cache but not in the new one.

                        foreach (var entry in fileMap.Values)
                        {
                            dcb.Add(entry);
                        }
                    }
                    finally
                    {
                        objectReader.Release();
                    }

                    dcb.Commit();
                }
                finally
                {
                    if (dc != null)
                        dc.Unlock();
                }
            }

            catch (IOException e)
            {
                throw new JGitInternalException("Exception during reset command", e);
            }
        }

        private static DirCacheEntry CreateDirCacheEntry(TreeWalk tw)
        {
            DirCacheEntry e = new DirCacheEntry(tw.RawPath, 0);
            AbstractTreeIterator i;
            i = tw.GetTree<AbstractTreeIterator>(0);
            e.FileMode = tw.GetFileMode(0);
            e.SetObjectIdFromRaw(i.IdBuffer, i.IdOffset);
            return e;
        }
    }
}
