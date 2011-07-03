// SharpGit\GitBranchCommand.cs
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
using NGit.Api;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitBranchCommand : GitCommand<GitBranchArgs>
    {
        public GitBranchCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, string branchName)
        {
            if (branchName == null)
                throw new ArgumentNullException("branchName");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var command = new Git(repository).BranchCreate();

                command.SetForce(Args.Force);
                command.SetName(branchName);
                
                if (Args.Revision != null)
                {
                    var revWalk = new RevWalk(repository);

                    try
                    {
                        command.SetStartPoint(revWalk.ParseCommit(Args.Revision.GetObjectId(repository)));
                    }
                    finally
                    {
                        revWalk.Release();
                    }
                }

                command.Call();
            }
        }
    }
}
