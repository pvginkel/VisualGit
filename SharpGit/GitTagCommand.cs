// SharpGit\GitTagCommand.cs
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
using NGit;

namespace SharpGit
{
    internal class GitTagCommand : GitCommand<GitTagArgs>
    {
        public GitTagCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, string tagName)
        {
            if (tagName == null)
                throw new ArgumentNullException("tagName");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var revWalk = new RevWalk(repository);
                try
                {
                    var revision = Args.Revision ?? GitRevision.Head;
                    var commit = revWalk.ParseCommit(revision.GetObjectId(repository));


                    if (Args.AnnotatedTag)
                    {
                        var command = new Git(repository).Tag();

                        command.SetMessage(Args.Message);
                        command.SetForceUpdate(Args.Force);
                        command.SetName(tagName);

                        command.SetObjectId(commit);

                        command.Call();
                    }
                    else
                    {
                        // TagCommand is not smart enough to support not annotated
                        // tags. Create a tag ourself.

                        var tagRef = repository.UpdateRef(Constants.R_TAGS + tagName);

                        tagRef.SetNewObjectId(commit.ToObjectId());
                        tagRef.SetForceUpdate(Args.Force);
                        tagRef.SetRefLogMessage("tagged " + tagName, false);

                        tagRef.Update(revWalk);
                    }
                }
                finally
                {
                    revWalk.Release();
                }
            }
        }
    }
}
