// SharpGit\GitExportCommand.cs
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
using NGit.Revwalk;
using NGit.Treewalk;
using NGit.Dircache;
using System.IO;

namespace SharpGit
{
    internal class GitExportCommand : GitCommand<GitExportArgs>
    {
        public GitExportCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(GitTarget target, string targetPath)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (targetPath == null)
                throw new ArgumentNullException("targetPath");

            var repositoryEntry = Client.GetRepository(target.FullPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var checkoutRoot = new Sharpen.FilePath(targetPath);
                var revWalk = new RevWalk(repository);
                var objectReader = repository.NewObjectReader();
                var revision = Args.Revision ?? GitRevision.Head;
                var tw = new TreeWalk(objectReader);

                tw.Filter = new CustomPathFilter(repository.GetRepositoryPath(target.FullPath), Args.Depth);

                try
                {
                    AbstractTreeIterator ti;

                    if (revision == GitRevision.Working)
                        ti = new FileTreeIterator(repository);
                    else
                        ti = new CanonicalTreeParser(new byte[0], objectReader, revWalk.ParseCommit(revision.GetObjectId(repository)).Tree.ToObjectId());

                    tw.AddTree(ti);
                    tw.Recursive = true;

                    while (tw.Next())
                    {
                        var entry = CreateDirCacheEntry(tw);

                        var f = new Sharpen.FilePath(checkoutRoot, entry.PathString);
                        
                        f.GetParentFile().Mkdirs();

                        if (revision == GitRevision.Working)
                            File.Copy(repository.GetAbsoluteRepositoryPath(tw.PathString), f, true);
                        else
                            DirCacheCheckout.CheckoutEntry(repository, f, entry);
                    }
                }
                finally
                {
                    tw.Release();
                    revWalk.Release();
                    objectReader.Release();
                }
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
