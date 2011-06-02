using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit;
using NGit.Revwalk;
using NGit.Dircache;
using NGit.Treewalk;
using System.IO;

namespace SharpGit
{
    internal sealed class GitRevertCommand : GitCommand<GitRevertArgs>
    {
        public GitRevertCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            var collectedPaths = RepositoryUtil.CollectPaths(paths);

            foreach (var item in collectedPaths)
            {
                lock (item.Key.SyncLock)
                {
                    UnstageAndRestoreFiles(item.Key.Repository, item.Value);
                }
            }

            foreach (string path in paths)
            {
                RaiseNotify(new GitNotifyEventArgs
                {
                    Action = GitNotifyAction.Revert,
                    CommandType = Args.CommandType,
                    FullPath = path,
                    NodeKind = GitNodeKind.File,
                    ContentState = GitNotifyState.Unknown
                });
            }
        }

        private void UnstageAndRestoreFiles(Repository repository, IEnumerable<string> files)
        {
            // Make sure all files are unstaged.

            UnstageFiles(repository, files);

            // Take an appropriate action per file.

            var dirCache = repository.ReadDirCache();
            var objectReader = repository.NewObjectReader();

            try
            {
                foreach (var path in files)
                {
                    // When the entry is not in the disk cache, it's a new
                    // entry so it can just be deleted.

                    string fullPath = repository.GetAbsoluteRepositoryPath(path);

                    var entryIndex = dirCache.FindEntry(path);

                    if (entryIndex < 0)
                    {
                        File.Delete(fullPath);
                    }
                    else
                    {
                        // When it is in the disk cache, we need to overwrite
                        // the current contents with that of the disk cache.

                        var entry = dirCache.GetEntry(entryIndex);

                        var loader = objectReader.Open(entry.GetObjectId());
                        using (var inStream = new ObjectStreamWrapper(loader.OpenStream()))
                        using (var outStream = File.Create(fullPath))
                        {
                            inStream.CopyTo(outStream);
                        }
                    }
                }
            }
            finally
            {
                objectReader.Release();
            }
        }

        private void UnstageFiles(Repository repository, IEnumerable<string> files)
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
                    throw new GitCouldNotLockException();

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
                throw new GitException(GitErrorCode.Unspecified, e);
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
