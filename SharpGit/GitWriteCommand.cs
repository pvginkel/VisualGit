// SharpGit\GitWriteCommand.cs
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
using NGit;
using NGit.Revwalk;
using NGit.Treewalk;

namespace SharpGit
{
    internal sealed class GitWriteCommand : GitCommand<GitWriteArgs>
    {
        public GitWriteCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(GitTarget path, Stream stream)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (stream == null)
                throw new ArgumentNullException("stream");

            var repositoryEntry = Client.GetRepository(path.FullPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                if (Args.Revision == GitRevision.Working)
                    WriteWorkingRevision(repository, stream, path.FullPath);
                else if (Args.Revision == GitRevision.Base)
                    WriteBaseRevision(repository, stream, path.FullPath);
                else if (Args.Revision.RevisionType == GitRevisionType.Hash)
                    WriteSpecificRevision(repository, stream, path.FullPath, Args.Revision);
                else
                    throw new NotImplementedException();
            }
        }

        private void WriteSpecificRevision(Repository repository, Stream stream, string path, GitRevision revision)
        {
            var objectId = revision.GetObjectId(repository);

            if (objectId == null)
                throw new GitRevisionNotFoundException(Properties.Resources.RevisionNotFound);

            string relativePath = repository.GetRepositoryPath(path);

            var objectReader = repository.NewObjectReader();

            try
            {
                RevWalk revWalk = new RevWalk(repository);

                try
                {
                    TreeWalk startWalk = new TreeWalk(revWalk.GetObjectReader());

                    try
                    {
                        startWalk.AddTree(revWalk.ParseCommit(objectId).Tree);
                        startWalk.Recursive = true;

                        bool found = false;

                        while (startWalk.Next())
                        {
                            if (String.Equals(startWalk.PathString, relativePath, FileSystemUtil.StringComparison))
                            {
                                LoadObject(objectReader, startWalk.GetObjectId(0), stream);

                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            // TODO: We currently do not track renames.
                        }
                    }
                    finally
                    {
                        startWalk.Release();
                    }
                }
                finally
                {
                    revWalk.Release();
                }
            }
            finally
            {
                objectReader.Release();
            }
        }

        private void LoadObject(ObjectReader objectReader, ObjectId pathObjectId, Stream stream)
        {
            var loader = objectReader.Open(pathObjectId);

            using (var inStream = new ObjectStreamWrapper(loader.OpenStream()))
            {
                inStream.CopyTo(stream);
            }
        }

        private void WriteWorkingRevision(Repository repository, Stream stream, string path)
        {
            using (var inStream = File.OpenRead(path))
            {
                inStream.CopyTo(stream);
            }
        }

        private void WriteBaseRevision(Repository repository, Stream stream, string path)
        {
            var dirCache = repository.ReadDirCache();
            var objectReader = repository.NewObjectReader();

            try
            {
                string relativePath = repository.GetRepositoryPath(path);

                var entryIndex = dirCache.FindEntry(relativePath);

                if (entryIndex >= 0)
                {
                    // When it is in the disk cache, we need to overwrite
                    // the current contents with that of the disk cache.

                    var entry = dirCache.GetEntry(entryIndex);

                    LoadObject(objectReader, entry.GetObjectId(), stream);
                }
            }
            finally
            {
                objectReader.Release();
            }
        }
    }
}
