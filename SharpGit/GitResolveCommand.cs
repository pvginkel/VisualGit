// SharpGit\GitResolveCommand.cs
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
using NGit.Dircache;
using NGit.Api;
using System.Diagnostics;
using NGit;
using System.IO;

namespace SharpGit
{
    internal class GitResolveCommand : GitCommand<GitResolveArgs>
    {
        public GitResolveCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string fullPath, GitAccept accept)
        {
            Debug.Assert(Args.Depth == GitDepth.Empty);

            if (accept == GitAccept.Merged)
            {
                MarkMerged(fullPath);
            }
            else
            {
                var repositoryEntry = Client.GetRepository(fullPath);

                GitConflictEventArgs args = Args.ConflictArgs;
                bool requireCleanup = false;

                if (args == null)
                {
                    args = new GitConflictEventArgs
                    {
                        MergedFile = fullPath,
                        Path = repositoryEntry.Repository.GetRepositoryPath(fullPath),
                        ConflictReason = GitConflictReason.Edited
                    };

                    requireCleanup = true;
                }

                try
                {
                    using (repositoryEntry.Lock())
                    {
                        var repository = repositoryEntry.Repository;

                        switch (accept)
                        {
                            case GitAccept.Base:
                                SelectAndMarkMerged(repository, args, args.BaseFile);
                                break;

                            case GitAccept.MineFull:
                                SelectAndMarkMerged(repository, args, args.MyFile);
                                break;

                            case GitAccept.TheirsFull:
                                SelectAndMarkMerged(repository, args, args.TheirFile);
                                break;
                        }
                    }
                }
                finally
                {
                    if (requireCleanup)
                        args.Cleanup();
                }
            }
        }

        private void SelectAndMarkMerged(Repository repository, GitConflictEventArgs args, string path)
        {
            File.Copy(path, args.MergedFile, true);

            MarkMerged(args.MergedFile);
        }

        private void MarkMerged(string fullPath)
        {
            Client.Add(fullPath, new GitAddArgs { Update = true });
        }
    }
}
